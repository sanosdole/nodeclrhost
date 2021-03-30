#ifndef __CORECLR_HOSTING_HOSTMAIN_H__
#define __CORECLR_HOSTING_HOSTMAIN_H__

#include <memory>
#include <string>
#include <vector>

namespace coreclrhosting {

namespace DotNetHostCreationResult {
enum Enum {
  kOK,
  kAssemblyNotFound,
  kCoreClrNotFound,
  kInvalidCoreClr,
  kInitializeFailed,
  kReinitializationNotSupported
};
}

class DotNetHost {
  class Impl;
  std::shared_ptr<Impl> impl_;

  DotNetHost(std::shared_ptr<Impl> impl);

  DotNetHost(const DotNetHost&) = delete;
  DotNetHost& operator=(const DotNetHost&) = delete;  // no self-assignments

 public:
  ~DotNetHost();

  static DotNetHostCreationResult::Enum Create(
      std::string assembly_path,
      std::unique_ptr<DotNetHost>& host);

  void* GetManagedFunction(std::string type_name, std::string method_name, std::string signature_delegate_name);
};

}  // namespace coreclrhosting

#endif