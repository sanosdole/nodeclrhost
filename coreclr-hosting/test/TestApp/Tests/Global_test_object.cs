using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;

namespace TestApp.Tests
{
    public sealed class Global_test_object : MochaTest
    {
        public void Before()
        {
            Global.setupTestObject();
        }

        public void It_should_have_integerValue_42()
        {
            ((int) Global.testObject.integerValue).Should().Be(42);

            // TODO DM 05.03.2020: This does not work as DLR binds an `object`, which will be a double for number types :(
            /*int i = global.testObject.integerValue;
            i.Should().Be(42);*/
        }

        public void It_should_have_doubleValue_3_1415()
        {
            ((double) Global.testObject.doubleValue).Should().Be(3.1415);
        }

        public void It_should_have_stringValue_Hello_world()
        {
            ((string) Global.testObject.stringValue).Should().Be("Hello world");
        }

        public void It_should_have_objectValue_nestedStringValue_Nested_hello_()
        {
            ((string) Global.testObject.objectValue.nestedStringValue).Should().Be("Nested hello");
        }

        public void It_should_have_nullValue_null()
        {
            ((object) Global.testObject.nullValue).Should().Be(null);
        }

        public void It_should_have_boolean_values()
        {
            ((bool) Global.testObject.trueValue).Should().Be(true);
            ((bool) Global.testObject.falseValue).Should().Be(false);
        }

        public void It_should_have_string_array_values()
        {
            ((string[]) Global.testObject.stringArray).Should().Equal(new string[] { "A", "B" });
            ((IReadOnlyCollection<string>) Global.testObject.stringArray).Should().Equal(new string[] { "A", "B" });
            ((IEnumerable<string>) Global.testObject.stringArray).Should().Equal(new string[] { "A", "B" });

            ((object[]) Global.testObject.stringArray).Should().Equal(new string[] { "A", "B" });
            ((IReadOnlyCollection<object>) Global.testObject.stringArray).Should().Equal(new string[] { "A", "B" });
            ((IEnumerable<object>) Global.testObject.stringArray).Should().Equal(new string[] { "A", "B" });

        }

        public void It_should_have_mixed_array_values()
        {
            var mixedObjArray = (object[]) Global.testObject.mixedArray;
            mixedObjArray.Length.Should().Be(3);
            ((string) mixedObjArray[0]).Should().Be("A");
            ((double) mixedObjArray[1]).Should().Be(42); // TODO DM 05.03.2020: Casting to (int) should_be supported
            ((int) ((dynamic) mixedObjArray[2]).iValue).Should().Be(42);
            ((string) ((dynamic) mixedObjArray[2]).strValue).Should().Be("strValue");

            var mixedDynArray = (IReadOnlyCollection<dynamic>) Global.testObject.mixedArray;
            mixedDynArray.Count.Should().Be(3);
            /*((string)mixedDynArray[0]).Should().Be("A");
            ((int)mixedDynArray[1]).Should().Be(42);
            ((int)((dynamic)mixedDynArray[2]).iValue).Should().Be(42);
            ((string)((dynamic)mixedDynArray[2]).strValue).Should().Be("strValue");*/

        }

        public void It_should_support_indexer()
        {
            dynamic array = Global.testObject["stringArray"];
            string entry1 = array[0];
            string entry2 = array[1];

            entry1.Should().Be("A");
            entry2.Should().Be("B");
        }

        public void It_should_accept_dynamic_numbers_back()
        {
            var result = (int) Global.testObject.addIntegerValue(Global.testObject.integerValue);
            result.Should().Be(84);
        }

        public void It_should_throw_on_funcThatThrows()
        {
            var accessError = new Action(() => Global.testObject.funcThatThrows());
            accessError.Should().Throw<InvalidOperationException>().WithMessage("Test error message");
        }

        public void It_should_invoke_passed_callback()
        {
            Global.testObject.invokeCallback("Ping", new Action<string>(arg => arg.Should().Be("PingPong")));
        }

        public void It_should_invoke_callback_with_int_argument()
        {
            Global.testObject.invokeIntCallback(42, new Action<long>(arg => arg.Should().Be(84)));
        }

        public void It_should_invoke_passed_callback_when_invoked_as_dynamic_object()
        {
            var invokeCallback = Global.testObject.invokeCallback;
            invokeCallback("Ping", new Action<string>(arg => arg.Should().Be("PingPong")));
        }

        public void It_should_pass_exceptions_from_passed_callback()
        {
            var shouldPassException = new Action(() =>
                Global.testObject.invokeCallback("Ping",
                    new Action<string>(_ =>
                    {
                        throw new InvalidOperationException("Test error message");
                    })));
            shouldPassException.Should().Throw<InvalidOperationException>();
        }

        public void It_should_get_Task_passed_as_Promise()
        {
            ((bool) Global.testObject.isPromise(Task.FromResult(5))).Should().Be(true);
        }

        public async Task It_should_return_Task_from_createPromise()
        {
            var result = await (Task<string>) Global.testObject.createPromise(true);
            result.Should().Be("Resolved");
        }

        // TODO DM 24.11.2019: This would require another bunch of reflection code to get working...
        /*public async Task It_should_await_result_from_createPromise()
        {
            string result = await global.testObject.createPromise(true);
            result.Should().Be("Resolved");
        }*/

        public async Task It_should_return_failed_Task_from_createPromise()
        {
            // DM 27.11.2019: We need the try/catch as FluentAssertions waits on the threadpool :(
            var didThrow = false;
            try
            {
                await (Task) Global.testObject.createPromise(false);
            }
            catch (Exception e)
            {
                didThrow = true;
                e.Message.Should().Contain("Error: As requested");
            }
            didThrow.Should().Be(true);

        }

        private sealed class UnsupportedType { }

        public async Task It_should_marshal_Task_with_unsupported_result_type_as_exception()
        {
            var task = Task.FromResult(new UnsupportedType());
            ((bool) Global.testObject.isPromise(task)).Should().Be(true);

            // DM 27.11.2019: We need the try/catch as FluentAssertions waits on the threadpool :(
            var didThrow = false;
            try
            {
                await (Task) Global.testObject.awaitPromise(task);
            }
            catch (Exception e)
            {
                didThrow = true;
                e.Message.Should().Contain("Error: InvalidOperationException: Unsupported object type for passing into JS: TestApp.Tests.Global_test_object+UnsupportedType");
            }
            didThrow.Should().Be(true);

        }

        public async Task It_should_marshal_Task_with_VoidTaskResult()
        {
            var task = new Func<Task>(async () => { await Task.Yield(); await Task.Yield(); })();
            ((bool) Global.testObject.isPromise(task)).Should().Be(true);

            await (Task) Global.testObject.awaitPromise(task);
        }

        public void It_should_instantiate_TestClass()
        {
            var inst = Global.testObject.TestClass.CreateNewInstance(42);
            ((int) inst.value).Should().Be(42);
        }

        public void It_should_call_TestClass_staticFunc()
        {
            var result = Global.testObject.TestClass.staticFunc(42);
            ((int) result).Should().Be(42 + 42);
        }

        public void It_should_support_require()
        {
            string sep = Global.require("path").sep;
            sep.Should().Be(System.IO.Path.DirectorySeparatorChar.ToString());
        }

        public void It_should_implement_equals_for_objects_and_functions()
        {
            var a = Node.New();
            Global.a = a;

            AssertionExtensions.Should(Equals(a, a)).BeTrue("Ref equal");
            AssertionExtensions.Should(Equals(a, Global.a)).Be(true);
            AssertionExtensions.Should(Equals(a, Global.testObject)).Be(false);
            AssertionExtensions.Should(Equals(Global.testObject, Global.testObject)).Be(true);
            AssertionExtensions.Should(Equals(Global.testObject, Global.it)).Be(false);
            AssertionExtensions.Should(Equals(Global.it, Global.it)).Be(true);

            AssertionExtensions.Should(a == a).BeTrue("Ref equal");
            AssertionExtensions.Should(a == Global.a).BeTrue("a == global.a");
            AssertionExtensions.Should(a == Global.testObject).Be(false);
            AssertionExtensions.Should(Global.testObject == Global.testObject).Be(true);
            AssertionExtensions.Should(Global.testObject == Global.it).Be(false);
            AssertionExtensions.Should(Global.it == Global.it).Be(true);

        }

        // TODO: Test hash code

        public void It_should_marshal_byte_arrays_to_JS()
        {
            var a = new byte[] { 1, 2, 3, 4 };
            Global.testObject.assertByteArray(a);
        }

        public void It_should_marshal_mixed_collections_to_js()
        {
            Global.testObject.assertMixedArray(new object[] { "a", 1, "b", 2 });
            Global.testObject.assertMixedArray(new List<object> { "a", 1, "b", 2 });
        }

        public void It_should_marshal_string_collections_to_js()
        {
            Global.testObject.assertStringArray(new string[] { "a", "b" });
            Global.testObject.assertStringArray(new List<string> { "a", "b" });
        }

        // TODO: support those collections
        /*public void It_should_marshal_primitive_collections_to_js()
        {                        
            global.testObject.assertIntArray(new int[] { 1, 2 });
            global.testObject.assertIntArray(new List<int> { 1, 2 });                        
        }*/

    }
}