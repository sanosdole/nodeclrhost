﻿namespace TestApp
{
    using System;
    using System.Threading.Tasks;
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

                global.registerAsyncTest(new Func<Task>(() => Task.Delay(5)));

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
                        ((bool) global.testObject.trueValue).Should().Be(true);
                        ((bool) global.testObject.falseValue).Should().Be(false);
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

                    global.it("should invoke passed callback when invoked as dynamic object", new Action(() =>
                    {
                        var invokeCallback = global.testObject.invokeCallback;
                        invokeCallback("Ping", new Action<string>(arg => arg.Should().Be("PingPong")));
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

                    global.it("should get Task passed as Promise", new Action(() =>
                    {
                        ((bool) global.testObject.isPromise(Task.FromResult(5))).Should().Be(true);
                    }));

                    global.it("should return Task from createPromise", new Func<Task>(async () => 
                    {
                        var result = await (Task<string>)global.testObject.createPromise(true);
                        result.Should().Be("Resolved");
                    }));

                    // TODO DM 24.11.2019: This would require another bunch of reflection code to get working...
                    /*global.it("should await result from createPromise", new Func<Task>(async () => 
                    {
                        string result = await global.testObject.createPromise(true);
                        result.Should().Be("Resolved");
                    }));*/

                    global.it("should return failed Task from createPromise", new Func<Task>(async () => 
                    {
                        // DM 27.11.2019: We need the try/catch as FluentAssertions waits on the threadpool :(
                        var didThrow = false;
                        try
                        {
                            await (Task)global.testObject.createPromise(false);
                        }
                        catch(Exception e)
                        {
                            didThrow = true;
                            e.Message.Should().Contain("Error: As requested");
                        }
                        didThrow.Should().Be(true);
                        
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