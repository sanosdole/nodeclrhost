#ifndef __CORECLR_NATIVEAPI_H__
#define __CORECLR_NATIVEAPI_H__

extern "C" {
struct NativeApi {
  void* PostCallback;
  void* GetMember;
  void* GetMemberByIndex;
  void* SetMember;
  void* Invoke;
  void* InvokeByName;
  void* CreateObject;
  void* CompletePromise;
  void* Release;

  static NativeApi instance_;
};
}

#endif