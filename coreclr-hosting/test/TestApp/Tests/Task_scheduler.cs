namespace TestApp.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using NodeHostEnvironment;

    public sealed class Task_scheduler : MochaTest
    {
        private readonly SynchronizationContext _mainCtx;
        private readonly TaskScheduler _mainScheduler;

        public Task_scheduler(SynchronizationContext mainCtx, TaskScheduler mainScheduler)
        {
            _mainCtx = mainCtx;
            _mainScheduler = mainScheduler;
        }

        public async Task It_should_set_proper_context_on_main()
        {
            await Node.Run(() =>
                           {
                               _mainCtx.Should().BeSameAs(SynchronizationContext.Current);
                               _mainScheduler.Should().BeSameAs(TaskScheduler.Current);
                           });
        }

        public async Task It_should_schedule_tasks_in_order_with_micro_tasks()
        {
            await Task.Delay(1);
            var outputs = new List<int>();
            var resolvedPromise = Node.Global.Promise.resolve();
            outputs.Add(0);
            resolvedPromise.then(new Action(() => outputs.Add(1)));
            await Task.Yield();
            resolvedPromise.then(new Action(() => outputs.Add(3)));
            resolvedPromise.then(new Action(() => outputs.Add(4)));
            outputs.Add(2);
            await Task.Yield();
            outputs.Add(5);
            await Task.Delay(0);

            outputs.Should().BeEquivalentTo(new [] { 0, 1, 2, 3, 4, 5 });
            outputs.Should().BeInAscendingOrder("ordered properly");
        }

        public async Task It_should_schedule_nested_tasks_in_order()
        {
            await Task.Delay(1);
            var outputs = new List<int>();
            var resolvedPromise = Node.Global.Promise.resolve();
            outputs.Add(0);
            var resultTask = (Task<int>)resolvedPromise.then(new Func<Task<int>>(async () =>
                                                                                 {
                                                                                     outputs.Add(1);
                                                                                     await (Task)resolvedPromise;
                                                                                     outputs.Add(3);
                                                                                     return 4;
                                                                                 }));
            await Task.Yield();
            outputs.Add(2);

            outputs.Add(await resultTask);

            outputs.Should().BeEquivalentTo(new [] { 0, 1, 2, 3, 4 });
            outputs.Should().BeInAscendingOrder("ordered properly");
        }

        public async Task It_should_schedule_from_background_task_in_order()
        {
            var outputs = new List<int>();
            var outputs2 = new List<int>();
            await Task.Run(() =>
                               Task.WhenAll(Enumerable.Range(0, 20)
                                                      .Select(i => Node.RunAsync(async () =>
                                                                                 {
                                                                                     outputs.Add(i);
                                                                                     await Task.Yield();
                                                                                     outputs2.Add(i);
                                                                                 }))));

            outputs.Count.Should().Be(20);
            outputs2.Count.Should().Be(20);
            outputs.Should().BeInAscendingOrder("ordered properly");
            outputs2.Should().BeInAscendingOrder("ordered properly");
        }
    }
}
