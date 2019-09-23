namespace BlazorApp
{
    using BlazorApp.Builder;
    using Microsoft.Extensions.DependencyInjection;
    
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        { }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}