#include "context.h"

#include <map>
#include <sstream>

namespace {

struct AsyncHandleData {
  std::function<void(void)> callback_;
  std::function<void(void)> release_callback_;
};

void asyncCallback(uv_async_t* handle) {
  auto data = static_cast<AsyncHandleData*>(handle->data);
  data->callback_();
}

void asyncReleaseCallback(uv_handle_t* handle) {
  auto data = static_cast<AsyncHandleData*>(handle->data);
  data->release_callback_();
}
}  // namespace

namespace coreclrhosting {

thread_local Context* Context::ThreadInstance::thread_instance_;

Context::Context(std::unique_ptr<DotNetHost> dotnet_host, Napi::Env env)
    : env_(env),
      release_called_(false),
      host_(std::move(dotnet_host)),
      function_factory_(
          std::bind(&Context::CreateFunction, this, std::placeholders::_1)) {
  auto async_data = new AsyncHandleData();
  async_data->callback_ = [this]() { this->UvAsyncCallback(); };
  async_data->release_callback_ = [this]() { delete this; };
  async_handle_.data = async_data;
  uv_async_init(uv_default_loop(), &async_handle_, &asyncCallback);
}
Context::~Context() {
  ThreadInstance _(this);
  host_.reset(nullptr);  // Explicit reset while having thread instance set
}

void Context::UvAsyncCallback() {
  ThreadInstance _(this);
  while (true) {
    netCallbacks_t callbacks;
    {
      std::lock_guard<std::mutex> lock(mutex_);
      if (dotnet_callbacks_.empty())
        break;
      else
        callbacks.swap(dotnet_callbacks_);
    }

    for (const auto& callback : callbacks) {
      // printf("Executing callback\n");
      callback.first(callback.second);
    }
  }

  if (release_called_) {
    uv_close(reinterpret_cast<uv_handle_t*>(&async_handle_),
             &asyncReleaseCallback);
  }
}

void Context::PostCallback(netCallback_t callback, void* data) {
  {
    std::lock_guard<std::mutex> lock(mutex_);
    dotnet_callbacks_.push_back({callback, data});
  }
  uv_async_send(&async_handle_);
}

void Context::Release() {
  {
    std::lock_guard<std::mutex> lock(mutex_);
    release_called_ = true;
  }
  uv_async_send(&async_handle_);
}

Napi::Value Context::RunCoreApp(const Napi::CallbackInfo& info) {
  Napi::Env env = info.Env();

  if (info.Length() != 2 || !info[0].IsString() ||
      !info[1].IsString() /*|| !info[2].IsFunction()*/) {
    Napi::Error::New(env,
                     "Expected path and assembly name in call to runCoreApp.")
        .ThrowAsJavaScriptException();
    return Napi::Value();
  }

  std::unique_ptr<DotNetHost> host;

  auto result = DotNetHost::Create(info[0].ToString(), host);
  switch (result) {
    case DotNetHostCreationResult::kOK:
      break;
    case DotNetHostCreationResult::kCoreClrNotFound: {
      std::ostringstream stringStream;
      stringStream << "Could not find coreclr at given base path: "
                   << (std::string)info[0].ToString();
      Napi::Error::New(env, stringStream.str()).ThrowAsJavaScriptException();
      return Napi::Value();
    }
    case DotNetHostCreationResult::kInvalidCoreClr:
      Napi::Error::New(env,
                       "The coreclr found at base path is invalid. Probably "
                       "incompatible version")
          .ThrowAsJavaScriptException();
      return Napi::Value();
    case DotNetHostCreationResult::kInitializeFailed:
      Napi::Error::New(env, "Failed to initialize the coreclr runtime.")
          .ThrowAsJavaScriptException();
      return Napi::Value();
    default:
      Napi::Error::New(env, "Unexpected error while creating coreclr host.")
          .ThrowAsJavaScriptException();
      return Napi::Value();
  }

  // Set up context on current thread
  auto context = new Context(std::move(host), env);
  ThreadInstance _(context);

  unsigned int exit_code = -1;
  const char* argv[1] = {"Name"};
  auto execute_result =
      context->host_->ExecuteAssembly(info[1].ToString(), 1, argv, exit_code);
  switch (execute_result) {
    case coreclrhosting::DotNetHostExecuteAssemblyResult::kOK:
      return Napi::Number::New(env, exit_code);
    case coreclrhosting::DotNetHostExecuteAssemblyResult::kAssemblyNotFound:
      context->Release();
      Napi::Error::New(env, "Could not find assembly to execute.")
          .ThrowAsJavaScriptException();
      return Napi::Value();
    default:
      context->Release();
      Napi::Error::New(env, "Unexpected error while executing assembly.")
          .ThrowAsJavaScriptException();
      return Napi::Value();
  }
}

JsHandle Context::GetMember(JsHandle owner_handle, const char* name) {
  if (!owner_handle.IsObject())
    return JsHandle::Error("Only objects support GetMember");
  if (!IsActiveContext())
    return JsHandle::Error("Must be called on node thread");

  Napi::HandleScope handleScope(env_);

  auto owner = owner_handle.ToValue(env_);
  // std::assert(owner.IsObject());
  auto owner_object = owner.ToObject();
  auto result = owner_object.Get(name);

  return JsHandle::FromValue(result);
}
JsHandle Context::SetMember(JsHandle owner_handle, const char* name,
                            DotNetHandle dotnet_handle) {
  if (!owner_handle.IsObject())
    return JsHandle::Error("Only objects support GetMember");
  if (!IsActiveContext())
    return JsHandle::Error("Must be called on node thread");

  Napi::HandleScope handleScope(env_);

  auto owner = owner_handle.ToValue(env_);
  auto owner_object = owner.ToObject();
  auto value = dotnet_handle.ToValue(env_, function_factory_);
  dotnet_handle.Release();
  owner_object.Set(name, value);
  return JsHandle::FromValue(value);
}
JsHandle Context::CreateObject(JsHandle prototype_function, int argc,
                               DotNetHandle* argv) {
  if (!IsActiveContext())
    return JsHandle::Error("Must be called on node thread");

  Napi::HandleScope handleScope(env_);

  if (prototype_function.IsNotNullFunction()) {
    std::vector<napi_value> arguments(argc);
    for (int c = 0; c < argc; c++) {
      arguments[c] = argv[c].ToValue(env_, function_factory_);
      argv[c].Release();
    }

    napi_value result;
    auto status = napi_new_instance(env_, prototype_function.ToValue(env_),
                                    argc, arguments.data(), &result);
    if (status != napi_ok) {
      return JsHandle::Error("Could not create instance");
    }
    return JsHandle::FromValue(Napi::Value(env_, result));
  }

  auto newObj = Napi::Object::New(env_);

  return JsHandle(newObj);
}
JsHandle Context::Invoke(JsHandle handle, JsHandle receiver_handle, int argc,
                         DotNetHandle* argv) {
  if (!IsActiveContext())
    return JsHandle::Error("Must be called on node thread");

  Napi::HandleScope handleScope(env_);

  auto function = handle.ToValue(env_).As<Napi::Function>();
  std::vector<napi_value> arguments(argc);
  for (int c = 0; c < argc; c++) {
    arguments[c] = argv[c].ToValue(env_, function_factory_);
    // printf("Releasing ptr %p \n",
    // reinterpret_cast<u_int64_t>(argv[c].value_));
    argv[c].Release();
  }

  auto result = function.MakeCallback(receiver_handle.ToValue(env_), arguments);
  return JsHandle::FromValue(result);
}

Napi::Function Context::CreateFunction(DotNetHandle* handle) {
  auto release_func = handle->release_func_;
  auto function_value = handle->function_value_;
  handle->release_func_ = nullptr;  // We delay the release

  // TODO: Check if using data instead of capture is better
  auto function = Napi::Function::New(
      env_, [this, function_value](const Napi::CallbackInfo& info) {
        ThreadInstance _(this);
        auto argc = info.Length();
        std::vector<JsHandle> arguments;
        for (auto c = 0; c < argc; c++) {
          arguments.push_back(JsHandle::FromValue(info[c]));
        }

        DotNetHandle resultIntern;
        (*function_value)(argc, arguments.data(), resultIntern);

        auto napiResultValue =
            resultIntern.ToValue(info.Env(), this->function_factory_);
        resultIntern.Release();
        return napiResultValue;
      });

  auto releaseCopy = new DotNetHandle;
  releaseCopy->type_ = DotNetType::Function;
  releaseCopy->function_value_ = function_value;
  releaseCopy->release_func_ = release_func;

  napi_add_finalizer(
      env_, function, (void*)releaseCopy,
      [](napi_env env, void* finalize_data, void* finalize_hintnapi_env) {
        auto toRelease = (DotNetHandle*)finalize_data;
        toRelease->Release();
        delete toRelease;
      },
      nullptr, nullptr);

  return function;
}

}  // namespace coreclrhosting