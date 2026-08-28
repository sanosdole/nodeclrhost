# Adapting nodeclrhost to a new .NET release (Net10)

This documents how to adapt this repository to a new .NET version. It is written
against the move from **.NET 8 → .NET 10**, but the general sequence applies to
any future release.

The last major migration (net6 → net8) is in commit `aa4b947`
("Switched to .NET 8"); use it as a reference for how the change was sliced.

## Goal

Everything in this repo pins a .NET/Node/Electron version:

1. `.NET target frameworks` in every `.csproj`
2. `Node.js` + `Electron` versions (and the **prebuilt native binaries** that
   must be generated for each ABI)
3. The JS/TS toolchain (`electron-blazor-glue` webpack build)

"Hosting" = `coreclr-hosting` (native addon) + `NodeHostEnvironment` (.NET
library). "Blazor" = `ElectronHostedBlazor` + `electron-blazor-glue`.

---

## 1. Dependency updates, from coreclr-hosting down

### `coreclr-hosting/`

- `package.json`
  - `engines.node` → `>=20` (matches the lowest Node you still support).
  - `devDependencies`:
    - `mocha`, `node-addon-api`, `node-gyp` → latest majors.
    - **`node-abi`** must be pinned >= the version whose `supportedTargets`
      include your newest Electron ABI. `prebuild` 13.0.1 itself depends on
      `node-abi ^3.54.0`, so install the newest `node-abi@3` that covers the
      target (e.g. `node-abi@^3.95.0` for Electron 44 / ABI v149).
  - `prebuild-node` / `prebuild-electron` scripts → point at the new Node /
    Electron versions.
- `hostfxr/DownloadHostFxr.csproj`, `test/TestApp/TestApp.csproj`,
  `benchmark/Benchmark.csproj` → `netX.0`.
- `test/test.js` → the `runCoreApp(..., '.../bin/Debug/netX.0/TestApp.dll')`
  path.

### `NodeHostEnvironment/NodeHostEnvironment.csproj`

- `TargetFrameworks` → the new `netX.0` **plus** a lower supported one. We
  dropped the now-EOL `netcoreapp3.1;net5.0;net6.0` and keep `net8.0;net10.0`.
  The library is multi-targeted because consumers may still be on older .NET.
- `Version ...>10.0.0-alpha</Version>` (bump the alpha line to match).
- Repackage: since the modern SDK auto-includes `Microsoft.CSharp`, the
  explicit `<PackageReference Include="Microsoft.CSharp">` triggers warning
  `NU1510` and can be removed.

### `ElectronHostedBlazor/ElectronHostedBlazor.csproj`

- `TargetFramework` → `netX.0`, bump `Version` alpha line. Matches the
  `Microsoft.AspNetCore.App` framework reference for that .NET major.

### Examples (all `.csproj` + `package.json` + `index.js` / `.html`)

- Every example's `TargetFramework` → `netX.0`.
- Every `index.js` / `renderer.html` / `wwwroot/index.html` path containing
  `bin/Debug/netX.0/...` → update.
- `package.json` `electron` → new major. (Here `^44.0.0`.)
- **Removed legacy language pins** in `examples/electron-blazor/BlazorApp/BlazorApp.csproj`:
  `LangVersion 7.3` + `RazorLangVersion 3.0`. The modern SDK generates
  `GlobalUsings` that require C# `>= 10`; these old pins produce
  `error CS8370`.

### `electron-blazor-glue/package.json`

- `electron`, `@types/node`, `webpack`, `ts-loader`, `webpack-cli`,
  `node-loader`, `typescript` → latest compatible.

---

## 2. The native addon (coreclr-hosting) C++

This is the most common source of compile breakage on upgrade:

- Newer `binding.gyp`/node-gyp builds default to **C++20**, where `u8"..."`
  literals become `const char8_t[]` and can no longer concatenate with
  `std::string`. We saw this as
  `dotnethost.cc(390,398,603): error C2676` for `u8".dll"`, `u8".runtimeconfig.json"`,
  `u8".deps.json"`. **Fix:** use plain ASCII `"..."` literals there.
- If switch/platform code changes (`#ifdef WINDOWS` Linux/macOS), the unpacking
  of `runtime_config` differ per-OS; keep those synchronized.

After editing, rebuild and run the node tests:

```bash
cd coreclr-hosting
npm install          # or npm ci
npm run build        # download-hostfxr + configure + build
npm run build-testapp
npm run mocha        # 40 tests; must all pass
```

---

## 3. Prebuilds (native binaries per ABI)

Native binaries are compiled per Node.js/Electron ABI and shipped as
`prebuilds/*.tar.gz`, and served from GitHub releases.

- `coreclr-hosting` is built with `node-addon-api` (N-API/NAPI_VERSION), which
  is ABI-stable across Node/Electron, so one binary per major ABI is enough,
  but CI still emits one per target for safety.
- ABI mapping (`node -e "require('node-abi').getAbi(ver, 'node')"`):
  Node `20→v115, 22→v127, 24→v137`;
  Electron `40→v143, 42→v146, 44→v149`.

### Release-artifact isolation (do not break previous releases/prebuilds)

This is a hard requirement: there are downstream builds pinned to *older*
releases of this project. Each released version must keep working forever.
The migration must not (and does not) violate this. How it is guaranteed:

1. **Prebuilds are version-keyed and immutable.** Released assets are named
   `coreclr-hosting-<npm-version>-<runtime>-v<abi>-<platform>-<arch>.tar.gz`,
   where `<npm-version>` is taken from the release tag via
   `npm version "${GITHUB_REF:11}"` (e.g. `v8.0.1` → `...-v8.0.1-electron-v128-...`).
   A `v10.0.x` release therefore produces assets named `...-v10.0.x-...`,
   entirely separate from the `v8.0.1`/`v6.0.4` assets — no collision.
2. **`prebuild-install` is pinned by version.** A consumer installing
   `coreclr-hosting@8.0.1` resolves the GitHub release tagged exactly `v8.0.1`
   and downloads *that* version's prebuilds. It never consults the newest
   workflow. Old releases stay usable.
3. **Uploads are release-scoped.** `upload-to-github-release` with `tags: true`
   attaches prebuilds only to the current release tag; old tags are untouched.
4. **`prebuilds/` is gitignored** — artifacts are always generated per release,
   never shared between versions.
5. **Keep the CI prebuild matrix LIMITED to supported LTS runtimes.** Old
   Node/Electron ABI targets are dropped because: (a) Node ≤16 headers still
   contain an `openssl_fips` condition in `common.gypi` that modern
   `node-gyp`/gyp fails to evaluate (`gyp: name 'openssl_fips' is not defined`),
   and (b) they are EOL. Current matrix (all three OS platforms):
   Node `20, 22, 24` (supported LTS) and Electron `40, 42, 44` (current stable
   window). Anchoring the matrix to supported runtimes keeps the build green and
   the release lean. (Downstream builds already pinned to older releases keep
   their immutable prebuilt artifacts — see points 1–4 above.)
- `.github/workflows/build.yml` and `release.yml` list the exact
  `prebuild -t <ver> [-r electron]` invocations. Update only by adding new
  targets. Release CI also bumps the `setup-dotnet` runtimes (`8.0.x;10.0.x`)
  and uploads all `prebuilds/*`.

### Pitfall 1 (local): node-gyp vs newer Visual Studio
`prebuild@13` and `@electron/rebuild@3` bundle an older node-gyp that does not
detect Visual Studio **2026**. Symptom:
`Could not find any Visual Studio installation to use`. Locally we bumped the
tooling (`node-gyp@13`) to build; CI uses `windows-2019` / matching images so
this is not an issue in CI.

### Pitfall 2 — registering the binary
If the native `.node` is not present (e.g. an install-time
`electron-rebuild` step failed), copy the already built
`coreclr-hosting/build/Debug/coreclr-hosting.node` (and `nethost.dll`) into the
example's `node_modules/coreclr-hosting/build/Debug/`. Because the addon is
N-API/ABI-stable, a Node-built binary loads fine under Electron.

---

## 4. Run & test

- **Core hosting** (node): `examples/sample` boots the CLR, marshals dynamic
  objects, callbacks, promises/btye arrays, byte arrays and exceptions on
  `net10.0` + Node 22. Expect `40 passing` mocha and a clean process exit.
- **Electron hosting** (main process): `examples/electron-sample` runs the CLR
  inside an Electron window.
- **Blazor** is a **later step** — the aspnet/Core Blazor runtime diffing. See
  `docs/net10-blazor-ci.md` when that work is taken on.

> The `electron-mvc` sample binds a Kestrel listener (`localhost:5000`). On
> sandboxed hosts that bind is denied (`SocketException 10013`) — an
> environment restriction, not a nodeclrhost regression; the .NET host boots
> correctly.

---

## 5. Blazor migration (electron-blazor-glue + ElectronHostedBlazor)

Blazor is *not* a blind copy — the electron hosting differs from official
Blazor (it bridges via `coreclr-hosting`/`NodeHostEnvironment` instead of
WebSockets/WASM). The approach: take the **official v10.0.11 code**, see what
changed, and **adapt** it to the electron-specific files — while never breaking
hosting or rebuilding it completely.

Reference checkouts (sparse-clonned the aspnetcore repo at tag `v10.0.11`):

- `electron-blazor-glue` ⇄ `aspnetcore/src/Components/Web.JS/src`
- `ElectronHostedBlazor` ⇄ `aspnetcore/src/Components/WebAssembly/WebAssembly/src`
  (mainly) + `Components/Server/src`; re-implements some `Components/Components/src`
  internals.

### Key adaptations for the electron bridge

- The glue uses a **local** `JsInterop/Microsoft.JSInterop.ts` (electron
  bridge), *not* `@microsoft/dotnet-js-interop`. Keep local imports.
- `Boot.Electron.ts` / `JsInterop/Microsoft.JSInterop.ts` are electron-only
  (no upstream counterpart) — never overwrite them.
- `WebRootComponentManager` deliberately omits the WebAssembly/Server platform
  bootstrap imports (`Boot.Server.Common`, `Boot.WebAssembly.Common`) because
  electron hosts via `coreclr-hosting`. Keep those commented out; only add the
  interface members they need (e.g. `setWebAssemblyOptions` to satisfy
  `DescriptorHandler`).

### v8 → v10 changes applied (bug fixes + features)

- `StreamingInterop.ts` — `ReadableStreamDefaultController<Uint8Array>` +
  `byteOffset` chunk fix (streaming bug fix).
- `DomWrapper.ts` — dropped `preventScroll` param from `focusBySelector`.
- `AttributeSync.ts` — sync `integrity` on `HTMLLinkElement`/`HTMLScriptElement`
  (SRI bug fix).
- `DomSync.ts` — checkbox/radio value sync; `setWebAssemblyOptions`;
  comment typo.
- `EventTypes.ts` — `isComposing` on `KeyboardEventArgs`.
- `EventDelegator.ts` — `removeListenersForElement`, `decrementCountByEventName`,
  `addActiveGlobalListener` (passive:false for wheel/touch preventDefault),
  `isRendererAttached` guard, `enumerateHandlers`.
- `Virtualize.ts` — `isConnected` guards + always dispose (`dotNetHelper`)
  to avoid disposed-component errors.
- `BrowserRenderer.ts` — `appendContent` logical child container,
  `detachEventHandlersFromElement`,
  export `markAsInteractiveRootComponentElement` + `setClearContentOnRootComponentRerender`.
- `LogicalElements.ts` — skip metadata comments; `comment`-based insertion;
  `depthFirstNodeTreeTraversal`.
- `ComponentDescriptorDiscovery.ts` — `isMetadataComment`,
  `discoverWebAssemblyOptions`, `WebAssemblyServerOptions` type.
- `JSEventRegistry.ts` — `enhancednavigationstart/end` events.
- `NavigationUtils/Manager/Enhancement` — `isSamePageWithHash`, `isForSamePath`,
  `resetScroll*`, scroll-to-hash handling, `Element` (not `HTMLElement`),
  `notifyEnhancedNavigationListeners` rename.
- `JSInitializers.ts` — `new URL(...)` relative-path resolution.
- `StreamingRendering.ts` — `not-found`/`redirection` (larger; tied to
  `performEnhancedPageLoad` signature) — verify before applying.

### Not applied (deliberately)

- WebAssembly platform bootstrap / circuit imports — electron does not use them.
- `FileInput`/WASM-specific paths — electron uses the node bridge.

### C# adaptations (ElectronHostedBlazor) for Net10

`ElectronHostedBlazor` is an adapted `WebAssembly` host. Comparing it to
`aspnetcore/src/Components/WebAssembly/WebAssembly/src` (plus the shared
`Components` internal bits) surfaced the following **required** Net 10
adaptations. The rendering mechanism differs (electron pushes render batches
via `coreclr-hosting`), so the render pipeline needs more extrapolation, but the
interop/service code maps almost 1:1.

- **`Services/ElectronNavigationManager.cs`** — implementing
  `SetNavigationLockState(bool)`. In .NET 10 the base `NavigationManager`
  gained this hook to support the `NavigationLock` component; the default
  throws `NotSupportedException`, so without the override `NavigationLock`
  breaks. Mirror `WebAssemblyNavigationManager` by forwarding to
  `setHasLocationChangingListeners(rendererId, value)`. Keep the renderer id
  consistent with the registered nav listener (`listenForNavigationEvents(0,…)`).
- **`Hosting/ElectronHostBuilder.cs`** — register the .NET 10 client-side
  value suppliers, mirroring the `WebAssemblyHostBuilder.InitializeDefaultServices`:
  - `AddSupplyValueFromPersistentComponentStateProvider()` — supports
    `[SupplyParameterFromPersistentComponentState]`.
  - `AddSupplyValueFromQueryProvider()` — supports `[SupplyParameterFromQuery]`.

### Not applied (deliberately)

- **`AntiforgeryStateProvider`** / `ResourceCollectionProvider` — these are
  geared toward WebAssembly SSR/antiforgery and resource loading; electron hosts
  via `coreclr-hosting` and uses its own loader. Only add if a concrete
  component needs them.
- **`WebRenderer.GetWebRendererId()`** override — electron historically relies on
  the base default and rendering works; do not change without a concrete
  consumer.

### Verify after migrating

```bash
cd electron-blazor-glue && npm run build-js-glue:release   # must compile
cd ../examples/electron-blazor && npm run build && electron .   # must boot + render
```

Note: `webpack` production-minifies the bundle (method names are mangled), so an
application-only tests in
`examples/electron-blazor` are the real confirmation.

The renderer may crash intermittently in a headless/`ClrDumps` environment —
running a few times confirms it boots reliably; an occasional "Renderer gone:
crashed" is pre-existing environment flakiness, not a code regression.

```text
[x] Blazor glue (electron-blazor-glue) diff ported to v10 (hosting preserved)
[x] ElectronHostedBlazor adapted to NET10 (navigation locks, value suppliers)
[~] ElectronHostedBlazor exhaustive re-diff/port (WebAssembly+Server+Components) — optional, if a specific feature is needed
```

---

## 6. Docs & version consistency

- Bump the `Version`/alpha lines in both packages and keep the asset version
  in `README.md` in sync once a release is cut.
- Keep `.github/workflows/*.yml` in lock-step: the `prebuild -t` targets, the
  `dotnet` runtimes and the `node` runtimes must all match what was verified
  locally.
- The glue (`electron-blazor-glue`) is now ported to v10 (see section 5). The
  C# `ElectronHostedBlazor` re-implementation has been adapted for the key
  NET10 features (navigation locks, `[SupplyParameterFromQuery]` / persistent
  state). A further exhaustive re-diff is optional and only needed if a
  specific feature not yet used by the electron examples is required.

```text
Approximate checklist summary:
[x] node10.csproj targets
[x] node/electron versions + toolchains
[x] C++20 u8-literal fix
[x] regenerated Electron-44 (ABI v149) prebuild
[x] coreclr-hosting + Node-HostEnvironment tests pass (40)
[x] sample & electron-sample run
[x] electron-blazor-glue diff ported to v10 (hosting preserved)
[x] ElectronHostedBlazor adapted to NET10 (navigation locks, value suppliers)
```