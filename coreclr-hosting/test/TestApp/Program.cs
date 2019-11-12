namespace TestApp
{
    using System;
    using FluentAssertions;
    using NodeHostEnvironment;

    public class Program
    {
        public static void Main(string[] args)
        {

            try
            {
                var host = NodeHost.InProcess("./build/Release/coreclr-hosting.node");
                var global = host.Global;

                // Important: As mocha runs the tests asynchronously, we have to dispose after all tests have been run.
                global.after(new Action(() => host.Dispose()));

                global.describe("Global test object ", new Action(() =>
                {
                    global.before(new Action(() =>
                    {
                        global.setupTestObject();

                    }));

                    global.it("should have integerValue 42", new Action(() =>
                    {
                        ((int) global.testObject.integerValue).Should().Be(42);
                    }));

                    global.it("should have doubleValue 3.1415", new Action(() =>
                    {
                        ((double) global.testObject.doubleValue).Should().Be(3.1415);
                    }));

                    global.it(@"should have stringValue ""Hello world""", new Action(() =>
                    {
                        ((string) global.testObject.stringValue).Should().Be("Hello world");
                    }));

                    global.it(@"should have objectValue.nestedStringValue ""Nested hello""", new Action(() =>
                    {
                        ((string) global.testObject.objectValue.nestedStringValue).Should().Be("Nested hello");
                    }));

                    global.it("should have nullValue null", new Action(() =>
                    {
                        ((object) global.testObject.nullValue).Should().Be(null);
                    }));

                    global.it("should have boolean values", new Action(() =>
                    {
                        ((double) global.testObject.trueValue > 0.0).Should().Be(true);
                        ((double) global.testObject.falseValue > 0.0).Should().Be(false);
                        //((bool) global.testObject.trueValue).Should().Be(true);
                        //((bool) global.testObject.falseValue).Should().Be(false);
                    }));

                    global.it("should throw on funcThatThrows", new Action(() =>
                    {
                        var accessError = new Action(() => global.testObject.funcThatThrows());
                        accessError.Should().Throw<InvalidOperationException>().WithMessage("Test error message");
                    }));

                    global.it("should invoke passed callback", new Action(() =>
                    {
                        global.testObject.invokeCallback("Ping", new Action<string>(arg => arg.Should().Be("PingPong")));
                    }));

                    global.it("should pass exceptions from passed callback", new Action(() =>
                    {
                        var shouldPassException = new Action(() => 
                        global.testObject.invokeCallback("Ping",
                            new Action<string>(_ =>
                            {
                                throw new InvalidOperationException("Test error message");
                            })));
                        shouldPassException.Should().Throw<InvalidOperationException>();
                    }));

                }));

            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
            }
        }
    }
}