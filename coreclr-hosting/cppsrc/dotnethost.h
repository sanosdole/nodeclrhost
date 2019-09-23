#ifndef __CORECLR_HOSTING_HOSTMAIN_H__
#define __CORECLR_HOSTING_HOSTMAIN_H__

#include <memory>
#include <string>

namespace coreclrhosting {

namespace DotNetHostCreationResult {
enum Enum { kOK, kCoreClrNotFound, kInvalidCoreClr, kInitializeFailed };
}

namespace DotNetHostExecuteAssemblyResult {
enum Enum { kOK, kAssemblyNotFound };
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
      std::string base_path, std::unique_ptr<DotNetHost>& host);

  // name must be located in path
  DotNetHostExecuteAssemblyResult::Enum ExecuteAssembly(
      std::string name, int argc, const char** argv, unsigned int& resultCode);
};
/*
class coreclr_not_found : public std::exception {
    std::string path_;
    

    coreclr_not_found(const std::string& path) : path_(std::move(path)) { }

    const char* what() const noexcept override {
        return "No coreclr runtime found at path '" + path_ + "'!";
    }
};

class invalid_coreclr : public std::exception {
    std::string path_;
    

    invalid_coreclr(const std::string& path) : path_(std::move(path)) { }

    const char* what() const noexcept override {
        return "The coreclr runtime at path '" + path_ + "' is not valid!";
    }
};

class assembly_not_found : public std::exception {
    std::string path_;
    std::string name_;
    

    assembly_not_found(std::string path, std::string name) :
path_(std::move(path)), name_(std::move(name) { }

    const char* what() const noexcept override {
        return "The coreclr runtime at path '" + path_ + "' is not valid";
    }
};*/

}  // namespace coreclrhosting

#endif