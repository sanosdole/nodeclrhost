namespace TestApp.Tests
{
    using FluentAssertions;

    public sealed class Arguments : MochaTest
    {
        public Arguments(string[] arguments)
        {
            _arguments = arguments;
        }

        private readonly string[] _arguments;

        public void It_should_be_passed_to_entry_point()
        {
            _arguments.Length.Should().Be(1);
            _arguments[0].Should().Be("AdditionalArgument");
        }
    }
}