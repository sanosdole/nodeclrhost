# Publish release

1. Update the latest version in the readme and push it.
2. Go to <https://github.com/sanosdole/nodeclrhost/releases> and select `Draft a new release`.
3. Use the version prefixed with `v` as tag name _and_ release name. E.g. `v0.1.0-alpha.11`.
4. Done. GitHub CI will publish the release and upload artifacts to GitHub :)

## Versioning

- The tag version (e.g. `v10.0.0`) becomes the npm version (`coreclr-hosting`,
  `electron-blazor-glue`), the NuGet version, and the **prebuild asset** version
  (`coreclr-hosting-{version}-{runtime}-v{abi}-{platform}-{arch}.tar.gz`).
- `NodeHostEnvironment` / `ElectronHostedBlazor` derive their NuGet version from
  the tag via `GITHUB_REF.Substring(11)` and fall back to `10.0.0-alpha` on
  non-tag builds.
- When publishing a release, ensure the README __Latest release__ line and the
  `publishConfig`/version file in each package reflect the intended tag.

## CI prerequisites (must be green before merging to main / releasing)

- The workflows (`build.yml`, `release.yml`) must use **current GitHub Actions**:
  - `actions/checkout@v4`, `actions/setup-node@v4`, `actions/setup-dotnet@v4`
    (Node 20) — the old `@v3` (Node 16) actions are deprecated and fail on
    today's runners.
  - `runs-on: windows-2022` (the `windows-2019` image was retired).
  - `xresloader/upload-to-github-release@v1.6.0` (Node 20) — the old `@v1`
    runs on Node 16 and is deprecated.
- The **global `prebuild`** resolved by `npm i -g prebuild` bundles `node-abi`
  that must know the newest Electron/Node target. `prebuild@13` resolves
  `node-abi@^3.54` → `3.95`, which supports Electron 44 (ABI 149).
- Both NuGet packages must `dotnet pack` cleanly for the new target
  (`net8.0;net10.0`).

