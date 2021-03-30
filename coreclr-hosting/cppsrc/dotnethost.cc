#include "dotnethost.h"

// Standard headers
#include <assert.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include <algorithm>
#include <condition_variable>
#include <fstream>
#include <iostream>
#include <limits>
#include <mutex>
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
#define DIR_SEPARATOR '\\'

#define CORECLR_FILE_NAME "coreclr.dll"

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

/*void HOSTFXR_CALLTYPE WriteTrace(const char_t *message) {
  printf(StringUtf8FromT(message).c_str());
}*/

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

class DotNetHost::Impl {
  Impl(string_t assembly_path_t)
      : initialization_done_(false),
        assembly_path_t_(assembly_path_t),
        hostfxr_lib_(nullptr),
        load_assembly_and_get_function_(nullptr),
        context_(nullptr),
        close_fptr_(nullptr),
        coreclr_lib_(nullptr),
        coreclr_host_handle_(nullptr),
        coreclr_domain_id_(0),
        coreclr_create_delegate_(nullptr),
        coreclr_shutdown_(nullptr) {}

  std::condition_variable initialized_;
  std::mutex init_mutex_;
  bool initialization_done_;

 public:
  string_t assembly_path_t_;

  LibraryHandle hostfxr_lib_;
  load_assembly_and_get_function_pointer_fn load_assembly_and_get_function_;
  hostfxr_handle context_;
  hostfxr_close_fn close_fptr_;

  LibraryHandle coreclr_lib_;
  void *coreclr_host_handle_;
  unsigned int coreclr_domain_id_;
  coreclr_create_delegate_ptr coreclr_create_delegate_;
  coreclr_shutdown_ptr coreclr_shutdown_;

  void *GetManagedFunction(std::string type_name, std::string method_name,
                           std::string signature_delegate_name) {
    void *result = nullptr;
    if (load_assembly_and_get_function_) {
      // Installed FX using hostfx
      auto type_name_t = StringTFromUtf8(type_name);
      auto method_name_t = StringTFromUtf8(method_name);
      auto signature_delegate_name_t = StringTFromUtf8(signature_delegate_name);
      auto rc = load_assembly_and_get_function_(
          assembly_path_t_.c_str(), type_name_t.c_str(), method_name_t.c_str(),
          signature_delegate_name_t.c_str(), nullptr, (void **)&result);
      if (rc != 0) {
        std::cerr << "Get delegate failed: " << std::hex << std::showbase << rc
                  << std::endl;

        return nullptr;
      }

    } else if (coreclr_create_delegate_) {
      // SCD use case
      auto split_pos = type_name.find(',');
      auto short_type_name = type_name.substr(0, split_pos);
      auto assembly_name =
          type_name.substr(split_pos + 1, type_name.length() - (split_pos + 1));
      auto rc = coreclr_create_delegate_(
          coreclr_host_handle_, coreclr_domain_id_, assembly_name.c_str(),
          short_type_name.c_str(), method_name.c_str(), &result);
      if (rc != 0) {
        std::cerr << "Get delegate " << assembly_name.c_str() << " "
                  << short_type_name.c_str() << " failed: " << std::hex
                  << std::showbase << rc << std::endl;

        return nullptr;
      }
    } else {
      std::cerr
          << "Neither standard nor self-contained 3.1 workaround is configured!"
          << std::endl;
    }
    return result;
  }

  ~Impl() {    
    // TODO DM 26.03.2021: Cleanup crashes on nix ci server in tests :(
#ifdef WINDOWS
    if (coreclr_lib_) {
      coreclr_shutdown_(coreclr_host_handle_, coreclr_domain_id_);
      free_library(coreclr_lib_);
    }

    close_fptr_(context_);
    free_library(hostfxr_lib_);
#endif
  }

  static std::shared_ptr<Impl> Instance(string_t assembly_path_t,
                                        bool &created) {
    static std::mutex lock;
    static std::shared_ptr<Impl> shared;

    std::lock_guard<std::mutex> guard(lock);

    // Get an existing instance from the weak reference, if possible.
    if (auto instance = shared /*.lock()*/) {
      std::unique_lock<std::mutex> lock(instance->init_mutex_);
      instance->initialized_.wait(
          lock, [instance] { return instance->initialization_done_; });
      created = false;
      return instance;
    }

    // Create a new instance and keep a weak reference.
    // Global state will be cleaned up when last thread exits.
    auto instance = std::shared_ptr<Impl>(new Impl(assembly_path_t));
    shared = instance;
    created = true;
    return instance;
  }

  void SetInitialized() {
    std::lock_guard<std::mutex> lock(init_mutex_);
    initialization_done_ = true;
    initialized_.notify_all();
  }
};

DotNetHost::DotNetHost(std::shared_ptr<Impl> impl) : impl_(impl) {}
DotNetHost::~DotNetHost() {}

DotNetHostCreationResult::Enum DotNetHost::Create(
    std::string assembly_path, std::unique_ptr<DotNetHost> &host) {
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
  runtime_config =
      runtime_config.substr(0, dll_index - 3) + u8".runtimeconfig.json";
#else
  runtime_config =
      runtime_config.substr(0, dll_index - 3) + ".runtimeconfig.json";
#endif

  if (!FileExists(assembly_path) || !FileExists(runtime_config))
    return DotNetHostCreationResult::kAssemblyNotFound;

  auto assembly_path_t = StringTFromUtf8(assembly_path);
  bool created;
  auto impl = Impl::Instance(assembly_path_t, created);
  if (impl->assembly_path_t_ != assembly_path_t)
    return DotNetHostCreationResult::kReinitializationNotSupported;
  if (!created) {
    host = std::unique_ptr<DotNetHost>(new DotNetHost(impl));
    return DotNetHostCreationResult::kOK;
  }

  string_t hostfxr_path;
  auto lib = LoadHostfxr(assembly_path, hostfxr_path);
  if (nullptr == lib) return DotNetHostCreationResult::kCoreClrNotFound;

  /*auto set_writer_fptr = GetFunction<hostfxr_set_error_writer_fn>(
      lib, "hostfxr_set_error_writer");
    set_writer_fptr(&WriteTrace);*/

  // TODO DM 08.06.2020: Switch to initialize_for_command_line once .NET 5 is
  // available
  auto init_fptr = GetFunction<hostfxr_initialize_for_runtime_config_fn>(
      lib, "hostfxr_initialize_for_runtime_config");
  auto get_runtime_delegate_fptr = GetFunction<hostfxr_get_runtime_delegate_fn>(
      lib, "hostfxr_get_runtime_delegate");
  auto close_fptr = GetFunction<hostfxr_close_fn>(lib, "hostfxr_close");

  if (!init_fptr || !get_runtime_delegate_fptr || !close_fptr) {
    free_library(lib);
    return DotNetHostCreationResult::kInvalidCoreClr;
  }

  // Load context
  auto base_path = GetDirectoryFromFilePath(assembly_path);
  auto base_path_t = StringTFromUtf8(base_path);

  hostfxr_handle cxt = nullptr;
  auto runtime_config_t = StringTFromUtf8(runtime_config);
  /*hostfxr_initialize_parameters init_params;
  init_params.size = sizeof(hostfxr_initialize_parameters);
  init_params.host_path = base_path_t.c_str();
  init_params.dotnet_root = nullptr;*/
  auto rc = init_fptr(runtime_config_t.c_str(), nullptr /*&init_params*/, &cxt);
  if (rc != 0 || cxt == nullptr) {
    // See
    // <https://github.com/dotnet/runtime/blob/master/docs/design/features/host-error-codes.md>
    // why this signals self-contained deployment
    if (rc == 0x80008093) {
      auto coreclr_path = base_path + DIR_SEPARATOR + CORECLR_FILE_NAME;
      if (FileExists(coreclr_path)) {
        // Handle SCD for dotnet 3.1 using the load coreclr dll workaround
        auto coreclr_path_t = StringTFromUtf8(coreclr_path);
        auto init2_fptr =
            GetFunction<hostfxr_initialize_for_dotnet_command_line_fn>(
                lib, "hostfxr_initialize_for_dotnet_command_line");
        auto get_props_fptr = GetFunction<hostfxr_get_runtime_properties_fn>(
            lib, "hostfxr_get_runtime_properties");

        if (!init2_fptr || !get_props_fptr) {
          free_library(lib);
          return DotNetHostCreationResult::kInvalidCoreClr;
        }

        // Load context for running
        hostfxr_handle cxt = nullptr;
        auto arguments = assembly_path_t.c_str();
        auto rc = init2_fptr(1, &arguments, nullptr, &cxt);
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

        auto coreclr_lib = load_library(coreclr_path_t.c_str());
        if (nullptr == coreclr_lib) {
          std::cerr << "No coreclr found at: " << coreclr_path.c_str()
                    << std::endl;
          close_fptr(cxt);
          free_library(lib);
          return DotNetHostCreationResult::kInvalidCoreClr;
        }

        auto coreclr_initialize = GetFunction<coreclr_initialize_ptr>(
            coreclr_lib, "coreclr_initialize");
        auto coreclr_shutdown =
            GetFunction<coreclr_shutdown_ptr>(coreclr_lib, "coreclr_shutdown");
        auto coreclr_create_delegate = GetFunction<coreclr_create_delegate_ptr>(
            coreclr_lib, "coreclr_create_delegate");

        if (!coreclr_initialize || !coreclr_shutdown ||
            !coreclr_create_delegate) {
          std::cerr << "Invalid coreclr found at: " << coreclr_path.c_str()
                    << std::endl;
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
                    << coreclr_path.c_str() << std::endl;
          free_library(coreclr_lib);
          close_fptr(cxt);
          free_library(lib);
          return DotNetHostCreationResult::kInvalidCoreClr;
        }

        impl->load_assembly_and_get_function_ = nullptr;
        impl->context_ = cxt;
        impl->close_fptr_ = close_fptr;
        impl->hostfxr_lib_ = lib;
        impl->coreclr_lib_ = coreclr_lib;
        impl->coreclr_host_handle_ = host_handle;
        impl->coreclr_domain_id_ = domain_id;
        impl->coreclr_create_delegate_ = coreclr_create_delegate;
        impl->coreclr_shutdown_ = coreclr_shutdown;
        impl->SetInitialized();

        // As there is no public ctor, we need this instead of std::make_unique:
        host = std::unique_ptr<DotNetHost>(new DotNetHost(impl));
        return DotNetHostCreationResult::kOK;
      }
    }

    std::cerr << "Init '" << assembly_path << "' failed: " << std::hex
              << std::showbase << rc << std::endl;
    close_fptr(cxt);
    free_library(lib);
    return DotNetHostCreationResult::kInitializeFailed;
  }

  auto set_runtime_property_value_fptr =
      GetFunction<hostfxr_set_runtime_property_value_fn>(
          lib, "hostfxr_set_runtime_property_value");
  /*auto get_runtime_property_value_fptr =
     GetFunction<hostfxr_get_runtime_property_value_fn>( lib,
     "hostfxr_get_runtime_property_value");
  const char_t *prop_buffer = nullptr;  // new char_t[1024];*/

  /*rc = get_runtime_property_value_fptr(cxt, STR("APP_CONTEXT_BASE_DIRECTORY"),
  &prop_buffer); if (rc != 0) wprintf(STR("APP_CONTEXT_BASE_DIRECTORY: %s\n"),
  prop_buffer);*/
  set_runtime_property_value_fptr(cxt, STR("APP_CONTEXT_BASE_DIRECTORY"),
                                  base_path_t.c_str());

  /*rc = get_runtime_property_value_fptr(cxt, STR("APP_PATHS"), &prop_buffer);
  if (rc != 0)
    wprintf(STR("APP_PATHS: %s\n"), prop_buffer);*/
  set_runtime_property_value_fptr(cxt, STR("APP_PATHS"), base_path_t.c_str());

  /*rc = get_runtime_property_value_fptr(cxt, STR("APP_NI_PATHS"),
  &prop_buffer); if (rc != 0) wprintf(STR("APP_NI_PATHS: %s\n"), prop_buffer);*/
  set_runtime_property_value_fptr(cxt, STR("APP_NI_PATHS"),
                                  base_path_t.c_str());

  // TODO DM 27.05.2020: This is not set....
  /*rc = get_runtime_property_value_fptr(cxt, STR("APP_CONTEXT_DEPS_FILES"),
  &prop_buffer); if (rc != 0) wprintf(STR("APP_CONTEXT_DEPS_FILES: %s\n"),
  prop_buffer);*/
#ifdef WINDOWS
  auto app_context_deps_file =
      runtime_config.substr(0, dll_index - 3) + u8".deps.json";
#else
  auto app_context_deps_file =
      runtime_config.substr(0, dll_index - 3) + ".deps.json";
#endif
  auto app_context_deps_file_t = StringTFromUtf8(app_context_deps_file);
  set_runtime_property_value_fptr(cxt, STR("APP_CONTEXT_DEPS_FILES"),
                                  app_context_deps_file_t.c_str());

  /*rc = get_runtime_property_value_fptr(cxt, STR("PLATFORM_RESOURCE_ROOTS"),
  &prop_buffer); if (rc != 0) wprintf(STR("PLATFORM_RESOURCE_ROOTS: %s\n"),
  prop_buffer);*/
  // set_runtime_property_value_fptr(cxt,STR("PLATFORM_RESOURCE_ROOTS"),base_path_t.c_str());

  /*rc = get_runtime_property_value_fptr(cxt,
  STR("NATIVE_DLL_SEARCH_DIRECTORIES"), &prop_buffer); if (rc != 0)
    wprintf(STR("NATIVE_DLL_SEARCH_DIRECTORIES: %s\n"), prop_buffer);*/
  // set_runtime_property_value_fptr(cxt,STR("NATIVE_DLL_SEARCH_DIRECTORIES"),base_path_t.c_str());

  // Get runtime delegate for resolving delegates
  load_assembly_and_get_function_pointer_fn
      load_assembly_and_get_function_fptr = nullptr;
  rc = get_runtime_delegate_fptr(
      cxt, hdt_load_assembly_and_get_function_pointer,
      reinterpret_cast<void **>(&load_assembly_and_get_function_fptr));
  if (rc != 0 || load_assembly_and_get_function_fptr == nullptr) {
    std::cerr << "Get delegate failed: " << std::hex << std::showbase << rc
              << std::endl;
    close_fptr(cxt);
    free_library(lib);
    return DotNetHostCreationResult::kInitializeFailed;
  }

  impl->load_assembly_and_get_function_ = load_assembly_and_get_function_fptr;
  impl->context_ = cxt;
  impl->close_fptr_ = close_fptr;
  impl->hostfxr_lib_ = lib;
  impl->SetInitialized();

  // As there is no public ctor, we need this instead of std::make_unique:
  host = std::unique_ptr<DotNetHost>(new DotNetHost(impl));
  return DotNetHostCreationResult::kOK;
}

void *DotNetHost::GetManagedFunction(std::string type_name,
                                     std::string method_name,
                                     std::string signature_delegate_name) {
  return impl_->GetManagedFunction(type_name, method_name,
                                   signature_delegate_name);
}

}  // namespace coreclrhosting