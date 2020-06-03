# Notes

## Setup native development environment

```shell
npm install node-gyp --save-dev
npm install node-addon-api
```

Use `npm run build` to compile native.
Exclude `"**/build/*.sln": true` in `settings.json` to prevent omnisharp from locking the build folder.

## Packing the hosted .net app

Install `Microsoft.Packaging.Tools.Trimming` to reduce size of distribution.
Use `dotnet publish --self-contained -r win10-x64 SampleApp.csproj -o bin\published /p:TrimUnusedDependencies=true` to publish a self contained app.

## Hosting .net

[Sample code](https://github.com/dotnet/samples/tree/master/core/hosting/HostWithCoreClrHost)

We can PInvoke into the hosting library, as `LoadLibrary`/`dlopen` uses reference counting.

## Asynchronous execution

> TODO: Check if `napi_create_threadsafe_function` could/should be used instead.

Node is based upon `libuv` default loop.

To schedule work on Node event loop see functions extracted from [example](https://github.com/mika-fischer/napi-thread-safe-callback):

```cpp
#include <uv.h>

// Setup an async operation, put context into handle->data
// NOTE: This will keep node alive until unref or close of the handle!
uv_async_init(uv_default_loop(), &handle_, &static_async_callback);

// Provoke execution
// libuv will coalesce calls to uv_async_send(), that is, not every call to it will yield an execution of the callback.
uv_async_send(&handle_); // This is the only thread-safe function!!!

static void static_async_callback(uv_async_t *handle)
        {
            try
            {
                static_cast<Impl *>(handle->data)->async_callback();
            }
            catch (std::exception& e)
            {
                Napi::Error::Fatal("", e.what());
            }
            catch (...) 
            {
                Napi::Error::Fatal("", "ERROR: Unknown exception during async callback");
            }
        }

// Will close the handle, the handles memory must only be deallocated after the callback is called.
uv_close(reinterpret_cast<uv_handle_t *>(&handle_), [](uv_handle_t *handle) {
                    delete static_cast<Impl *>(handle->data);
                });

```

## Marshalling

We need an wrapper object that stores value references (`Napi::Reference`) that we return to managed code.
The must be released from managed code.

For function invocation we need to create argument lists.
Conversion of primitives to `IntPtr` see [](https://gist.github.com/jordanzhang/2288117).

In order to map general purpose passing of callbacks to JS we need to fulfill the JS callback signature.

> TODO: How do we map callbacks to Task and vice versa?

The following access methods are needed:
```cs

// Get a handle
JsValue GetMember(IntPtr ownerHandle, string name); // A zero handle uses the global object.

// Convert handles to primitives can be done in managed code based on JsType
// ATTENTION: 32bit node exists :(

// Set a member
void SetMember(IntPtr ownerHandle, string name, DotNetType type, IntPtr value);

// Invoke handles that represent functions
JsValue Invoke(IntPtr handle, IntPtr receiverHandle, int argc, DotNetType[] argt, IntPtr[] argv);

// Create a JSON object
JsValue CreateJsonObject(int argc, string[] argn, DotNetType[] argt, IntPtr[] argv);
JsValue CreateJsonObject(string json);

// Release a handle
void Release(JsValue handle);

// Callback signature
delegate IntPtr JsCall(int argc, JsValue[] argv, out DotNetType resultType)

```


## Implementing an ASP.NET server

The following services are required (copied from <https://github.com/dotnet/aspnetcore/blob/master/src/Servers/HttpSys/src/WebHostBuilderHttpSysExtensions.cs>):

```cs
services.AddSingleton<IServer>;
services.AddSingleton<IServerIntegratedAuth>
```

Also `services.AddAuthenticationCore();` should be provided.

### IServer interface

See <https://github.com/dotnet/aspnetcore/blob/master/src/Hosting/Server.Abstractions/src/IServer.cs>

Started with an `IHttpApplication<TContext>` and provides an `IFeatureCollection`.

For each incoming request create a context from the `IHttpApplication.CreateContext(IFeatureCollection contextFeatures)` and call `IHttpApplication.ProcessRequestAsync(TContext context)`. After that dispose the context using `IHttpApplication.DisposeContext(TContext context, Exception exception)`.
The type parameter can be worked around using a `IHttpApplication<object>` wrapper that casts properly on Process/Dispose.

An example of how this is used can be seen in <https://github.com/dotnet/aspnetcore/blob/master/src/Servers/HttpSys/src/FeatureContext.cs>.
It shows how to implement the `IFeatureCollection` for creating the context.









