#include "nativeapi.h"

#include "context.h"

#ifdef WINDOWS
#define EXPORT_TO_DOTNET extern "C" __declspec(dllexport)
#else
#define EXPORT_TO_DOTNET extern "C" __attribute__((visibility("default")))
#endif

using namespace coreclrhosting;

/* MUST BE C-STYLE FUNCTION */
EXPORT_TO_DOTNET void RegisterSchedulerCallbacks(void* context_handle, void (*process_event_loop)(void*),
                                   void (*process_micro_task)(void*)) {
  auto context = reinterpret_cast<Context*>(context_handle);
  context->RegisterSchedulerCallbacks(process_event_loop, process_micro_task);
}

EXPORT_TO_DOTNET void SignalEventLoopEntry(void* context_handle, void* data) {
  auto context = reinterpret_cast<Context*>(context_handle);
  context->SignalEventLoopEntry(data);
}

EXPORT_TO_DOTNET void SignalMicroTask(void* context_handle, void* data) {
  auto context = reinterpret_cast<Context*>(context_handle);
  context->SignalMicroTask(data);
}

EXPORT_TO_DOTNET void Release(JsHandle handle) { handle.Release(); }

EXPORT_TO_DOTNET JsHandle GetMember(void* context_handle, JsHandle owner_handle,
                                    char* name) {
  auto context = reinterpret_cast<Context*>(context_handle);
  return context->GetMember(owner_handle, name);
}
EXPORT_TO_DOTNET JsHandle GetMemberByIndex(void* context_handle,
                                           JsHandle owner_handle, int index) {
  auto context = reinterpret_cast<Context*>(context_handle);
  return context->GetMemberByIndex(owner_handle, index);
}
EXPORT_TO_DOTNET JsHandle SetMember(void* context_handle, JsHandle owner_handle,
                                    char* name, DotNetHandle dotnet_handle) {
  auto context = reinterpret_cast<Context*>(context_handle);
  return context->SetMember(owner_handle, name, dotnet_handle);
}

EXPORT_TO_DOTNET JsHandle CreateObject(void* context_handle, JsHandle prototype,
                                       int argc, DotNetHandle* argv) {
  auto context = reinterpret_cast<Context*>(context_handle);
  return context->CreateObject(prototype, argc, argv);
}

EXPORT_TO_DOTNET JsHandle Invoke(void* context_handle, JsHandle handle,
                                 JsHandle receiver, int argc,
                                 DotNetHandle* argv) {
  auto context = reinterpret_cast<Context*>(context_handle);
  return context->Invoke(handle, receiver, argc, argv);
}

EXPORT_TO_DOTNET JsHandle InvokeByName(void* context_handle, char* name,
                                       JsHandle receiver, int argc,
                                       DotNetHandle* argv) {
  auto context = reinterpret_cast<Context*>(context_handle);
  return context->Invoke(name, receiver, argc, argv);
}

EXPORT_TO_DOTNET void CompletePromise(void* context_handle, void* deferred,
                                      DotNetHandle dotnet_handle) {
  auto context = reinterpret_cast<Context*>(context_handle);
  return context->CompletePromise(reinterpret_cast<napi_deferred>(deferred),
                                  dotnet_handle);
}

NativeApi NativeApi::instance_ = {
    reinterpret_cast<void*>(&::RegisterSchedulerCallbacks),
    reinterpret_cast<void*>(&::SignalEventLoopEntry),
    reinterpret_cast<void*>(&::SignalMicroTask),
    reinterpret_cast<void*>(&::GetMember),
    reinterpret_cast<void*>(&::GetMemberByIndex),
    reinterpret_cast<void*>(&::SetMember),
    reinterpret_cast<void*>(&::Invoke),
    reinterpret_cast<void*>(&::InvokeByName),
    reinterpret_cast<void*>(&::CreateObject),
    reinterpret_cast<void*>(&::CompletePromise),
    reinterpret_cast<void*>(&::Release)};