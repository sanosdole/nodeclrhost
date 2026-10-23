// exampleJsImport.js
//
// ES module loaded dynamically from .NET via the "import" JS interop call:
//
//     _module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./exampleJsImport.js");
//     var message = await _module.InvokeAsync<string>("getWelcomeMessage");
//
// In the Electron renderer the module is imported through Node's loader, which
// auto-detects the ESM syntax and exposes the named exports directly on the
// module object returned to .NET.

export function getWelcomeMessage() {
    return "Hello from exampleJsImport.js (imported via IJSRuntime)";
}