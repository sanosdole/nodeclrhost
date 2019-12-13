#ifndef __CORECLR_HOSTING_CONTEXT_H__
#define __CORECLR_HOSTING_CONTEXT_H__

#include <mutex>
#include <string>
#include <vector>
#include <set>
#include <future>
#include <iostream>

#include <napi.h>
#include <uv.h>

#include "dotnethandle.h"
#include "dotnethost.h"
#include "jshandle.h"

namespace coreclrhosting {

class Context {
  struct FunctionFinalizerData;
  typedef std::function<void(void*)> netCallback_t;
  typedef std::vector<std::pair<netCallback_t, void*>> netCallbacks_t;
  
  Napi::Env env_;
  uv_async_t async_handle_;
  netCallbacks_t dotnet_callbacks_;
  std::set<FunctionFinalizerData*> function_finalizers_;
  std::shared_ptr<std::mutex> finalizer_mutex_;
  std::mutex mutex_;
  bool release_called_;
  std::unique_ptr<DotNetHost> host_;
  std::function<Napi::Function(DotNetHandle*)> function_factory_;

  Context(const Context&) = delete;
  Context& operator=(const Context&) = delete;  // no self-assignments
  Context(std::unique_ptr<DotNetHost> dotnet_host, Napi::Env env);
  ~Context();
  void UvAsyncCallback();

  class ThreadInstance {
    static thread_local Context* thread_instance_;
    Context* previous_;

   public:
    ThreadInstance(Context* instance) {
      previous_ = thread_instance_;
      thread_instance_ = instance;
    }
    ~ThreadInstance() { thread_instance_ = previous_; }
    static Context* Current() { return thread_instance_; }
  };

  bool IsActiveContext() { return ThreadInstance::Current() == this; }

  Napi::Function CreateFunction(DotNetHandle* handle);

 public:
  static Napi::Value RunCoreApp(const Napi::CallbackInfo& info);
  static Context* CurrentInstance() { return ThreadInstance::Current(); }

  // TODO: How to get the instance when not on node thread? => .NET calls
  // getInstance on main thread
  // TODO: How to ensure ThreadContext on calls from node to .NET delegates (see
  // DotNetHandle)
  void PostCallback(netCallback_t callback,
                    void* data);  // can be called from any thread
  void Release();                 // can be called from any thread

  // Get/SetMember , CreateObject, Invoke(Member) can only be called on node
  // thread
  JsHandle GetMember(JsHandle& owner_handle, const char* name);
  JsHandle SetMember(JsHandle& owner_handle, const char* name,
                     DotNetHandle& dotnet_handle);
  JsHandle CreateObject(JsHandle& prototype_function, int argc,
                        DotNetHandle* argv);
  JsHandle Invoke(JsHandle& handle, JsHandle& receiver_handle, int argc,
                  DotNetHandle* argv);
  void CompletePromise(napi_deferred deferred, DotNetHandle& handle);
};

}  // namespace coreclrhosting

#endif