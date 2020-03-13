#ifndef __CORECLR_HOSTING_JSHANDLE_H__
#define __CORECLR_HOSTING_JSHANDLE_H__

#include <functional>
#include <string>
#include <vector>

#include <napi.h>

extern "C" {

namespace JsType {
typedef enum {
  // napi_value_type
  Undefined,
  Null,
  Boolean,
  Number,
  String,
  Symbol,  // Not used
  Object,
  Function,
  External,  // Not used

  // Custom types
  Error

} Enum;
}

struct JsHandle {
  JsType::Enum type_;
  union {
    void* value_;
    double double_value_;
    char16_t* string_value_;
    char* u8_string_value_;
    Napi::Reference<Napi::Object>* object_value_;
    Napi::Reference<Napi::Function>* function_value_;
  };

  static JsHandle Undefined() { return JsHandle(JsType::Undefined, nullptr); }

  static JsHandle Error(std::string message) {
    JsHandle result = Undefined();
    auto buffer_size = message.size() + 1;
    result.u8_string_value_ = new char[buffer_size];
    memcpy(result.u8_string_value_, message.c_str(), buffer_size);
    result.type_ = JsType::Error;
    return result;
  }

  JsHandle(Napi::Object object) {
    object_value_ = new Napi::Reference<Napi::Object>(Napi::Persistent(object));
    type_ = JsType::Object;
  }
  JsHandle(Napi::Function func) {
    function_value_ =
        new Napi::Reference<Napi::Function>(Napi::Persistent(func));
    type_ = JsType::Function;
  }
  JsHandle(Napi::String value) {
    auto utf16 = value.Utf16Value();
    auto buffer_size = 2 * (utf16.size() + 1);
    string_value_ = new char16_t[buffer_size];
    memcpy(string_value_, utf16.c_str(), buffer_size);
    type_ = JsType::String;
  }
  JsHandle(Napi::Boolean boolean_value) {
    type_ = JsType::Boolean;
    //* ( void * * ) &doubleValue
    value_ = boolean_value.Value() ? (void*)1 : (void*)0;    
  }
  JsHandle(double double_value) {
    type_ = JsType::Number;
    //* ( void * * ) &doubleValue
    double_value_ = double_value;
  }
  JsHandle(JsType::Enum type, void* value) {
    value_ = value;
    type_ = type;
  }

  Napi::Value AsObject(const Napi::Env& env) const {
    if (type_ == JsType::Object) {
      if (nullptr == object_value_) return env.Global();
      return object_value_->Value();
    }
    if (type_ == JsType::Function) return function_value_->Value();

    if (type_ == JsType::Error) {
      Napi::Error::New(env, u8_string_value_)
          .ThrowAsJavaScriptException();
      return Napi::Value();
    }

    Napi::Error::New(env, "JsHandle is neither a object nor a function!")
          .ThrowAsJavaScriptException();
    return Napi::Value();
  }

  static JsHandle FromValue(Napi::Value value) {
    if (value.IsNull()) {
      return JsHandle(JsType::Null, nullptr);
    }
    if (value.IsString()) {
      return JsHandle(value.ToString());
    }
    if (value.IsBoolean()) {
      return JsHandle(value.ToBoolean());
    }
    if (value.IsNumber()) {
      auto number = value.ToNumber();
      // TODO: How to handle 32bit processes...
      auto doubleValue = number.DoubleValue();
      return JsHandle(doubleValue);
    }    
    if (value.IsFunction()) {
      return JsHandle(value.As<Napi::Function>());
    }
    if (value.IsObject()) {
      return JsHandle(value.ToObject());
    }
    
    return Undefined();
  }

  bool SupportsMembers() { return type_ == JsType::Object || type_ == JsType::Function; }

  bool IsNotNullFunction() {
    return type_ == JsType::Function && function_value_ != nullptr;
  }

  void Release() {
    // printf("Releasing %d \n", Type);
    if (type_ == JsType::Object) {
      delete object_value_;
      object_value_ = nullptr;
      return;
    }
    if (type_ == JsType::Function) {
      delete function_value_;
      function_value_ = nullptr;
      return;
    }

    if (type_ == JsType::String) {
      delete[] string_value_;
      string_value_ = nullptr;
      return;
    }

    if (type_ == JsType::Error) {
      delete[] u8_string_value_;
      string_value_ = nullptr;
    }
  }
};
}
#endif