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
  Exception,
  Collection
} Enum;
}

extern "C" struct DotNetHandle {
  DotNetType::Enum type_;
  union {
    void *value_;
    JsHandle *jshandle_value_;
    int32_t *string_value_;
    bool bool_value_;
    int32_t int32_value_;
    int64_t int64_value_;
    double double_value_;
    void (*function_value_)(int, JsHandle *, DotNetHandle &);
    DotNetHandle (*task_value_)(napi_deferred);
    int32_t *collection_value_;
  };

  void (*release_func_)(DotNetType::Enum, void *);

  void Release() {
    // printf("Releasing handle with %p \n", value_);
    if (nullptr != release_func_) release_func_(type_, value_);
  }

  Napi::Value ToValue(
      const Napi::Env &env,
      std::function<Napi::Function(DotNetHandle *)> function_factory,
      std::function<Napi::ArrayBuffer(DotNetHandle *)> array_buffer_factory) {
    if (type_ == DotNetType::Null) return env.Null();
    if (type_ == DotNetType::JsHandle) return jshandle_value_->AsObject(env);
    if (type_ == DotNetType::String) {
      return StringValue(env);
    }
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
      return array_buffer_factory(this);
    }
    if (type_ == DotNetType::Task) {
      napi_deferred deferred;
      napi_value promise;
      napi_status status = napi_create_promise(env, &deferred, &promise);
      if ((status) != napi_ok) {
        Napi::Error::New(env).ThrowAsJavaScriptException();
        return Napi::Value();
      }
      // DM 29.11.2019: Connect the deferred with the .NET Task
      auto result = task_value_(deferred);
      // This must return either DotNetType::Null or an DotNetType::Exception
      if (result.type_ == DotNetType::Exception) {
        // Reject with the error right away
        auto error = Napi::Error::New(env, result.StringValue(env));
        napi_reject_deferred(env, deferred, error.Value());
      }
      result.Release();

      return Napi::Promise(env, promise);
    }

    if (type_ == DotNetType::Exception) {      
      Napi::Error::New(env, StringValue(env)).ThrowAsJavaScriptException();
      return Napi::Value();
    }

    if (type_ == DotNetType::Collection) {
      auto length = *collection_value_;
      auto values = reinterpret_cast<DotNetHandle*>(collection_value_ + 1);
      auto array = Napi::Array::New(env, length);
      for (uint32_t inx = 0; inx < length; inx++) {
        array.Set(inx, values[inx].ToValue(env, function_factory, array_buffer_factory));
      }
      return array;
    }

    // TODO: Support other types
    return Napi::Value();
  }

  inline Napi::String StringValue(const Napi::Env &env) {
    return Napi::String::New(env, *reinterpret_cast<char16_t**>(string_value_ + 1), *string_value_);
  }
};

#endif