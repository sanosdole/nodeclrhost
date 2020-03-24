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

  static JsHandle Undefined() { return { JsType::Undefined, nullptr }; }
  static JsHandle Null() { return { JsType::Null, nullptr }; }

  static JsHandle Error(std::string message) {
    JsHandle result = Undefined();
    auto buffer_size = message.size() + 1;
    result.u8_string_value_ = new char[buffer_size];
    memcpy(result.u8_string_value_, message.c_str(), buffer_size);
    result.type_ = JsType::Error;
    return result;
  }

  // TODO DM 20.03.2020: Replace ctors with factory methods to remove UDT warning
  static JsHandle FromObject(Napi::Object object) {
    return { JsType::Object, new Napi::Reference<Napi::Object>(Napi::Persistent(object)) };
  }
  
  static JsHandle FromFunction(Napi::Function func) {
    return { JsType::Function, new Napi::Reference<Napi::Function>(Napi::Persistent(func)) };
  }
  static JsHandle FromString(Napi::String value) {
    auto utf16 = value.Utf16Value();
    auto buffer_size = 2 * (utf16.size() + 1);
    auto string_value = new char16_t[buffer_size];
    memcpy(string_value, utf16.c_str(), buffer_size);
    
    return { JsType::String, string_value };
  }
  static JsHandle FromBoolean(Napi::Boolean boolean_value) {
    return { JsType::Boolean, boolean_value.Value() ? (void*)1 : (void*)0 };
  }
  static JsHandle FromDouble(double double_value) {    
    JsHandle result;
    result.type_ = JsType::Number;
    result.double_value_ = double_value;
    return result;
  }
  static JsHandle Make(JsType::Enum type, void* value) {    
    return { type, value };
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
      return Null();
    }
    if (value.IsString()) {
      return FromString(value.ToString());
    }
    if (value.IsBoolean()) {
      return FromBoolean(value.ToBoolean());
    }
    if (value.IsNumber()) {
      auto number = value.ToNumber();
      // TODO: How to handle 32bit processes...
      auto doubleValue = number.DoubleValue();
      return FromDouble(doubleValue);
    }    
    if (value.IsFunction()) {
      return FromFunction(value.As<Napi::Function>());
    }
    if (value.IsObject()) {
      return FromObject(value.ToObject());
    }
    
    return Undefined();
  }

  bool SupportsMembers() const { return type_ == JsType::Object || type_ == JsType::Function; }

  bool IsNotNullFunction() const {
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