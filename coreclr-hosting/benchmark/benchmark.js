var assert = require('assert');

module.exports = (suite, benchmark) => {

    suite('callback () -> ()', () => {

        benchmark('noop', () => {
        });

        benchmark('js', () => {
            dotnetCallbacks.jsVoidToVoid();
        });

        benchmark('dotnet', () => {
            dotnetCallbacks.cbVoidToVoid();
        });
    });

    suite('callback (int,int) -> (int)', () => {

        benchmark('js', () => {
            let c = dotnetCallbacks.jsIntIntToInt(21015, 90518);
            assert.strictEqual(c, 21015 + 90518);
        });

        benchmark('dotnet', () => {
            let c = dotnetCallbacks.cbIntIntToInt(21015, 90518);
            assert.strictEqual(c, 21015 + 90518);
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

    suite('callback (int) -> Task.Delay(10)', { promises: true }, () => {

        benchmark('js 10', () => {
            return dotnetCallbacks.jsIntToTaskDelay(10);
        });

        benchmark('dotnet 10', () => {
            return dotnetCallbacks.cbIntToTaskDelay(10);
        });

        benchmark('dotnet async 10', () => {
            return dotnetCallbacks.cbIntToTaskDelayAsync(10);
        });
    });

    suite('callback (int) -> Task.Delay(100)', { promises: true }, () => {
        benchmark('js 100', () => {
            return dotnetCallbacks.jsIntToTaskDelay(100);
        });

        benchmark('dotnet 100', () => {
            return dotnetCallbacks.cbIntToTaskDelay(100);
        });

        benchmark('dotnet async 100', () => {
            return dotnetCallbacks.cbIntToTaskDelayAsync(100);
        });
    });

    suite('callback () -> Task.Yield()', { promises: true }, () => {

        benchmark('js (setImmediate)', () => {
            return dotnetCallbacks.jsVoidToTaskYield1();
        });

        benchmark('js (Promise.resolve)', () => {
            return dotnetCallbacks.jsVoidToTaskYield2();
        });

        benchmark('js (queueMicrotask)', () => {
            return dotnetCallbacks.jsVoidToTaskYield3();
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

    suite('callback (string[]) -> string', () => {

        let array = [
            "A",
            "B",
            "C",
            "D",
            " and sth. a little bit longer, so we do some real string joining",
            " and sth. a little bit longer, so we do some real string joining"
        ];
        let joinedArray = array.join("|");

        benchmark('js', () => {
            var r = dotnetCallbacks.jsStringArrayToString(array);
            assert.strictEqual(r, joinedArray);
        });

        benchmark('dotnet', () => {
            var r = dotnetCallbacks.cbStringArrayToString(array);
            assert.strictEqual(r, joinedArray);
        });
    });

    suite('callback (string) -> string[]', () => {

        const array = [
            "A",
            "B",
            "C",
            "D",
            " and sth. a little bit longer, so we do some real string joining",
            " and sth. a little bit longer, so we do some real string joining"
        ];
        const joinedArray = array.join("|");

        benchmark('js', () => {
            var r = dotnetCallbacks.jsStringToStringArray(joinedArray);
            assert.strictEqual(r.length, array.length);
            assert.strictEqual(r[0], array[0]);
            assert.strictEqual(r[5], array[5]);
        });

        benchmark('dotnet', () => {
            var r = dotnetCallbacks.cbStringToStringArray(joinedArray);
            assert.strictEqual(r[0], array[0]);
            assert.strictEqual(r[5], array[5]);
        });
    });
}


