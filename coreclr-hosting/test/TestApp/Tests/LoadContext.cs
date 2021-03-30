namespace TestApp.Tests
{
    using System.Reflection;
    using System.Runtime.Loader;
    using FluentAssertions;
    using NodeHostEnvironment;

    public sealed class LoadContext : MochaTest
    {
        public void It_should_not_reload_assembly()
        {
            var ownAssembly = GetType().Assembly;
            var loaded = Assembly.Load(ownAssembly.FullName);

            loaded.Should().BeSameAs(ownAssembly);
        }

        public void It_should_be_the_default_context()
        {
            var nheAlc = AssemblyLoadContext.GetLoadContext(typeof(IBridgeToNode).Assembly);
            var myAlc = AssemblyLoadContext.GetLoadContext(typeof(LoadContext).Assembly);
            var defaultAlc = AssemblyLoadContext.Default;

            nheAlc.Should().BeSameAs(defaultAlc);
            myAlc.Should().BeSameAs(defaultAlc);
        }
    }
}
