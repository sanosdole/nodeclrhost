#ifndef __CORECLR_HOSTING_CONTEXT_H__
#define __CORECLR_HOSTING_CONTEXT_H__

#include <napi.h>

#include <future>
#include <iostream>
#include <mutex>
#include <set>
#include <map>
#include <string>
#include <vector>

#include "dotnethandle.h"
#include "dotnethost.h"
#include "jshandle.h"

namespace coreclrhosting {

class Context {
  class SynchronizedFinalizerCallback;
  class BufferCache;

  Napi::Env env_;
  std::set<SynchronizedFinalizerCallback*> function_finalizers_;
  std::shared_ptr<std::mutex> finalizer_mutex_;
  std::unique_ptr<DotNetHost> host_;
  std::function<Napi::Function(DotNetHandle*)> function_factory_;
  std::function<Napi::Value(DotNetHandle*)> array_buffer_factory_;
  std::function<void(Napi::Env env, Napi::Function jsCallback, void* data)> process_event_loop_;  
  //std::function<Napi::Value(const Napi::CallbackInfo&)> process_micro_task_;
  Napi::FunctionReference process_micro_task_;
  Napi::FunctionReference signal_micro_task_;
  Napi::ThreadSafeFunction dotnet_thread_safe_callback_;
  void (*closing_runtime_)(void);

  Context(const Context&) = delete;
  Context& operator=(const Context&) = delete;  // no self-assignments
  Context(std::unique_ptr<DotNetHost> dotnet_host, Napi::Env env);
  ~Context();

  static void DeleteContext(Napi::Env env, Context* context);

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
  Napi::Value CreateArrayBuffer(DotNetHandle* handle);
  JsHandle InvokeIntern(Napi::Value handle, Napi::Value receiver, int argc,
                        DotNetHandle* argv);
  void Close();

 public:
  static Napi::Value RunCoreApp(const Napi::CallbackInfo& info);
  static Context* CurrentInstance() { return ThreadInstance::Current(); }

  void RegisterSchedulerCallbacks(void (*process_event_loop)(void*),
                                  void (*process_micro_task)(void*),
                                  void (*closing_runtime)(void));
  void SignalEventLoopEntry(void* data);  // Must be thread safe
  void SignalMicroTask(void* data);

  // Get/SetMember , CreateObject, Invoke(Member) can only be called on node
  // thread
  JsHandle GetMember(JsHandle& owner_handle, const char* name);
  JsHandle GetMemberByIndex(JsHandle& owner_handle, int index);
  JsHandle SetMember(JsHandle& owner_handle, const char* name,
                     DotNetHandle& dotnet_handle);
  JsHandle CreateObject(JsHandle& prototype_function, int argc,
                        DotNetHandle* argv);
  JsHandle Invoke(JsHandle& handle, JsHandle& receiver_handle, int argc,
                  DotNetHandle* argv);
  JsHandle Invoke(const char* name, JsHandle& receiver_handle, int argc,
                  DotNetHandle* argv);
  void CompletePromise(napi_deferred deferred, DotNetHandle& handle);
  int TryAccessArrayBuffer(JsHandle& handle, void*& address, int& byte_length);
};

}  // namespace coreclrhosting

#endif