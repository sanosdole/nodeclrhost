#ifndef __CORECLR_NATIVEAPI_H__
#define __CORECLR_NATIVEAPI_H__

extern "C" {
struct NativeApi {
  void* RegisterSchedulerCallbacks;
  void* SignalEventLoopEntry;
  void* SignalMicroTask;
  void* GetMember;
  void* GetMemberByIndex;
  void* SetMember;
  void* Invoke;
  void* InvokeByName;
  void* CreateObject;  
  void* CompletePromise;
  void* TryAccessArrayBuffer;
  void* Release;
  void* CloseContext;

  static NativeApi instance_;
};
}

#endif