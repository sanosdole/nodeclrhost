#include "dotnethost.h"

// Standard headers
#include <assert.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <fstream>
#include <iostream>
#include <limits>
#include <vector>

// Provided by the AppHost NuGet package and installed as an SDK pack
#include <nethost.h>

// Header files copied from https://github.com/dotnet/core-setup
#include <coreclr_delegates.h>
#include <hostfxr.h>

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

#else

string_t StringTFromUtf8(const std::string &utf8) { return utf8; }
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

LibraryHandle LoadHostfxr(const std::string &assembly) {
  // TODO: Does not work for self hosted :(
  //       => System.BadImageFormatException: Could not load file or assembly
  //       'XXX\coreclr.dll'.
  //                                          The module was expected to contain
  //                                          an assembly manifest.
  /*auto base_path =  GetDirectoryFromFilePath(assembly);
  std::string hostfxr_library_name = u8"hostfxr.dll";
  auto probe = base_path.append(hostfxr_library_name);
  if (FileExists(probe.u8string())) {
    return load_library(probe.c_str());
  }*/

  // Pre-allocate a large buffer for the path to hostfxr
  char_t buffer[MAX_PATH];
  size_t buffer_size = sizeof(buffer) / sizeof(char_t);

  get_hostfxr_parameters params;
  params.size = sizeof(get_hostfxr_parameters);
  params.dotnet_root = nullptr;
  auto assembly_t = StringTFromUtf8(assembly);
  params.assembly_path = assembly_t.c_str();

  // TODO: Also does not work for self hosted :(
  //       => get_hostfxr_path does not return a path
  /*
  if (FileExists(probe.u8string())) {
    params.dotnet_root = base_path.c_str();
  }*/

  auto rcPath = get_hostfxr_path(buffer, &buffer_size, &params);
  if (rcPath != 0) {
    // printf("get_hostfxr_path returned %d\n", rcPath);
    return nullptr;
  }
  // wprintf(L"Hostfxr found at %s\n", buffer);

  // Load hostfxr and get desired exports
  return load_library(buffer);
}

}  // namespace
namespace coreclrhosting {

class DotNetHost::Impl {
 public:
  hostfxr_handle context_;
  hostfxr_run_app_fn run_fptr_;
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
    const std::vector<std::string> &arguments,
    std::unique_ptr<DotNetHost> &host) {
  /*if (arguments.size() < 1)
    throw std::invalid_argument(
        "At least the path to an assembly is required!");*/

  // printf("Checking for %s\n", arguments[0].c_str());
  if (!FileExists(arguments[0]))
    return DotNetHostCreationResult::kAssemblyNotFound;

  auto lib = LoadHostfxr(arguments[0]);
  if (nullptr == lib) return DotNetHostCreationResult::kCoreClrNotFound;

  auto init_fptr = GetFunction<hostfxr_initialize_for_dotnet_command_line_fn>(
      lib, "hostfxr_initialize_for_dotnet_command_line");
  auto run_fptr = GetFunction<hostfxr_run_app_fn>(lib, "hostfxr_run_app");
  auto close_fptr = GetFunction<hostfxr_close_fn>(lib, "hostfxr_close");

  if (!init_fptr || !run_fptr || !close_fptr) {
    free_library(lib);
    return DotNetHostCreationResult::kInvalidCoreClr;
  }

  // Create proper arguments array
  auto in_size = arguments.size();
  std::vector<string_t> final_arguments(in_size);
  std::vector<const char_t *> final_arguments_char(in_size);
  // Is there a LINQ equivalent for this?
  for (auto i = 0u; i < in_size; i++) {
    final_arguments[i] = StringTFromUtf8(arguments[i]);
    final_arguments_char[i] = final_arguments[i].c_str();
    // wprintf(L"Argument %d %s\n", i, final_arguments_char[i]);
  }

  // Load runtime and assembly
  hostfxr_handle cxt = nullptr;
  auto rc = init_fptr(final_arguments.size(), final_arguments_char.data(),
                      nullptr, &cxt);
  if (rc != 0 || cxt == nullptr) {
    std::cerr << "Init failed: " << std::hex << std::showbase << rc
              << std::endl;
    close_fptr(cxt);
    free_library(lib);
    return DotNetHostCreationResult::kInitializeFailed;
  }

  auto impl = std::make_unique<DotNetHost::Impl>();
  impl->context_ = cxt;
  impl->run_fptr_ = run_fptr;
  impl->close_fptr_ = close_fptr;
  impl->hostfxr_lib_ = lib;
  /*auto impl = std::make_unique<DotNetHost::Impl>();
  impl->base_path_ = base_path;
  impl->coreclr_ = coreclr;
  // impl->coreclr_initialize_ = coreclr_initialize;
  // impl->coreclr_create_delegate_ = coreclr_create_delegate;
  impl->coreclr_shutdown_ = coreclr_shutdown;
  impl->coreclr_execute_assembly_ = coreclr_execute_assembly;
  impl->host_handle_ = host_handle;
  impl->domain_id_ = domain_id;*/

  // As there is no public ctor, we need this instead of std::make_unique:
  host = std::unique_ptr<DotNetHost>(new DotNetHost(std::move(impl)));
  return DotNetHostCreationResult::kOK;
}

int32_t DotNetHost::ExecuteAssembly() {
  return impl_->run_fptr_(impl_->context_);
}

}  // namespace coreclrhosting