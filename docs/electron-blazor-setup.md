# How to set up an electron-blazor project

## Setup node project and dependencies

Setup new node project using:

```shell
npm init
```

Install dependencies:

```shell
npm i --save-dev electron
npm i --save-dev electron-rebuild
npm i electron-blazor-glue
```

Run `electron-rebuild`. This can be done by adding and running the following script:

```json
"postinstall": "electron-rebuild"
```

## Setup dotnet renderer app

Setup new node project using:

```shell
mkdir RenderApp
cd RenderApp
dotnet new blazorserver
dotnet add package ElectronHostedBlazor -v 0.1.0-alpha.10
```

Replace the `CreateHostBuilder` method in `Program.cs` with:

```cs
public static INodeHostBuilder CreateHostBuilder(string[] args) =>
            BlazorNodeHost.CreateDefaultBuilder()
                .UseBlazorStartup<Startup>();
```

Also replace `Startup` with the following implementation:

```cs
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<WeatherForecastService>();
    }

    public void Configure(IComponentsApplicationBuilder app)
    {
        app.AddComponent<App>("app");
    }
}
```

And of course add proper using statements ;)

Delete the `RenderApp/_Host.cshtml`, as it is no longer needed.
Add the `RenderApp/wwwroot/index.html` file with content like this:

```html
<!DOCTYPE html>
<html>
  <head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" /> 
    <base href="./" />
    <!-- <base href="~/" /> -->
    <link rel="stylesheet" href="css/bootstrap/bootstrap.min.css" />
    <link href="css/site.css" rel="stylesheet" />
    <title>Blazor in renderer process!</title>
  </head>
  <body>
    <app>
    <h1>Hello World!</h1>
    We are using node <script>document.write(process.versions.node)</script>,
    Chrome <script>document.write(process.versions.chrome)</script>,
    and Electron <script>document.write(process.versions.electron)</script>.
    <br/>
    We are in process <script>document.write(process.pid)</script>.
    <br/>
    Loading coreclr and blazor app...
    </app>
    <script>
      const glue = require('electron-blazor-glue');
      window.runBlazorApp(__dirname + "/..", "RenderApp.dll");
    </script>
  </body>
</html>
```

## Setup JS electron application

Add the following scripts to `package.json`:

```json
"build-renderapp": "node node_modules/coreclr-hosting/build-scripts/dotnet-publish.js RenderApp/RenderApp.csproj bin",
"start": "npm run build-renderapp && electron .",
"electron": "electron ."
```

Create (and reference if not done by `npm init`) from `package.json` a `index.js` like this:

```javascript
const { app, BrowserWindow } = require('electron');

let mainWindow;

app.on("ready", function(launchInfo) {
    mainWindow = new BrowserWindow({
        title: "Hello there",
        webPreferences: {
            nodeIntegration: true
        }
    });
    mainWindow.loadURL(`file://${__dirname}/bin/wwwroot/index.html`);
});
```

This could also be implemented using a dotnet app and `coreclr-hosting`.

## Run it

```shell
npm run start
```
