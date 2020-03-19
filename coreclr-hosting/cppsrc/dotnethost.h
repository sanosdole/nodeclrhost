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
  kInitializeFailed
};
}

class DotNetHost {
  class Impl;
  std::unique_ptr<Impl> impl_;

  DotNetHost(std::unique_ptr<Impl> impl);

  DotNetHost(const DotNetHost&) = delete;
  DotNetHost& operator=(const DotNetHost&) = delete;  // no self-assignments

 public:
  ~DotNetHost();

  static DotNetHostCreationResult::Enum Create(
      std::string assembly_path,
      std::unique_ptr<DotNetHost>& host);

  int32_t ExecuteAssembly(const std::vector<std::string> &arguments/*, void *result_callback(int32_t)*/);
};

}  // namespace coreclrhosting

#endif