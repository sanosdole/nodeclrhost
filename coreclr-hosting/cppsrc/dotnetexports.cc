#include "context.h"

using namespace coreclrhosting;

#ifdef WINDOWS
#define EXPORT_TO_DOTNET extern "C" __declspec(dllexport)
#else
#define EXPORT_TO_DOTNET extern "C" __attribute__((visibility("default")))
#endif

EXPORT_TO_DOTNET void* GetContext() {
    return Context::CurrentInstance();
}

/* MUST BE C-STYLE FUNCTION */
EXPORT_TO_DOTNET void PostCallback(void* context_handle, void callback(void*), void* data) {
    auto context = reinterpret_cast<Context*>(context_handle);
    context->PostCallback(callback, data);
}

EXPORT_TO_DOTNET void ReleaseContext(void* context_handle) {
    auto context = reinterpret_cast<Context*>(context_handle);
    context->Release();
}

EXPORT_TO_DOTNET void Release(JsHandle handle) {
    handle.Release();
}

EXPORT_TO_DOTNET JsHandle GetMember(void* context_handle, JsHandle owner_handle, char* name) {
    auto context = reinterpret_cast<Context*>(context_handle);
    return context->GetMember(owner_handle, name);
}

EXPORT_TO_DOTNET JsHandle SetMember(void* context_handle, JsHandle owner_handle, char* name, DotNetHandle dotnet_handle) {
    auto context = reinterpret_cast<Context*>(context_handle);
    return context->SetMember(owner_handle, name, dotnet_handle);
}

EXPORT_TO_DOTNET JsHandle CreateObject(void* context_handle, JsHandle prototype, int argc, DotNetHandle* argv) {
    auto context = reinterpret_cast<Context*>(context_handle);
    return context->CreateObject(prototype, argc, argv);
}


// TODO: Maybe we should use (JsHandle owner, char* name,...)  so we do not need to marshal the function first!
EXPORT_TO_DOTNET JsHandle Invoke(void* context_handle, JsHandle handle, JsHandle receiver, int argc, DotNetHandle* argv) {
    auto context = reinterpret_cast<Context*>(context_handle);
    return context->Invoke(handle, receiver, argc, argv);
}
