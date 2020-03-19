#include "dotnethost.h"

// Standard headers
#include <assert.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <algorithm>
#include <fstream>
#include <iostream>
#include <limits>
#include <vector>

// Provided by the AppHost NuGet package and installed as an SDK pack
#include <nethost.h>

// Header files copied from https://github.com/dotnet/core-setup
#include <coreclr_delegates.h>
#include <hostfxr.h>

// Header file copied from
// https://github.com/dotnet/runtime/blob/master/src/coreclr/src/hosts/inc/coreclrhost.h
#include <coreclrhost.h>

#ifdef WINDOWS
#include <Windows.h>

#define STR(s) L##s
#define CH(c) L##c
#define DIR_SEPARATOR L'\\'

#else

#include <dlfcn.h>
#include <limits.h>

#define STR(s) s
#define CH(c) c
#define DIR_SEPARATOR '/'
#define MAX_PATH PATH_MAX

#if OSX
#define CORECLR_FILE_NAME "libcoreclr.dylib"
#else
#define CORECLR_FILE_NAME "libcoreclr.so"
#endif

#endif

using string_t = std::basic_string<char_t>;

#ifdef WINDOWS

string_t StringTFromUtf8(const std::string &utf8) {
  string_t utf16;

  if (utf8.empty()) {
    return utf16;
  }

  // Safely fails if an invalid UTF-8 character
  // is encountered in the input string
  constexpr DWORD kFlags = MB_ERR_INVALID_CHARS;

  /*if (utf8.length() > static_cast<size_t>((std::numeric_limits<int>::max)()))
  { throw std::overflow_error( "Input string too long: size_t-length doesn't fit
  into int.");
  }*/

  // Safely convert from size_t (STL string's length)
  // to int (for Win32 APIs)
  const int utf8Length = static_cast<int>(utf8.length());

  const int utf16Length = ::MultiByteToWideChar(
      CP_UTF8,      // Source string is in UTF-8
      kFlags,       // Conversion flags
      utf8.data(),  // Source UTF-8 string pointer
      utf8Length,   // Length of the source UTF-8 string, in chars
      nullptr,      // Unused - no conversion done in this step
      0             // Request size of destination buffer, in wchar_ts
  );

  /*if (utf16Length == 0) {
    // Conversion error: capture error code and throw
    // const DWORD error = ::GetLastError();
    throw std::runtime_error(
        "Cannot get result string length when converting "
        "from UTF-8 to UTF-16 (MultiByteToWideChar failed).");
  }*/

  utf16.resize(utf16Length);

  // Convert from UTF-8 to UTF-16
  int result = ::MultiByteToWideChar(
      CP_UTF8,      // Source string is in UTF-8
      kFlags,       // Conversion flags
      utf8.data(),  // Source UTF-8 string pointer
      utf8Length,   // Length of source UTF-8 string, in chars
      &utf16[0],    // Pointer to destination buffer
      utf16Length   // Size of destination buffer, in wchar_ts
  );

  /*if (result == 0) {
    // Conversion error: capture error code and throw
    // const DWORD error = ::GetLastError();
    throw std::runtime_error(
        "Cannot get result string length when converting "
        "from UTF-8 to UTF-16 (MultiByteToWideChar failed).");
  }*/

  return utf16;
}

std::string StringUtf8FromT(const string_t &str_t) {
  std::string utf8;

  if (str_t.empty()) {
    return utf8;
  }

  // Safely convert from size_t (STL string's length)
  // to int (for Win32 APIs)
  const int tLength = static_cast<int>(str_t.length());

  const int utf8Length = ::WideCharToMultiByte(
      CP_UTF8,        // Target string is in UTF-8
      0,              // Conversion flags
      str_t.c_str(),  // Source UTF-16 string pointer
      -1,             // Length of the source UTF-16 string, in chars
      nullptr,        // Unused - no conversion done in this step
      0,              // Request size of destination buffer, in wchar_ts,
      nullptr, nullptr);

  utf8.resize(utf8Length);

  // Convert from UTF-8 to UTF-16
  int result = ::WideCharToMultiByte(
      CP_UTF8,       // Target string is in UTF-8
      0,             // Conversion flags
      str_t.data(),  // Source UTF-16 string pointer
      tLength,       // Length of the source UTF-16 string, in chars
      &utf8[0],      // Pointer to destination buffer
      utf8.size(),   // Size of destination buffer, in wchar_ts
      nullptr, nullptr);

  return utf8;
}

#else

string_t StringTFromUtf8(const std::string &utf8) { return utf8; }
std::string StringUtf8FromT(const string_t &str_t) { return str_t; }
#endif

namespace {

#ifdef WINDOWS

typedef HMODULE LibraryHandle;

LibraryHandle load_library(const char_t *path) {
  auto h = ::LoadLibraryW(path);
  assert(h != nullptr);
  return h;
}
void *get_export(LibraryHandle h, const char *name) {
  void *f = ::GetProcAddress(h, name);
  assert(f != nullptr);
  return f;
}
void free_library(LibraryHandle h) { FreeLibrary(h); }

#else

typedef void *LibraryHandle;

void *load_library(const char_t *path) {
  void *h = dlopen(path, RTLD_LAZY | RTLD_LOCAL);
  assert(h != nullptr);
  return h;
}
void *get_export(LibraryHandle h, const char *name) {
  void *f = dlsym(h, name);
  assert(f != nullptr);
  return f;
}
void free_library(LibraryHandle h) { dlclose(h); }
#endif

template <typename TFuncPointer>
TFuncPointer GetFunction(LibraryHandle library, const char *name) {
  return reinterpret_cast<TFuncPointer>(get_export(library, name));
}

inline bool FileExists(const std::string &name) {
  std::ifstream f(name);
  return f.good();
}

std::string GetDirectoryFromFilePath(const std::string &assembly) {
  auto found = assembly.find_last_of("/\\");
  return assembly.substr(0, found);
}

#ifdef WINDOWS
string_t GetDirectoryFromFilePath(const string_t &assembly) {
  auto found = assembly.find_last_of(STR("/\\"));
  return assembly.substr(0, found);
}
#endif

LibraryHandle LoadHostfxr(const std::string &assembly,
                          string_t &hostfxr_path_out) {
  auto base_path = GetDirectoryFromFilePath(assembly);

  // Pre-allocate a large buffer for the path to hostfxr
  char_t buffer[MAX_PATH];
  size_t buffer_size = sizeof(buffer) / sizeof(char_t);

  get_hostfxr_parameters params;
  params.size = sizeof(get_hostfxr_parameters);
  params.dotnet_root = nullptr;
  // ATTENTION: This needs proper path seperators or it will fail for self
  // contained
  auto assembly_t = StringTFromUtf8(assembly);
  params.assembly_path = assembly_t.c_str();

  auto rcPath = get_hostfxr_path(buffer, &buffer_size, &params);
  if (rcPath != 0) {    
    return nullptr;
  }
  
  hostfxr_path_out = GetDirectoryFromFilePath(buffer);

  // Load hostfxr and get desired exports
  return load_library(buffer);
}

}  // namespace
namespace coreclrhosting {

extern "C" typedef int32_t(*EntryPointPtr)(int argc, const char *argv[]);

class DotNetHost::Impl {
 public:
  EntryPointPtr entry_point_;
  hostfxr_handle context_;  
  hostfxr_close_fn close_fptr_;
  LibraryHandle hostfxr_lib_;  

  ~Impl() {    
    close_fptr_(context_);
    free_library(hostfxr_lib_);
  }
};

DotNetHost::DotNetHost(std::unique_ptr<Impl> impl) : impl_(std::move(impl)) {}
DotNetHost::~DotNetHost() {}

DotNetHostCreationResult::Enum DotNetHost::Create(
    std::string assembly_path,
    std::unique_ptr<DotNetHost> &host) {
#ifdef WINDOWS
  std::replace(assembly_path.begin(), assembly_path.end(), u8'/', u8'\\');
#else
  std::replace(assembly_path.begin(), assembly_path.end(), '\\', '/');
#endif
  
  auto runtime_config = assembly_path;
#ifdef WINDOWS
  auto dll_index = runtime_config.find_last_of(u8".dll");
#else
  auto dll_index = runtime_config.find_last_of(".dll");
#endif  
  if (dll_index == std::string::npos)
    return DotNetHostCreationResult::kAssemblyNotFound;
#ifdef WINDOWS
  runtime_config = runtime_config.substr(0, dll_index - 3) + u8".runtimeconfig.json";  
#else
  runtime_config = runtime_config.substr(0, dll_index - 3) + ".runtimeconfig.json";
#endif  
  
  if (!FileExists(assembly_path) || !FileExists(runtime_config))
    return DotNetHostCreationResult::kAssemblyNotFound;

  string_t hostfxr_path;
  auto lib = LoadHostfxr(assembly_path, hostfxr_path);
  if (nullptr == lib) return DotNetHostCreationResult::kCoreClrNotFound;

  auto init_fptr = GetFunction<hostfxr_initialize_for_runtime_config_fn>(
      lib, "hostfxr_initialize_for_runtime_config");
  auto get_runtime_delegate_fptr = GetFunction<hostfxr_get_runtime_delegate_fn>(lib, "hostfxr_get_runtime_delegate");
  auto close_fptr = GetFunction<hostfxr_close_fn>(lib, "hostfxr_close");  

  if (!init_fptr || !get_runtime_delegate_fptr || !close_fptr) {
    free_library(lib);
    return DotNetHostCreationResult::kInvalidCoreClr;
  }

  // Load context
  hostfxr_handle cxt = nullptr;
  auto runtime_config_t = StringTFromUtf8(runtime_config);
  auto rc = init_fptr(runtime_config_t.c_str(), nullptr, &cxt);
  if (rc != 0 || cxt == nullptr) {
    std::cerr << "Init '" << assembly_path << "' failed: " << std::hex
              << std::showbase << rc << std::endl;
    close_fptr(cxt);
    free_library(lib);
    return DotNetHostCreationResult::kInitializeFailed;
  }

  // Get runtime delegate for resolving delegates
  load_assembly_and_get_function_pointer_fn load_assembly_and_get_function_fptr = nullptr;
  rc = get_runtime_delegate_fptr(
            cxt,
            hdt_load_assembly_and_get_function_pointer,
            reinterpret_cast<void**>(&load_assembly_and_get_function_fptr));
    if (rc != 0 || load_assembly_and_get_function_fptr == nullptr) {
        std::cerr << "Get delegate failed: " << std::hex << std::showbase << rc << std::endl;
        close_fptr(cxt);
    free_library(lib);
    return DotNetHostCreationResult::kInitializeFailed;
    }

  // Resolve NodeHostEnvironment entry point
  auto assembly_path_t = StringTFromUtf8(assembly_path);
  EntryPointPtr entry_point = nullptr; 
  rc = load_assembly_and_get_function_fptr(
        assembly_path_t.c_str(),
        STR("NodeHostEnvironment.NativeHost.NativeEntryPoint, NodeHostEnvironment"),
        STR("RunHostedApplication"),
        STR("NodeHostEnvironment.NativeHost.EntryPointSignature, NodeHostEnvironment"),
        nullptr,
        (void**)&entry_point);
  if (rc != 0 || entry_point == nullptr) {
        std::cerr << "Get delegate failed: " << std::hex << std::showbase << rc << std::endl;
        close_fptr(cxt);
    free_library(lib);
    return DotNetHostCreationResult::kInitializeFailed;
    }

  auto impl = std::make_unique<DotNetHost::Impl>();
  impl->entry_point_ = entry_point;  
  impl->context_ = cxt;  
  impl->close_fptr_ = close_fptr;
  impl->hostfxr_lib_ = lib;

  // As there is no public ctor, we need this instead of std::make_unique:
  host = std::unique_ptr<DotNetHost>(new DotNetHost(std::move(impl)));
  return DotNetHostCreationResult::kOK;
}

int32_t DotNetHost::ExecuteAssembly(const std::vector<std::string> &arguments) {
  std::vector<const char *> final_arguments(arguments.size() - 1);
  for (auto i = 0u; i < final_arguments.size(); i++) {
    final_arguments[i] = arguments[i + 1].c_str();
  }

  return impl_->entry_point_(final_arguments.size(), final_arguments.data());
}

}  // namespace coreclrhosting