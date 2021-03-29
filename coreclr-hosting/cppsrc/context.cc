#include "context.h"

#include <map>
#include <sstream>
#include <string>
#include <vector>

#include "nativeapi.h"

namespace {

const size_t MAX_ARGUMENTS_ON_STACK = 6;

Napi::Value Noop(const Napi::CallbackInfo& info) {
  return info.Env().Undefined();
}

}  // namespace

namespace coreclrhosting {

class Context::SynchronizedFinalizerCallback {
  Context* context_;
  std::shared_ptr<std::mutex> mutex_;
  std::function<void()> callback_;

 public:
  SynchronizedFinalizerCallback(Context* context,
                                std::function<void()> callback);
  void Call();
  void Cancel();
  static void Wrapper(napi_env env, void* finalize_data);
};

Context::SynchronizedFinalizerCallback::SynchronizedFinalizerCallback(
    Context* context, std::function<void()> callback) {
  context_ = context;
  mutex_ = context->finalizer_mutex_;
  callback_ = callback;
  context->function_finalizers_.insert(this);
}

void Context::SynchronizedFinalizerCallback::Wrapper(napi_env env,
                                                     void* finalize_data) {
  auto data = (SynchronizedFinalizerCallback*)finalize_data;
  data->Call();
  delete data;
}

void Context::SynchronizedFinalizerCallback::Call() {
  std::lock_guard<std::mutex> lock(*mutex_);

  if (context_) {
    context_->function_finalizers_.erase(this);
    context_ = nullptr;
    callback_();
  }
}

void Context::SynchronizedFinalizerCallback::Cancel() { context_ = nullptr; }

thread_local Context* Context::ThreadInstance::thread_instance_;

void Context::DeleteContext(Napi::Env env, Context* context) { delete context; }

Context::Context(std::unique_ptr<DotNetHost> dotnet_host, Napi::Env env)
    : env_(env),
      finalizer_mutex_(std::make_shared<std::mutex>()),
      host_(std::move(dotnet_host)),
      function_factory_(
          std::bind(&Context::CreateFunction, this, std::placeholders::_1)),
      array_buffer_factory_(
          std::bind(&Context::CreateArrayBuffer, this, std::placeholders::_1)),
      process_event_loop_(nullptr),
      dotnet_thread_safe_callback_(Napi::ThreadSafeFunction::New(
          env, Napi::Function::New(env, Noop, "invokeDotNetEventLoop"),
          "invokeDotNetEventLoop", 0, 1)),
      closing_runtime_(nullptr) {
  env.SetInstanceData<Context, Context::DeleteContext>(this);
}
Context::~Context() {
  ThreadInstance _(this);
  std::lock_guard<std::mutex> lock(*finalizer_mutex_);
  for (auto finalizer_data : function_finalizers_) {
    finalizer_data->Cancel();
  }
  host_.reset(nullptr);  // Explicit reset while having thread instance set
}

void Context::RegisterSchedulerCallbacks(void (*process_event_loop)(void*),
                                         void (*process_micro_task)(void*),
                                         void (*closing_runtime)(void)) {
  closing_runtime_ = closing_runtime;
  Napi::HandleScope handle_scope(env_);

  process_event_loop_ = [this, process_event_loop](Napi::Env env,
                                                   Napi::Function jsCallback,
                                                   void* data) {
    // jsCallback == Noop, so we do not call it
    ThreadInstance _(this);

    // Napi::HandleScope handle_scope(env_);
    // Napi::AsyncContext async_context(env_, "dotnet scheduler");

    // We use a new scope per callback, so microtasks can be run after
    // each callback (empty dotnet stack)
    // Napi::CallbackScope cb_scope(env_, async_context);

    (*process_event_loop)(data);

    // DM 25.04.2020: Running them manually was necessary when using libuv
    // directly. But with ThreadSafeFunction this is no longer required
    // auto isolate = v8::Isolate::GetCurrent();
    // // v8::MicrotasksScope::PerformCheckpoint(isolate); Does not run
    // // microtasks
    // isolate->RunMicrotasks();
  };

  /*process_micro_task_ = [this,
                         process_micro_task](const Napi::CallbackInfo& f_info) {
    ThreadInstance _(this);
    (*process_micro_task)(f_info.Data());
    return f_info.Env().Undefined();
  };*/

  process_micro_task_ = Napi::Persistent(Napi::Function::New(
      env_,
      [this, process_micro_task](const Napi::CallbackInfo& f_info) {
        ThreadInstance _(this);
        (*process_micro_task)(nullptr);
        return f_info.Env().Undefined();
      },
      "invokeDotnetMicrotask"));

  auto global = env_.Global();
  auto queue_microtask = global.Get("queueMicrotask").As<Napi::Function>();
  signal_micro_task_ = Napi::Persistent(queue_microtask);
}

void Context::SignalEventLoopEntry(void* data) {
  auto acquire_status = dotnet_thread_safe_callback_.Acquire();
  if (acquire_status != napi_ok) return;

  // This will never block, as we used an unlimited queue
  dotnet_thread_safe_callback_.BlockingCall(data, process_event_loop_);

  dotnet_thread_safe_callback_.Release();
}

void Context::SignalMicroTask(void* data) {
  /*auto bound_func = Napi::Function::New(env_, process_micro_task_,
                                        "invokeDotnetMicrotask", data);
  signal_micro_task_.Value().Call(env_.Global(), {bound_func});*/
  signal_micro_task_.Value().Call(env_.Global(), {process_micro_task_.Value()});
}

extern "C" typedef void (*EntryPointFunction)(Context* context,
                                              NativeApi& nativeApi, int argc,
                                              const char* argv[],
                                              DotNetHandle* resultValue);

Napi::Value Context::RunCoreApp(const Napi::CallbackInfo& info) {
  Napi::Env env = info.Env();

  if (info.Length() < 1 || !info[0].IsString()) {
    Napi::Error::New(env, "Expected path to assembly as first argument")
        .ThrowAsJavaScriptException();
    return Napi::Value();
  }

  std::vector<std::string> arguments(info.Length());
  std::vector<const char*> arguments_c(info.Length());
  for (auto i = 0u; i < info.Length(); i++) {
    if (!info[i].IsString()) {
      Napi::Error::New(env, "Expected only string arguments")
          .ThrowAsJavaScriptException();
      return Napi::Value();
    }
    arguments_c[i] = (arguments[i] = info[i].ToString()).c_str();
  }

  std::unique_ptr<DotNetHost> host;

  auto result = DotNetHost::Create(info[0].ToString(), host);
  switch (result) {
    case DotNetHostCreationResult::kOK:
      break;
    case DotNetHostCreationResult::kAssemblyNotFound: {
      std::ostringstream stringStream;
      stringStream << "Could not find the assembly at: "
                   << (std::string)info[0].ToString();
      Napi::Error::New(env, stringStream.str()).ThrowAsJavaScriptException();
      return Napi::Value();
    }
    case DotNetHostCreationResult::kCoreClrNotFound:
      Napi::Error::New(env, "The coreclr could not be found")
          .ThrowAsJavaScriptException();
      return Napi::Value();
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

  // TODO DM 20.03.2020: Move into method on context
  auto entry_point_ptr = context->host_->GetManagedFunction(
      "NodeHostEnvironment.NativeHost.NativeEntryPoint, NodeHostEnvironment",
      "RunHostedApplication",
      "NodeHostEnvironment.NativeHost.EntryPointSignature, "
      "NodeHostEnvironment");
  auto entry_point = reinterpret_cast<EntryPointFunction>(entry_point_ptr);

  DotNetHandle return_handle;
  entry_point(context, NativeApi::instance_, arguments_c.size(),
              arguments_c.data(), &return_handle);

  auto return_value = return_handle.ToValue(env, context->function_factory_,
                                            context->array_buffer_factory_);

  if (return_value.IsPromise()) {
    return return_value.As<Napi::Promise>()
        .Get("then")
        .As<Napi::Function>()
        .MakeCallback(
            return_value,
            {Napi::Function::New(env,
                                 [context](const Napi::CallbackInfo& f_info) {
                                   context->Close();
                                   return f_info[0];
                                 }),
             Napi::Function::New(env,
                                 [context](const Napi::CallbackInfo& r_info) {
                                   context->Close();
                                   return r_info[0];
                                 })});
  }

  context->Close();
  return return_value;
}

void Context::Close() {
  ThreadInstance _(this);
  if (closing_runtime_) closing_runtime_();
  // Without we crash after a hang (due to callback being invoked after context
  // is closed), with we sometimes have a hang after reload in electron
  // renderer.
  dotnet_thread_safe_callback_.Abort();
  dotnet_thread_safe_callback_.Release();
}

JsHandle Context::GetMember(JsHandle& owner_handle, const char* name) {
  if (!owner_handle.SupportsMembers())
    return JsHandle::Error("JsHandle does not support member-access");
  if (!IsActiveContext())
    return JsHandle::Error("Must be called on node thread");

  auto owner = owner_handle.AsObject(env_);
  auto owner_object = owner.As<Napi::Object>();
  auto result = owner_object.Get(name);
  if (env_.IsExceptionPending()) {
    return JsHandle::Error(env_.GetAndClearPendingException().Message());
  }

  return JsHandle::FromValue(result);
}

JsHandle Context::GetMemberByIndex(JsHandle& owner_handle, int index) {
  if (!owner_handle.SupportsMembers())
    return JsHandle::Error("JsHandle does not support member-access");
  if (!IsActiveContext())
    return JsHandle::Error("Must be called on node thread");

  auto owner = owner_handle.AsObject(env_);
  auto owner_object = owner.As<Napi::Object>();
  auto result = owner_object[index];
  if (env_.IsExceptionPending()) {
    return JsHandle::Error(env_.GetAndClearPendingException().Message());
  }

  return JsHandle::FromValue(result);
}

JsHandle Context::SetMember(JsHandle& owner_handle, const char* name,
                            DotNetHandle& dotnet_handle) {
  if (!owner_handle.SupportsMembers())
    return JsHandle::Error("JsHandle does not support member-access");
  if (!IsActiveContext())
    return JsHandle::Error("Must be called on node thread");

  auto owner = owner_handle.AsObject(env_);
  auto owner_object = owner.As<Napi::Object>();
  auto value =
      dotnet_handle.ToValue(env_, function_factory_, array_buffer_factory_);
  dotnet_handle.Release();
  owner_object.Set(name, value);
  return JsHandle::Undefined();
}
JsHandle Context::CreateObject(JsHandle& prototype_function, int argc,
                               DotNetHandle* argv) {
  if (!IsActiveContext())
    return JsHandle::Error("Must be called on node thread");

  if (prototype_function.IsNotNullFunction()) {
    napi_value result;
    napi_status status;
    auto ctor = prototype_function.AsObject(env_);

    if (argc == 0) {
      status = napi_new_instance(env_, ctor, 0, nullptr, &result);
    } else if (argc <= MAX_ARGUMENTS_ON_STACK) {
      napi_value arguments[MAX_ARGUMENTS_ON_STACK];
      for (int c = 0; c < argc; c++) {
        arguments[c] =
            argv[c].ToValue(env_, function_factory_, array_buffer_factory_);
        argv[c].Release();
      }
      status = napi_new_instance(env_, ctor, argc, arguments, &result);
    } else {
      std::vector<napi_value> arguments(argc);
      for (int c = 0; c < argc; c++) {
        arguments[c] =
            argv[c].ToValue(env_, function_factory_, array_buffer_factory_);
        argv[c].Release();
      }
      status = napi_new_instance(env_, ctor, argc, arguments.data(), &result);
    }

    if (status != napi_ok) {
      return JsHandle::Error("Could not create instance");
    }
    return JsHandle::FromValue(Napi::Value(env_, result));
  }

  auto newObj = Napi::Object::New(env_);

  return JsHandle::FromObject(newObj);
}

JsHandle Context::Invoke(JsHandle& handle, JsHandle& receiver_handle, int argc,
                         DotNetHandle* argv) {
  if (!IsActiveContext())
    return JsHandle::Error("Must be called on node thread");

  auto value = handle.AsObject(env_);

  return InvokeIntern(value, receiver_handle.AsObject(env_), argc, argv);
}

JsHandle Context::Invoke(const char* name, JsHandle& receiver_handle, int argc,
                         DotNetHandle* argv) {
  if (!receiver_handle.SupportsMembers())
    return JsHandle::Error("JsHandle does not support member-access");
  if (!IsActiveContext())
    return JsHandle::Error("Must be called on node thread");

  auto receiver_object = receiver_handle.AsObject(env_).As<Napi::Object>();
  auto handle = receiver_object.Get(name);

  return InvokeIntern(handle, receiver_object, argc, argv);
}

JsHandle Context::InvokeIntern(Napi::Value handle, Napi::Value receiver,
                               int argc, DotNetHandle* argv) {
  if (env_.IsExceptionPending()) {
    return JsHandle::Error(env_.GetAndClearPendingException().Message());
  }
  if (!handle.IsFunction())
    return JsHandle::Error("JS object is not an invocable function");

  auto function = handle.As<Napi::Function>();

  Napi::Value result;

  if (argc == 0) {
    result = function.Call(receiver, 0, nullptr);
  } else if (argc <= MAX_ARGUMENTS_ON_STACK) {
    napi_value arguments[MAX_ARGUMENTS_ON_STACK];
    for (int c = 0; c < argc; c++) {
      auto dotnet_arg = argv[c];
      arguments[c] =
          dotnet_arg.ToValue(env_, function_factory_, array_buffer_factory_);
      dotnet_arg.Release();
    }
    result = function.Call(receiver, argc, arguments);
  } else {
    std::vector<napi_value> arguments(argc);
    for (int c = 0; c < argc; c++) {
      auto dotnet_arg = argv[c];
      arguments[c] =
          dotnet_arg.ToValue(env_, function_factory_, array_buffer_factory_);
      dotnet_arg.Release();
    }
    result = function.Call(receiver, arguments);
  }

  if (env_.IsExceptionPending()) {
    return JsHandle::Error(env_.GetAndClearPendingException().Message());
  }
  return JsHandle::FromValue(result);
}

void Context::CompletePromise(napi_deferred deferred, DotNetHandle& handle) {
  // TODO DM 29.11.2019: How to handle errors from napi calls here?
  if (handle.type_ == DotNetType::Exception) {
    auto error = Napi::Error::New(env_, handle.StringValue(env_));
    napi_reject_deferred(env_, deferred, error.Value());
  } else {
    napi_resolve_deferred(
        env_, deferred,
        handle.ToValue(env_, function_factory_, array_buffer_factory_));
  }
  handle.Release();
}

Napi::Function Context::CreateFunction(DotNetHandle* handle) {
  auto release_func = handle->release_func_;
  auto function_value = handle->function_value_;
  handle->release_func_ = nullptr;  // We delay the release

  // TODO: Check if using data instead of capture is better performancewise
  auto function = Napi::Function::New(
      env_, [this, function_value](const Napi::CallbackInfo& info) {
        ThreadInstance _(this);
        DotNetHandle resultIntern;
        auto argc = info.Length();

        // TODO DM 11.05.2020: Do not pack unused arguments, by marshalling
        // argc from dotnet. The most efficient solution would be to use
        // different function pointers per argc value
        if (argc == 0) {
          resultIntern = (*function_value)(0, nullptr);
        } else if (argc <= MAX_ARGUMENTS_ON_STACK) {
          JsHandle arguments[MAX_ARGUMENTS_ON_STACK];
          for (size_t c = 0; c < argc; c++) {
            arguments[c] = JsHandle::FromValue(info[c]);
          }

          resultIntern = (*function_value)(argc, arguments);

        } else {
          std::vector<JsHandle> arguments;
          for (size_t c = 0; c < argc; c++) {
            arguments.push_back(JsHandle::FromValue(info[c]));
          }

          resultIntern = (*function_value)(argc, arguments.data());
        }

        auto napiResultValue = resultIntern.ToValue(
            info.Env(), this->function_factory_, this->array_buffer_factory_);
        resultIntern.Release();
        return napiResultValue;
      });

  auto finalizer_data =
      new SynchronizedFinalizerCallback(this, [release_func, function_value]() {
        release_func(DotNetType::Function,
                     reinterpret_cast<void*>(function_value));
      });

  function.AddFinalizer(SynchronizedFinalizerCallback::Wrapper, finalizer_data);

  return function;
}

class Context::BufferCache {
  std::map<void*, void (*)(DotNetType::Enum, void*)> release_callbacks_;
  Napi::ObjectReference buffer_ref_;

 public:
  BufferCache(Napi::Buffer<uint8_t> buffer, void* value,
              void (*release)(DotNetType::Enum, void*))
      : buffer_ref_(Napi::Weak((Napi::Object)buffer)) {
    release_callbacks_[value] = release;
  }

  ~BufferCache() {
    for (const auto& callback : release_callbacks_) {
      callback.second(DotNetType::ByteArray, callback.first);
    }
    // release_callbacks_.clear();
  }

  Napi::Value UseValue(void* value, void (*release)(DotNetType::Enum, void*)) {
    release_callbacks_[value] = release;
    return buffer_ref_.Value();
  }
};

Napi::Value Context::CreateArrayBuffer(DotNetHandle* handle) {
  auto value_copy = handle->value_;
  auto release_array_func = handle->release_func_;
  handle->release_func_ = nullptr;  // We delay the release

  // We have a pointer to a struct { int32_t, void* } and interpret it like a
  // int32_t*
  auto array_value_ptr = reinterpret_cast<int32_t*>(value_copy);
  auto length = *array_value_ptr;
  auto data_ptr = *reinterpret_cast<uint8_t**>(array_value_ptr + 1);

  auto existing_buffer = buffers_.find(data_ptr);
  if (existing_buffer != buffers_.end()) {
    return existing_buffer->second->UseValue(value_copy, release_array_func);
  }

  auto finalizerData =
      new SynchronizedFinalizerCallback(this, [this, data_ptr]() {
        auto cache_node = buffers_.find(data_ptr);
        auto cache = cache_node->second;
        buffers_.erase(cache_node);
        delete cache;
      });

  auto result = Napi::Buffer<uint8_t>::New(
      env_, data_ptr, length,
      [](napi_env env, void* data, SynchronizedFinalizerCallback* hint) {
        hint->Call();
        delete hint;
      },
      finalizerData);

  auto cache = new BufferCache(result, value_copy, release_array_func);
  buffers_[data_ptr] = cache;
  return result;

  // This makes a non-transferable array buffer => crashes electron when
  // rendering
  /*auto array_buf = Napi::ArrayBuffer::New(env_,
     *reinterpret_cast<uint8_t**>(array_value_ptr + 1), length,
      [](napi_env env, void* data, SynchronizedFinalizerCallback* hint) {
        hint->Call();
      },
      finalizerData);*/

  // This creates a transferable array_buffer, but does not use external memory
  // :(
  /*auto array_buf = Napi::ArrayBuffer::New(env_, length);
  memcpy(array_buf.Data(), *reinterpret_cast<uint8_t**>(array_value_ptr + 1),
  length); */

  // Do it manually without marking it as non-transferable (ATTENTION: New
  // BackingStore API in newer v8 versions) This does not work either, we have
  // many problems:
  // - Different v8 Versions (Electron/Node)
  // - It looks like externalized buffers can not be shared with worker threads
  /*v8::Isolate* isolate = v8::Isolate::GetCurrent();
  v8::Local<v8::ArrayBuffer> buffer =
      v8::ArrayBuffer::New(isolate, *reinterpret_cast<uint8_t**>(array_value_ptr
  + 1), length);

  napi_value array_buf =  reinterpret_cast<napi_value>(*buffer);//
  v8impl::JsValueFromV8LocalValue(buffer);

  return Napi::Uint8Array::New(env_, length, Napi::ArrayBuffer(env_, array_buf),
  0, napi_uint8_clamped_array);*/
}

int Context::TryAccessArrayBuffer(JsHandle& handle, void*& address,
                                  int& byte_length) {
  if (!IsActiveContext()) return 0;
  if (handle.type_ != JsType::Object) return 0;
  auto object = handle.AsObject(env_);
  if (!object.IsArrayBuffer()) return 0;

  auto array_buffer = object.As<Napi::ArrayBuffer>();
  address = array_buffer.Data();
  byte_length = array_buffer.ByteLength();

  return 1;
}

}  // namespace coreclrhosting