namespace BlazorApp.Hosting
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    
    public static class BlazorNodeHost
    {
        /// <summary>
        /// Creates an instance of <see cref="IWebAssemblyHostBuilder"/>.
        /// </summary>
        /// <returns>The <see cref="IWebAssemblyHostBuilder"/>.</returns>
        public static INodeHostBuilder CreateDefaultBuilder()
        {
            return new NodeHostBuilder();
        }
    }
}