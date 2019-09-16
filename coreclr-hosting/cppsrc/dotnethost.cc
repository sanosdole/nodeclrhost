#include "dotnethost.h"

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <string>

// https://github.com/dotnet/coreclr/blob/master/src/coreclr/hosts/inc/coreclrhost.h
#include "coreclrhost.h"

namespace {

// Define OS-specific items like the CoreCLR library's name and path elements

#if WINDOWS
#include <Windows.h>
#define FS_SEPARATOR "\\"
#define PATH_DELIMITER ";"
#define CORECLR_FILE_NAME "coreclr.dll"

typedef HMODULE LibraryHandle;

LibraryHandle LoadLibrary(std::string path) {
  return LoadLibraryExA(path.c_str(), NULL, 0);
}

template <typename TFuncPointer>
TFuncPointer GetFunction(LibraryHandle library, const char* name) {
  return reinterpret_cast<TFuncPointer>(GetProcAddress(library, name));
}

// Win32 directory search for .dll files
void BuildTpaList(const char* directory, const char* extension,
                  std::string& tpaList) {
  // This will add all files with a .dll extension to the TPA list.
  // This will include unmanaged assemblies (coreclr.dll, for example) that
  // don't belong on the TPA list. In a real host, only managed assemblies that
  // the host expects to load should be included. Having extra unmanaged
  // assemblies doesn't cause anything to fail, though, so this function just
  // enumerates all dll's in order to keep this sample concise.
  std::string searchPath(directory);
  searchPath.append(FS_SEPARATOR);
  searchPath.append("*");
  searchPath.append(extension);

  WIN32_FIND_DATAA findData;
  HANDLE fileHandle = FindFirstFileA(searchPath.c_str(), &findData);

  if (fileHandle != INVALID_HANDLE_VALUE) {
    do {
      // Append the assembly to the list
      tpaList.append(directory);
      tpaList.append(FS_SEPARATOR);
      tpaList.append(findData.cFileName);
      tpaList.append(PATH_DELIMITER);

      // Note that the CLR does not guarantee which assembly will be loaded if
      // an assembly is in the TPA list multiple times (perhaps from different
      // paths or perhaps with different NI/NI.dll extensions. Therefore, a real
      // host should probably add items to the list in priority order and only
      // add a file if it's not already present on the list.
      //
      // For this simple sample, though, and because we're only loading TPA
      // assemblies from a single path, and have no native images, we can ignore
      // that complication.
    } while (FindNextFileA(fileHandle, &findData));
    FindClose(fileHandle);
  }
}

#else
#include <dirent.h>
#include <dlfcn.h>
#include <limits.h>
#define FS_SEPARATOR "/"
#define PATH_DELIMITER ":"
#define MAX_PATH PATH_MAX
#if OSX
// For OSX, use Linux defines except that the CoreCLR runtime
// library has a different name
#define CORECLR_FILE_NAME "libcoreclr.dylib"
#else
#define CORECLR_FILE_NAME "libcoreclr.so"
#endif

typedef void* LibraryHandle;

LibraryHandle LoadLibrary(std::string path) {
  return dlopen(path.c_str(), RTLD_NOW | RTLD_LOCAL);
}

template <typename TFuncPointer>
TFuncPointer GetFunction(LibraryHandle library, const char* name) {
  return reinterpret_cast<TFuncPointer>(dlsym(library, name));
}

void FreeLibrary(LibraryHandle library) { dlclose(library); }

// POSIX directory search for .dll files
void BuildTpaList(const char* directory, const char* extension,
                  std::string& tpaList) {
  DIR* dir = opendir(directory);
  struct dirent* entry;
  int extLength = strlen(extension);

  while ((entry = readdir(dir)) != NULL) {
    // This simple sample doesn't check for symlinks
    std::string filename(entry->d_name);

    // Check if the file has the right extension
    int extPos = filename.length() - extLength;
    if (extPos <= 0 || filename.compare(extPos, extLength, extension) != 0) {
      continue;
    }

    // Append the assembly to the list
    tpaList.append(directory);
    tpaList.append(FS_SEPARATOR);
    tpaList.append(filename);
    tpaList.append(PATH_DELIMITER);

    // Note that the CLR does not guarantee which assembly will be loaded if an
    // assembly is in the TPA list multiple times (perhaps from different paths
    // or perhaps with different NI/NI.dll extensions. Therefore, a real host
    // should probably add items to the list in priority order and only add a
    // file if it's not already present on the list.
    //
    // For this simple sample, though, and because we're only loading TPA
    // assemblies from a single path, and have no native images, we can ignore
    // that complication.
  }
}
#endif

}  // namespace

namespace coreclrhosting {

class DotNetHost::Impl {
 public:
  std::string base_path_;
  LibraryHandle coreclr_;
  // coreclr_initialize_ptr coreclr_initialize_;
  // coreclr_create_delegate_ptr coreclr_create_delegate_;
  coreclr_shutdown_ptr coreclr_shutdown_;
  coreclr_execute_assembly_ptr coreclr_execute_assembly_;

  void* host_handle_;
  unsigned int domain_id_;

  ~Impl() {
    auto hr = coreclr_shutdown_(host_handle_, domain_id_);
    /*if (hr >= 0)
    {
        printf("CoreCLR successfully shutdown\n");
    }
    else
    {
        printf("coreclr_shutdown failed - status: 0x%08x\n", hr);
    }*/
    FreeLibrary(coreclr_);
  }
};

DotNetHost::DotNetHost(std::unique_ptr<Impl> impl) : impl_(std::move(impl)) {}
DotNetHost::~DotNetHost() {}

DotNetHostCreationResult::Enum DotNetHost::Create(
    std::string base_path, std::unique_ptr<DotNetHost>& host) {
  std::string coreclr_path(base_path);
  coreclr_path.append(FS_SEPARATOR);
  coreclr_path.append(CORECLR_FILE_NAME);

  auto coreclr = LoadLibrary(coreclr_path);

  if (NULL == coreclr) return DotNetHostCreationResult::kCoreClrNotFound;

  auto coreclr_initialize =
      GetFunction<coreclr_initialize_ptr>(coreclr, "coreclr_initialize");
  if (NULL == coreclr_initialize) {
    FreeLibrary(coreclr);
    return DotNetHostCreationResult::kInvalidCoreClr;
  }

  auto coreclr_create_delegate = GetFunction<coreclr_create_delegate_ptr>(
      coreclr, "coreclr_create_delegate");
  if (NULL == coreclr_create_delegate) {
    FreeLibrary(coreclr);
    return DotNetHostCreationResult::kInvalidCoreClr;
  }

  auto coreclr_shutdown =
      GetFunction<coreclr_shutdown_ptr>(coreclr, "coreclr_shutdown");
  if (NULL == coreclr_shutdown) {
    FreeLibrary(coreclr);
    return DotNetHostCreationResult::kInvalidCoreClr;
  }

  auto coreclr_execute_assembly = GetFunction<coreclr_execute_assembly_ptr>(
      coreclr, "coreclr_execute_assembly");
  if (NULL == coreclr_execute_assembly) {
    FreeLibrary(coreclr);
    return DotNetHostCreationResult::kInvalidCoreClr;
  }

  // Construct the trusted platform assemblies (TPA) list
  // This is the list of assemblies that .NET Core can load as
  // trusted system assemblies.
  // For this host (as with most), assemblies next to CoreCLR will
  // be included in the TPA list
  std::string tpaList;
  BuildTpaList(base_path.c_str(), ".dll", tpaList);

  // <Snippet3>
  // Define CoreCLR properties
  // Other properties related to assembly loading are common here,
  // but for this simple sample, TRUSTED_PLATFORM_ASSEMBLIES is all
  // that is needed. Check hosting documentation for other common properties.
  const char* propertyKeys[] = {
      "TRUSTED_PLATFORM_ASSEMBLIES"  // Trusted assemblies
  };

  const char* propertyValues[] = {tpaList.c_str()};

  void* host_handle;
  unsigned int domain_id;

  // This function both starts the .NET Core runtime and creates
  // the default (and only) AppDomain
  auto hr = coreclr_initialize(
      base_path.c_str(),                     // App base path
      "NodeHosted",                          // AppDomain friendly name
      sizeof(propertyKeys) / sizeof(char*),  // Property count
      propertyKeys,                          // Property names
      propertyValues,                        // Property values
      &host_handle,                          // Host handle
      &domain_id);                           // AppDomain ID

  if (hr < 0) {
    // printf("coreclr_initialize failed - status: 0x%08x\n", hr);
    FreeLibrary(coreclr);
    return DotNetHostCreationResult::kInitializeFailed;
  }

  auto impl = std::make_unique<DotNetHost::Impl>();
  impl->base_path_ = base_path;
  impl->coreclr_ = coreclr;
  // impl->coreclr_initialize_ = coreclr_initialize;
  // impl->coreclr_create_delegate_ = coreclr_create_delegate;
  impl->coreclr_shutdown_ = coreclr_shutdown;
  impl->coreclr_execute_assembly_ = coreclr_execute_assembly;
  impl->host_handle_ = host_handle;
  impl->domain_id_ = domain_id;

  // As there is no public ctor, we need this instead of std::make_unique:
  host = std::unique_ptr<DotNetHost>(new DotNetHost(std::move(impl)));
  return DotNetHostCreationResult::kOK;
}

DotNetHostExecuteAssemblyResult::Enum DotNetHost::ExecuteAssembly(
    std::string name, int argc, const char** argv, unsigned int& result_code) {
  std::string managed_library_path(impl_->base_path_);
  managed_library_path.append(FS_SEPARATOR);
  managed_library_path.append(name);

  // The assembly name passed in the third parameter is a managed assembly name
  // as described at
  // https://docs.microsoft.com/dotnet/framework/app-domains/assembly-names
  auto hr = impl_->coreclr_execute_assembly_(
      impl_->host_handle_, impl_->domain_id_, argc, argv,
      managed_library_path.c_str(), &result_code);

  if (hr >= 0) {
    // printf("Managed assembly executed\n");
    return DotNetHostExecuteAssemblyResult::kOK;
  } else {
    // printf("coreclr_create_delegate failed - status: 0x%08x\n", hr);
    return DotNetHostExecuteAssemblyResult::kAssemblyNotFound;
  }
}

}  // namespace coreclrhosting