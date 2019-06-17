dotnet publish --self-contained -r win10-x64 BrowserApp\BrowserApp.csproj -o bin\published /p:TrimUnusedDependencies=true

dotnet publish --self-contained -r win10-x64 RendererApp\RendererApp.csproj -o bin\published /p:TrimUnusedDependencies=true
