call clear-local-packages.bat
dotnet pack NodeHostEnvironment/NodeHostEnvironment.csproj --include-symbols
dotnet pack ElectronHostedBlazor/ElectronHostedBlazor.csproj --include-symbols