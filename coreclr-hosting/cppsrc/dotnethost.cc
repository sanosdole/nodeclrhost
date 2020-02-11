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
#define CORECLR_FILE_NAME L"coreclr.dll"

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

string_t GetDirectoryFromFilePath(const string_t &assembly) {
  auto found = assembly.find_last_of(STR("/\\"));
  return assembly.substr(0, found);
}

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
    // printf("get_hostfxr_path returned %d\n", rcPath);
    return nullptr;
  }
  wprintf(L"Hostfxr found at %s\n", buffer);
  hostfxr_path_out = GetDirectoryFromFilePath(buffer);

  // Load hostfxr and get desired exports
  return load_library(buffer);
}

}  // namespace
namespace coreclrhosting {

class DotNetHost::Impl {
 public:
  std::vector<std::string> arguments_;
  coreclr_execute_assembly_ptr coreclr_execute_assembly_;
  coreclr_shutdown_ptr coreclr_shutdown_;
  void *host_handle_;
  unsigned int domain_id_;
  hostfxr_handle context_;
  // hostfxr_run_app_fn run_fptr_;
  hostfxr_close_fn close_fptr_;
  LibraryHandle hostfxr_lib_;
  LibraryHandle coreclr_lib_;

  ~Impl() {
    coreclr_shutdown_(host_handle_, domain_id_);
    free_library(coreclr_lib_);
    close_fptr_(context_);
    free_library(hostfxr_lib_);
  }
};

DotNetHost::DotNetHost(std::unique_ptr<Impl> impl) : impl_(std::move(impl)) {}
DotNetHost::~DotNetHost() {}

DotNetHostCreationResult::Enum DotNetHost::Create(
    const std::vector<std::string> &arguments,
    std::unique_ptr<DotNetHost> &host) {
  /*if (arguments.size() < 1)
    throw std::invalid_argument(
        "At least the path to an assembly is required!");*/
  auto assembly_path = arguments[0];
#ifdef WINDOWS
  std::replace(assembly_path.begin(), assembly_path.end(), u8'/', u8'\\');
#else
  std::replace(assembly_path.begin(), assembly_path.end(), u8'\\', u8'/');
#endif

  if (!FileExists(assembly_path))
    return DotNetHostCreationResult::kAssemblyNotFound;

  string_t hostfxr_path;
  auto lib = LoadHostfxr(assembly_path, hostfxr_path);
  if (nullptr == lib) return DotNetHostCreationResult::kCoreClrNotFound;

  auto init_fptr = GetFunction<hostfxr_initialize_for_dotnet_command_line_fn>(
      lib, "hostfxr_initialize_for_dotnet_command_line");
  // auto run_fptr = GetFunction<hostfxr_run_app_fn>(lib, "hostfxr_run_app");
  auto close_fptr = GetFunction<hostfxr_close_fn>(lib, "hostfxr_close");

  auto get_prop_fptr = GetFunction<hostfxr_get_runtime_property_value_fn>(
      lib, "hostfxr_get_runtime_property_value");
  // auto set_prop_fptr =
  // GetFunction<hostfxr_set_runtime_property_value_fn>(lib,
  // "hostfxr_set_runtime_property_value");
  auto get_props_fptr = GetFunction<hostfxr_get_runtime_properties_fn>(
      lib, "hostfxr_get_runtime_properties");

  if (!init_fptr /*|| !run_fptr*/ || !close_fptr || !get_prop_fptr ||
      !get_props_fptr) {
    free_library(lib);
    return DotNetHostCreationResult::kInvalidCoreClr;
  }

  // Create proper arguments array
  auto in_size = arguments.size();
  std::vector<string_t> final_arguments(in_size);
  std::vector<const char_t *> final_arguments_char(in_size);
  // Is there a LINQ equivalent for this?
  for (auto i = 0u; i < in_size; i++) {
    final_arguments[i] =
        StringTFromUtf8(i == 0u ? assembly_path : arguments[i]);
    final_arguments_char[i] = final_arguments[i].c_str();
  }

  // Load context for running
  hostfxr_handle cxt = nullptr;
  auto rc = init_fptr(final_arguments.size(), final_arguments_char.data(),
                      nullptr, &cxt);
  if (rc != 0 || cxt == nullptr) {
    std::cerr << "Init '" << assembly_path << "' failed: " << std::hex
              << std::showbase << rc << std::endl;
    close_fptr(cxt);
    free_library(lib);
    return DotNetHostCreationResult::kInitializeFailed;
  }

  // Log properties
  size_t prop_count = 64;
  const char_t *prop_keys[64];
  const char_t *prop_values[64];
  rc = get_props_fptr(cxt, &prop_count, prop_keys, prop_values);
  if (0 != rc) {
    std::cerr << "hostfxr did not set up properties" << std::hex
              << std::showbase << rc << std::endl;
    close_fptr(cxt);
    free_library(lib);
    return DotNetHostCreationResult::kInvalidCoreClr;
  }

  // Due to hostpolicy.cpp:255 shutting down the coreclr after running we load
  // coreclr manually
  const char_t *jit_path_buffer;
  rc = get_prop_fptr(cxt, STR("JIT_PATH"), &jit_path_buffer);
  if (0 != rc) {
    std::cerr << "hostfxr did not set up JIT_PATH, which is required for "
                 "locating coreclr"
              << std::hex << std::showbase << rc << std::endl;
    close_fptr(cxt);
    free_library(lib);
    return DotNetHostCreationResult::kInvalidCoreClr;
  }
  string_t jit_path = jit_path_buffer;

  auto coreclr_base_path = GetDirectoryFromFilePath(jit_path);
  auto coreclr_path = coreclr_base_path + DIR_SEPARATOR + CORECLR_FILE_NAME;
  auto coreclr_lib = load_library(coreclr_path.c_str());
  if (nullptr == coreclr_lib) {
    std::cerr << "No coreclr found at: "
              << StringUtf8FromT(coreclr_path).c_str() << std::endl;
    close_fptr(cxt);
    free_library(lib);
    return DotNetHostCreationResult::kInvalidCoreClr;
  }

  auto coreclr_initialize =
      GetFunction<coreclr_initialize_ptr>(coreclr_lib, "coreclr_initialize");
  auto coreclr_shutdown =
      GetFunction<coreclr_shutdown_ptr>(coreclr_lib, "coreclr_shutdown");
  auto coreclr_execute_assembly = GetFunction<coreclr_execute_assembly_ptr>(
      coreclr_lib, "coreclr_execute_assembly");

  if (!coreclr_initialize || !coreclr_shutdown || !coreclr_execute_assembly) {
    std::cerr << "Invalid coreclr found at: "
              << StringUtf8FromT(coreclr_path).c_str() << std::endl;
    free_library(coreclr_lib);
    close_fptr(cxt);
    free_library(lib);
    return DotNetHostCreationResult::kInvalidCoreClr;
  }

  std::vector<std::string> clr_prop_keys_string(prop_count);
  std::vector<const char *> clr_prop_keys(prop_count);
  std::vector<std::string> clr_prop_values_string(prop_count);
  std::vector<const char *> clr_prop_values(prop_count);

  for (auto i = 0u; i < prop_count; i++) {
    clr_prop_keys_string[i] = StringUtf8FromT(prop_keys[i]);
    clr_prop_keys[i] = clr_prop_keys_string[i].c_str();

    clr_prop_values_string[i] = StringUtf8FromT(prop_values[i]);
    clr_prop_values[i] = clr_prop_values_string[i].c_str();

    // printf("%d %s = %s\n", i, clr_prop_keys[i], clr_prop_values[i]);
  }

  void *host_handle = nullptr;
  unsigned int domain_id = -1;
  rc = coreclr_initialize(assembly_path.c_str(), "nodehost", prop_count,
                          clr_prop_keys.data(), clr_prop_values.data(),
                          &host_handle, &domain_id);
  if (0 != rc) {
    std::cerr << "Could not initialize coreclr found at: "
              << StringUtf8FromT(coreclr_path).c_str() << std::endl;
    free_library(coreclr_lib);
    close_fptr(cxt);
    free_library(lib);
    return DotNetHostCreationResult::kInvalidCoreClr;
  }

  auto impl = std::make_unique<DotNetHost::Impl>();
  impl->coreclr_lib_ = coreclr_lib;
  impl->arguments_ = arguments;
  impl->host_handle_ = host_handle;
  impl->domain_id_ = domain_id;
  impl->coreclr_execute_assembly_ = coreclr_execute_assembly;
  impl->coreclr_shutdown_ = coreclr_shutdown;
  impl->context_ = cxt;
  // impl->run_fptr_ = run_fptr;
  impl->close_fptr_ = close_fptr;
  impl->hostfxr_lib_ = lib;

  // As there is no public ctor, we need this instead of std::make_unique:
  host = std::unique_ptr<DotNetHost>(new DotNetHost(std::move(impl)));
  return DotNetHostCreationResult::kOK;
}

int32_t DotNetHost::ExecuteAssembly() {
  // Due to hostpolicy.cpp:255 this will shutdown the coreclr once
  // finished. Instead load/initialize coreclr and execute it manually (using
  // props from context)
  // return impl_->run_fptr_(impl_->context_);

  std::vector<const char *> arguments(impl_->arguments_.size() - 1);
  for (auto i = 0; i < arguments.size(); i++) {
    arguments[i] = impl_->arguments_[i + 1].c_str();
  }

  unsigned int exit_code = 0;
  auto rc = impl_->coreclr_execute_assembly_(
      impl_->host_handle_, impl_->domain_id_, arguments.size(),
      arguments.data(), impl_->arguments_[0].c_str(), &exit_code);

  if (0 != rc) {
    std::cerr << "Failed to execute assembly: " << impl_->arguments_[0].c_str()
              << std::endl;
  }
  return exit_code;
}

}  // namespace coreclrhosting