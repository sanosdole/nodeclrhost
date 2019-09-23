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

  // TODO: What about special objects like arrays & promises
} Enum;
}

struct JsHandle {
  JsType::Enum type_;
  union {
    void* value_;
    double double_value_;
    char* string_value_;
    Napi::Reference<Napi::Object>* object_value_;
    Napi::Reference<Napi::Function>* function_value_;
  };

  static JsHandle Undefined() { return JsHandle(JsType::Undefined, nullptr); }

  static JsHandle Error(std::string message) {
    JsHandle result = Undefined();
    result.string_value_ = new char[message.size()];
    memcpy(result.string_value_, message.c_str(), message.size());
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
    auto utf8 = value.Utf8Value();
    string_value_ = new char[utf8.size() + 1];
    memcpy(string_value_, utf8.c_str(), utf8.size() + 1);
    type_ = JsType::String;
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

  Napi::Value ToValue(const Napi::Env& env) const {
    if (type_ == JsType::Object) {
      if (nullptr == object_value_) return env.Global();
      return object_value_->Value();
    }
    if (type_ == JsType::Function) return function_value_->Value();

    // TODO: Support blittable types
    return Napi::Value();
  }

  static JsHandle FromValue(Napi::Value value) {
    if (value.IsNull()) {
      return JsHandle(JsType::Null, nullptr);
    }
    if (value.IsString()) {
      return JsHandle(value.ToString());
    }
    if (value.IsNumber()) {
      auto number = value.ToNumber();
      // TODO: How to handle 32bit processes...
      auto doubleValue = number.DoubleValue();
      return JsHandle(doubleValue);
    }
    if (value.IsBoolean()) {
      return JsHandle(value.ToBoolean());
    }
    if (value.IsFunction()) {
      return JsHandle(value.As<Napi::Function>());
    }
    if (value.IsObject()) {
      return JsHandle(value.ToObject());
    }

    // TODO: Marshal arrays

    return Undefined();
  }

  bool IsObject() { return type_ == JsType::Object; }

  bool IsNotNullFunction() {
    return type_ == JsType::Function && function_value_ != nullptr;
  }

  void Release() {
    // printf("Releasing %d \n", Type);
    if (type_ == JsType::Object) {
      delete object_value_;
      return;
    }
    if (type_ == JsType::Function) {
      delete function_value_;
      return;
    }

    if (type_ == JsType::String || type_ == JsType::Error) {
      delete[] string_value_;
    }
  }
};
}
#endif