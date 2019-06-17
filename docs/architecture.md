# Architecture

Native node addon `coreclr-hosting`.
.NET library `NodeHostEnvironment`.

Node calls `coreclrHosting.runCoreApp` to execute a core application.
> Do not block in Main, as this would block node. The clr will run and keep node alive until `NodeHostEnvironment.ReleaseHost` is called.

## NodeHostEnvironment

Calls into `coreclr-hosting.node` using PInvoke.

### Responsibilities

`DynamicObject` for js objects.
`NodeSynchronizationContext` & `NodeTaskScheduler` for async support.
Marshalling .Net types back to js.