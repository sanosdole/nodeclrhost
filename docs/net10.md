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
  Node `20→v115, 22→v127, 24→v137, 26→v147`;
  Electron `24.6.1→v114, 28→v119, 32→v128, 36→v135, 40→v143, 42→v146, 44→v149`.

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
5. **Keep the CI prebuild matrix ADDITIVE.** When bumping versions, keep every
   previously-shipped ABI target and *add* the new ones. The matrix must be a
   strict superset of what older releases shipped. Current matrix (all three
   platforms): Node `14.16,16.13,18.14,19,20,21,22,24,26` and
   Electron `24.6,25,26,27,28,29,30,31,32,36,40,42,44`. Do *not* delete
   historical prebuild lines when adding newer ones — they serve downstream
   builds still running older Node/Electron ABIs.
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

## 5. Docs & version consistency

- Bump the `Version`/alpha lines in both packages and keep the asset version
  in `README.md` in sync once a release is cut.
- Keep `.github/workflows/*.yml` in lock-step: the `prebuild -t` targets, the
  `dotnet` runtimes and the `node` runtimes must all match what was verified
  locally.
- Only after hosting is green, take on the **Blazor rendering**/DOM diffing
  future step (see the `electron-blazor-glue` + `ElectronHostedBlazor`
  section) — those are tracked separately to keep this change focused.

```text
Approximate checklist summary:
[x] node10.csproj targets
[x] node/electron versions + toolchains
[x] C++20 u8-literal fix
[x] regenerated Electron-44 (ABI v149) prebuild
[x] coreclr-hosting + Node-HostEnvironment tests pass (40)
[x] sample & electron-sample run
[ ] Blazor diff (future step)
```