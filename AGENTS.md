# AGENTS.md

Guidance for AI coding agents working in this repository.

## What this project is

**nodeclrhost** enables writing node/electron applications with .NET. The core idea:

- A native node addon (`coreclr-hosting`) boots a .NET Core runtime inside a node process.
- The .NET side talks back to the Node runtime via the `NodeHostEnvironment` library.
- Because the CLR runs natively (not WASM), .NET code gets full framework + debugger access.
- `ElectronHostedBlazor` + `electron-blazor-glue` extend this to run Blazor apps inside an Electron renderer process without WebAssembly.

Do not block in `Main` — the CLR runs and keeps Node alive until `NodeHostEnvironment.ReleaseHost` is called.

## Repository layout & the three components

This is a cross-language monorepo producing 4 published artifacts (2 npm packages, 2 NuGet packages).

### 1. `coreclr-hosting/` — native Node addon (C++)
- Native module that loads the .NET host (`hostfxr`/`nethost`) and runs a .NET entry point.
- Node-gyp/napi build (`NAPI_DISABLE_CPP_EXCEPTIONS`). Sources in `cppsrc/`:
  `main.cc` (module entry), `context.cc`, `dotnethost.cc`, `nativeapi.cc` + headers.
- `binding.gyp` per-OS flags (WINDOWS / LINUX / NON_WINDOWS_DEFINE), links `nethost`.
- `hostfxr/DownloadHostFxr.csproj` downloads the hostfxr bits into `hostfxr/bin`.
- `prebuilds/` stores prebuilt binaries per Node/Electron ABI version (built by CI via `prebuild`).
- npm scripts: `build`, `test` (build addon + TestApp + `mocha`), `benchmark`.
- Install does `prebuild-install || npm run rebuild`.

### 2. `NodeHostEnvironment/` — .NET library (C# → Node bridge)
- Multi-targets: `netcoreapp3.1;net5.0;net6.0;net8.0`. Package id `NodeHostEnvironment`.
- P/Invokes into `coreclr-hosting.node`.
- Key pieces:
  - `NodeHost.cs` exposes `NodeHost.Instance` (`IBridgeToNode`).
  - `NativeHost/` — `NativeApi`, `NativeEntryPoint`, `NativeNodeHost`, `NodeTaskScheduler` (async support).
  - `InProcess/` — In-process bridge: `NodeBridge`, `JsDynamicObject`, `JsValue`/`JsType` (DynamicObject for JS objects),
    `DotNetValue`/`DotNetType` marshalling, `InvokeHelper`, `IHostInProcess`.

### 3. `ElectronHostedBlazor/` (.NET, `net8.0`) + `electron-blazor-glue/` (TypeScript/Webpack)
- `ElectronHostedBlazor` = Blazor renderer host for Electron using `NodeHostEnvironment` (single .NET project,
  `net8.0`, `Nullable` enabled, `AllowUnsafeBlocks`, references ASP.NET Core). Folders: `Hosting`, `Logging`, `Rendering`, `Services`, `Shared`.
- `electron-blazor-glue` = bundled JS/TS glue (`webpack`, `ts-loader`) that bridges the Blazor renderer in
  Electron. Main output `dist/blazor.electron.js`. Depends on `coreclr-hosting`.

## Build & test

- `nodeclrhost.sln` — Visual Studio solution: `NodeHostEnvironment`, `ElectronHostedBlazor`, `TestApp`(test), plus examples
  (`electron-blazor/BlazorApp`, `LocalService`, `electron-mvc/MvcApp`).
- Native addon:
  ```bash
  cd coreclr-hosting
  npm ci --build-from-source   # or: npm install (uses prebuilt if available)
  npm run build-testapp        # dotnet build test/TestApp/TestApp.csproj
  npm run mocha                # run coreclr-hosting/test/test.js
  npm test                     # everything above
  ```
- NuGet (version derived from `$GITHUB_REF`; `8.0.0-alpha` on non-tag builds):
  ```bash
  cd NodeHostEnvironment && dotnet pack -c Release
  cd ../ElectronHostedBlazor && dotnet pack -c Release
  ```
- JS/TS glue:
  ```bash
  cd electron-blazor-glue
  npm ci
  npm run build-js-glue:release
  ```

## Testing

- Node-side tests live in `coreclr-hosting/test/` (`test.js` + `TestApp/`), run via `mocha`.
- Example apps under `examples/` (`sample`, `electron-sample`, `electron-blazor`, `electron-mvc`) are runnable
  end-to-end demos, not unit tests.

## Code style & conventions

- **Formatting**: driven by `.editorconfig` and `omnisharp.json`.
  - 4-space indent, `utf-8-bom` for code files, final-newline on.
  - Allman braces (brace on new line for types/methods/properties/control blocks; `NewLineForElse`/`Catch`/etc.).
  - C# style: `dotnet_sort_system_directives_first`, prefer predefined types (`int` over `Int32`),
    `this.`-flagging off, `dotnet_style_readonly_field = true:suggestion`.
- Keep formatting/naming consistent with the existing code (do not reformat unrelated files).
- Do not commit `node_modules/`, `bin/`, `obj/`, `prebuilds/` binaries, `_ReSharper.Caches`, `.vs/` (see `.gitignore`).

## CI / release workflow

- **CI** (`.github/workflows/build.yml`): on push/PR to `master`. Builds & tests the addon on Windows/Linux/macOS
  (uses `actions/setup-dotnet` with 3.1/5.0/6.0/8.0 and Node 16/18), produces `prebuilds`, builds glue, packs NuGet.
- **Release** (`.github/workflows/release.yml`): triggered by a published GitHub release. Bumps versions from the
  tag, builds + uploads prebuilds to the release, publishes to npm and NuGet. Secrets: `NPM_TOKEN`, `NUGET_TOKEN`, `GITHUB_TOKEN`.
- Version scheme: `v`-prefixed tag, e.g. `v8.0.1`. `docs/publish-release.md` explains the manual release steps.

## Docs

`docs/` contains design/handbook material worth reading before deep changes:
- `architecture.md` — high-level host/interop design.
- `electron-blazor-setup.md` — how to set up a Blazor-in-Electron app.
- `design-research-electron-blazor.md`, `net6.md`, `notes.md`, `todos.md` — history/research.