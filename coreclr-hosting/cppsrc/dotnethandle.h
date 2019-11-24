#ifndef __CORECLR_HOSTING_DOTNETHANDLE_H__
#define __CORECLR_HOSTING_DOTNETHANDLE_H__

#include <functional>

#include "jshandle.h"

namespace DotNetType {
typedef enum {
  Undefined,
  Null,
  Boolean,
  Int32,
  Int64,
  Double,
  String,    // Do we need encodings or do we assume UTF8?
  JsHandle,  // A handle that was received from node
  Function,
  ByteArray,
  Task,
  Exception
} Enum;
}

extern "C" struct DotNetHandle {
  DotNetType::Enum type_;
  union {
    void *value_;
    JsHandle *jshandle_value_;
    char *string_value_;
    bool bool_value_;
    int32_t int32_value_;
    int64_t int64_value_;
    double double_value_;
    void (*function_value_)(int, JsHandle *, DotNetHandle &);
    void (*task_value_)(napi_deferred);
  };

  void (*release_func_)(DotNetType::Enum, void *);

  void Release() {
    // printf("Releasing handle with %p \n", value_);
    if (nullptr != release_func_) release_func_(type_, value_);
  }

  Napi::Value ToValue(
      const Napi::Env &env,
      std::function<Napi::Function(DotNetHandle *)> function_factory) {
    if (type_ == DotNetType::Null) return env.Null();
    if (type_ == DotNetType::JsHandle) return jshandle_value_->ToValue(env);
    if (type_ == DotNetType::String)
      return Napi::String::New(
          env, string_value_);  // TODO: string is copied, we could use char16_t
                                // to prevent a copy
    if (type_ == DotNetType::Boolean)
      return Napi::Boolean::New(env, bool_value_);
    if (type_ == DotNetType::Int32) return Napi::Number::New(env, int32_value_);
    if (type_ == DotNetType::Int64) return Napi::Number::New(env, int64_value_);
    if (type_ == DotNetType::Double)
      return Napi::Number::New(env, double_value_);

    if (type_ == DotNetType::Function) {
      return function_factory(this);
    }
    if (type_ == DotNetType::ByteArray) {
      auto release_func = release_func_;
      release_func_ = nullptr;  // We delay the release

      return Napi::ArrayBuffer::New(
          env,
          reinterpret_cast<void *>(reinterpret_cast<int32_t *>(value_) + 1),
          *reinterpret_cast<int32_t *>(value_),
          [release_func](napi_env env, void *finalize_data) {
            DotNetHandle copy;
            copy.type_ = DotNetType::ByteArray;
            copy.value_ = reinterpret_cast<void *>(
                reinterpret_cast<int32_t *>(finalize_data) - 1);
            copy.release_func_ = release_func;
            copy.Release();
          });
    }
    if (type_ == DotNetType::Task) {
      napi_deferred deferred;
      napi_value promise;
      napi_status status = napi_create_promise(env, &deferred, &promise);
      if ((status) != napi_ok) {
        Napi::Error::New(env).ThrowAsJavaScriptException();
        return Napi::Value();
      }
      task_value_(deferred);
      return Napi::Promise(env, promise);
    }

    if (type_ == DotNetType::Exception) {
      //printf("Throwing: %s\n", string_value_);
      Napi::Error::New(env, string_value_).ThrowAsJavaScriptException();
      return Napi::Value();
    }

    // TODO: Support other types
    return Napi::Value();
  }
};

#endif