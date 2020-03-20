namespace TestApp
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System;
    using FluentAssertions;
    using NodeHostEnvironment.NativeHost;

    public class Program
    {
        public static int Main(string[] args)
        {
            var host = NativeHost.Initialize();
            //var tcs = new TaskCompletionSource<int>();            
            var global = host.Global;

            //Console.WriteLine("Waiting for debugger!");
            //while(!System.Diagnostics.Debugger.IsAttached) System.Threading.Thread.Sleep(50);

            global.registerAsyncTest(new Func<Task>(() => Task.Delay(5)));

            // Important: As mocha runs the tests asynchronously, we have to dispose after all tests have been run.
            global.after(new Action(() =>
            {
                //tcs.SetResult(0);
                host.Dispose();
            }));

            global.describe("Arguments ", new Action(() =>
            {
                global.it("should be passed to entry point", new Action(() =>
                {
                    args.Length.Should().Be(1);
                    args[0].Should().Be("AdditionalArgument");
                }));

            }));

            global.describe("Global test object ", new Action(() =>
            {
                global.before(new Action(() =>
                {
                    global.setupTestObject();

                }));

                global.it("should have integerValue 42", new Action(() =>
                {
                    ((int) global.testObject.integerValue).Should().Be(42);

                    // TODO DM 05.03.2020: This does not work as DLR binds an `object`, which will be a double for number types :(
                    /*int i = global.testObject.integerValue;
                    i.Should().Be(42);*/
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

                global.it("should have string array values", new Action(() =>
                {
                    ((string[]) global.testObject.stringArray).Should().Equal(new string[] { "A", "B" });
                    ((IReadOnlyCollection<string>) global.testObject.stringArray).Should().Equal(new string[] { "A", "B" });
                    ((IEnumerable<string>) global.testObject.stringArray).Should().Equal(new string[] { "A", "B" });

                    ((object[]) global.testObject.stringArray).Should().Equal(new string[] { "A", "B" });
                    ((IReadOnlyCollection<object>) global.testObject.stringArray).Should().Equal(new string[] { "A", "B" });
                    ((IEnumerable<object>) global.testObject.stringArray).Should().Equal(new string[] { "A", "B" });

                }));

                global.it("should have mixed array values", new Action(() =>
                {
                    var mixedObjArray = (object[]) global.testObject.mixedArray;
                    mixedObjArray.Length.Should().Be(3);
                    ((string) mixedObjArray[0]).Should().Be("A");
                    ((double) mixedObjArray[1]).Should().Be(42); // TODO DM 05.03.2020: Casting to (int) should be supported
                    ((int) ((dynamic) mixedObjArray[2]).iValue).Should().Be(42);
                    ((string) ((dynamic) mixedObjArray[2]).strValue).Should().Be("strValue");

                    var mixedDynArray = (IReadOnlyCollection<dynamic>) global.testObject.mixedArray;
                    mixedDynArray.Count.Should().Be(3);
                    /*((string)mixedDynArray[0]).Should().Be("A");
                    ((int)mixedDynArray[1]).Should().Be(42);
                    ((int)((dynamic)mixedDynArray[2]).iValue).Should().Be(42);
                    ((string)((dynamic)mixedDynArray[2]).strValue).Should().Be("strValue");*/

                }));

                global.it("should support indexer", new Action(() =>
                {
                    dynamic array = global.testObject["stringArray"];
                    string entry1 = array[0];
                    string entry2 = array[1];

                    entry1.Should().Be("A");
                    entry2.Should().Be("B");
                }));

                global.it("should accept dynamic numbers back", new Action(() =>
                {
                    var result = (int) global.testObject.addIntegerValue(global.testObject.integerValue);
                    result.Should().Be(84);
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

                global.it("should invoke callback with int argument", new Action(() =>
                {
                    global.testObject.invokeIntCallback(42, new Action<long>(arg => arg.Should().Be(84)));
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

                global.it("should return Task from createPromise", new Func<Task>(async() =>
                {
                    var result = await (Task<string>) global.testObject.createPromise(true);
                    result.Should().Be("Resolved");
                }));

                // TODO DM 24.11.2019: This would require another bunch of reflection code to get working...
                /*global.it("should await result from createPromise", new Func<Task>(async () => 
                {
                    string result = await global.testObject.createPromise(true);
                    result.Should().Be("Resolved");
                }));*/

                global.it("should return failed Task from createPromise", new Func<Task>(async() =>
                {
                    // DM 27.11.2019: We need the try/catch as FluentAssertions waits on the threadpool :(
                    var didThrow = false;
                    try
                    {
                        await (Task) global.testObject.createPromise(false);
                    }
                    catch (Exception e)
                    {
                        didThrow = true;
                        e.Message.Should().Contain("Error: As requested");
                    }
                    didThrow.Should().Be(true);

                }));

                global.it("should marshal Task with unsupported result type as exception", new Func<Task>(async() =>
                {
                    var task = Task.FromResult(new UnsupportedType());
                    ((bool) global.testObject.isPromise(task)).Should().Be(true);

                    // DM 27.11.2019: We need the try/catch as FluentAssertions waits on the threadpool :(
                    var didThrow = false;
                    try
                    {
                        await (Task) global.testObject.awaitPromise(task);
                    }
                    catch (Exception e)
                    {
                        didThrow = true;
                        e.Message.Should().Contain("Error: InvalidOperationException: Unsupported object type for passing into JS: TestApp.Program+UnsupportedType");
                    }
                    didThrow.Should().Be(true);

                }));

                global.it("should instantiate TestClass", new Action(() =>
                {
                    var inst = global.testObject.TestClass.CreateNewInstance(42);
                    ((int) inst.value).Should().Be(42);
                }));

                global.it("should call TestClass.staticFunc", new Action(() =>
                {
                    var result = global.testObject.TestClass.staticFunc(42);
                    ((int) result).Should().Be(42 + 42);
                }));

                global.it("should support require", new Action(() =>
                {
                    string sep = global.require("path").sep;
                    sep.Should().Be(System.IO.Path.DirectorySeparatorChar.ToString());
                }));

                global.it("should implement equals for objects & functions", new Action(() =>
                {
                    var a = host.New();
                    global.a = a;

                    AssertionExtensions.Should(Equals(a, a)).BeTrue("Ref equal");
                    AssertionExtensions.Should(Equals(a, global.a)).Be(true);
                    AssertionExtensions.Should(Equals(a, global.testObject)).Be(false);
                    AssertionExtensions.Should(Equals(global.testObject, global.testObject)).Be(true);
                    AssertionExtensions.Should(Equals(global.testObject, global.it)).Be(false);
                    AssertionExtensions.Should(Equals(global.it, global.it)).Be(true);

                    AssertionExtensions.Should(a == a).BeTrue("Ref equal");
                    AssertionExtensions.Should(a == global.a).BeTrue("a == global.a");
                    AssertionExtensions.Should(a == global.testObject).Be(false);
                    AssertionExtensions.Should(global.testObject == global.testObject).Be(true);
                    AssertionExtensions.Should(global.testObject == global.it).Be(false);
                    AssertionExtensions.Should(global.it == global.it).Be(true);

                }));

                // TODO: Test hash code

                global.it("should marshal byte arrays to JS", new Action(() =>
                {
                    var a = new byte[] { 1, 2, 3, 4 };
                    global.testObject.assertByteArray(a);
                }));

                global.it("should marshal mixed collections to js", new Action(() =>
                {
                    global.testObject.assertMixedArray(new object[] { "a", 1, "b", 2 });
                    global.testObject.assertMixedArray(new List<object> { "a", 1, "b", 2 });
                }));

                global.it("should marshal string collections to js", new Action(() =>
                {
                    global.testObject.assertStringArray(new string[] { "a", "b" });
                    global.testObject.assertStringArray(new List<string> { "a", "b" });
                }));

                // TODO: support those collections
                /*global.it("should marshal primitive collections to js", new Action(() =>
                {                        
                    global.testObject.assertIntArray(new int[] { 1, 2 });
                    global.testObject.assertIntArray(new List<int> { 1, 2 });                        
                }));*/

            }));

            //return tcs.Task;
            return 0;

        }

        private sealed class UnsupportedType { }
    }
}