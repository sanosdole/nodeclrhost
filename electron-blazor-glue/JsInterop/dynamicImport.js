// Nodeclrhost-specific helper: a native dynamic `import()` that survives the webpack bundle.
//
// MUST be plain JavaScript (not TypeScript) and must NOT be routed through ts-loader.
//
// Why this exists:
//   The glue is bundled with webpack, and `Microsoft.JSInterop.ts` needs a real dynamic import
//   to serve the .NET-side `JSRuntime.InvokeAsync<IJSObjectReference>("import", "...")` call.
//   However, the glue's tsconfig uses `"module": "commonjs"`, so TypeScript compiles
//   `import(specifier)` down to `require(specifier)` and drops any magic comments such as
//   `/* webpackIgnore: true */`. Webpack therefore never sees an `import()` expression and instead
//   replaces the dynamic `require(expr)` with an empty context module (webpackEmptyContext) whose
//   body is `throw new Error("Cannot find module '" + req + "'")` - i.e. every IJSRuntime module
//   import failed with "Cannot find module" at runtime.
//
// By keeping the dynamic import in this plain .js file, webpack parses the `import()` expression
// itself and honours the `/* webpackIgnore: true */` magic comment, leaving a native runtime
// `import()` in the bundle. At runtime the Electron renderer executes the bundle as CommonJS via
// Node's loader, so this resolves through Node's ESM loader (auto-detecting ESM syntax in .js files).
//
// Right future solution:
//   Compile the glue to an ESM-flavoured module (e.g. "module": "es2020" / "esnext") instead of
//   "commonjs". TypeScript then emits a native `import()` unchanged, the `/* webpackIgnore: true */`
//   comment works directly from the TypeScript source (matching upstream ASP.NET Core, where the
//   same construct works), and this helper becomes unnecessary and can be deleted.

export const nativeImport = (specifier) => import(/* webpackIgnore: true */ specifier);