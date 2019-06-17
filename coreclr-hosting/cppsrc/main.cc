#include <napi.h>

#include "context.h"

Napi::Object InitAll(Napi::Env env, Napi::Object exports) {
  exports.Set("runCoreApp",
              Napi::Function::New(env, coreclrhosting::Context::RunCoreApp));
  return exports;
}

NODE_API_MODULE(coreclrHosting, InitAll)
