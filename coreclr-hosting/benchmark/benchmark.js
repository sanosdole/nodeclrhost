module.exports = (suite, benchmark) => {

   suite('callback () -> ()', () => {

        benchmark('js', () => {
            dotnetCallbacks.jsVoidToVoid();
        });

        benchmark('dotnet', () => {
            dotnetCallbacks.cbVoidToVoid();
        });
    });

    suite('callback () -> Task', { promises: true }, () => {

        benchmark('js', () => {
            return dotnetCallbacks.jsVoidToTask();
        });

        benchmark('dotnet', () => {
            return dotnetCallbacks.cbVoidToTask();
        });
    });

    suite('callback (int) -> Task.Delay()', { promises: true }, () => {

        benchmark('js', () => {
            return dotnetCallbacks.jsIntToTaskDelay(10);
        });

        benchmark('dotnet', () => {
            return dotnetCallbacks.cbIntToTaskDelay(10);
        });

        benchmark('dotnet async', () => {
            return dotnetCallbacks.cbIntToTaskDelayAsync(10);
        });
    });

    suite('callback () -> Task.Yield()', { promises: true }, () => {

        benchmark('js (setImmediate)', () => {
            return dotnetCallbacks.jsVoidToTaskYield1();
        });

        benchmark('js (Promise.resolve)', () => {
            return dotnetCallbacks.jsVoidToTaskYield2();
        });

        benchmark('dotnet', () => {
            return dotnetCallbacks.cbVoidToTaskYield();
        });

        benchmark('dotnet Promise.resolve.then', () => {
            return dotnetCallbacks.cbVoidToTaskYieldPromise();
        });

        benchmark('dotnet async Promise.resolve', () => {
            return dotnetCallbacks.cbVoidToTaskYieldPromiseAsync();
        });
    });

    suite('callback (Task) -> Task', { promises: true }, () => {

        function delayedPromise() {
            return new Promise(r => {
                setImmediate(r);
            });
        }

        benchmark('js', () => {
            return dotnetCallbacks.jsTaskToTask(delayedPromise());
        });

        benchmark('dotnet', () => {
            return dotnetCallbacks.cbTaskToTask(delayedPromise());
        });
    });
}


