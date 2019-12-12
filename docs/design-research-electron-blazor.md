# Hosting blazor

Blazor requires the following services (from <https://github.com/aspnet/AspNetCore/blob/master/src/Components/Server/src/DependencyInjection/ComponentServiceCollectionExtensions.cs>):


```cs
// Standard blazor hosting services implementations
            //
            // These intentionally replace the non-interactive versions included in MVC.
            services.AddScoped<NavigationManager, RemoteNavigationManager>();
            services.AddScoped<IJSRuntime, RemoteJSRuntime>();
            services.AddScoped<INavigationInterception, RemoteNavigationInterception>();
            services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
```

Also we would require a logger and logger factory, see blazor set up <https://github.com/aspnet/AspNetCore/tree/master/src/Components/Blazor/Blazor/src/Services>

Also we will need a `Renderer` implementation like <https://github.com/aspnet/AspNetCore/blob/master/src/Components/Server/src/Circuits/RemoteRenderer.cs>

All of this requires a JS implementation like <https://github.com/aspnet/AspNetCore/blob/master/src/Components/Web.JS>


## Following WebAssembly infrastructure without WebAssembly

### How WebAssembly works

1. JS invokes Program.Main
2. `BlazorWebAssemblyHost.CreateDefaultBuilder().UseBlazorStartup<Startup>().Build().Run()`
3. `WebAssemblyHost.StartAsync()` 
  - sets `JSRuntime.SetCurrentJSRuntime()` synchronously
  - Creates service scope `IServiceScopeFactory.CreateScope()`
  - creates a `WebAssemblyBlazorApplicationBuilder`
  - Runs registered `IBlazorStartup.Configure(builder)`
    - This adds the root `App.razor` of the app with a DOM selector `app`
  - Creates the renderer using `_renderer = await builder.CreateRendererAsync();`
    - Creates `new WebAssemblyRenderer(Services, loggerFactory)`
    - Adds components to the renderer
      - This will call js `Blazor._internal.attachRootComponentToElement`
      - Then will call `base.RenderRootComponentAsync`
        - TODO: Somehow render batches are created and sent to JS
        - Those render batches register event handler (special attribute set)
        - Those event handlers call back to the `WebAssemblyRenderer` to dispatch them
          - This will invoke the handlers and synchronize the tree (directly and after the task is completed)

