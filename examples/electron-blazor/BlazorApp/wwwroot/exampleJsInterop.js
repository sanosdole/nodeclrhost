window.exampleJsFunctions = {
    getInputValue: function(inputElem) {

        return inputElem.value;
    },
    displayWelcome: function(welcomeMessage) {
        document.getElementById('welcome').innerText = welcomeMessage;
    },
    returnArrayAsyncJs: function() {
        DotNet.invokeMethodAsync('BlazorApp', 'ReturnArrayAsync')
            .then(data => {
                data.push(4);
                console.log(data);
            });
    },
    sayHello: function(dotnetHelper) {
        return dotnetHelper.invokeMethodAsync('SayHello')
            .then(r => console.log(r))
            .then(r => DotNet.invokeMethodAsync('BlazorApp', 'AsyncVoidTest', dotnetHelper))
            .then(r => console.log(r));
    },
    crash: function() {
        require("process").crash();
    },
    getUInt8Array: function() {
        return new Uint8Array(1000);
    },
    returnsNullObject: function() {
        return null;
    },
    createJsObject: function() {
        // A plain JS object we can read/write properties on via IJSObjectReference
        return { counter: 0, name: "test" };
    },
    getDirSelectionCount: function() {
        // Confirms the <input type=file webkitdirectory> change event fired
        return globalThis.__dirChangeCount || 0;
    }
};

// Native (non-Blazor) change-event counter for <input type=file webkitdirectory>.
// Lets us distinguish 'Electron never fires change on empty-dir selection' from
// 'Blazor event wiring drops it'.
globalThis.__dirChangeCount = 0;
document.addEventListener('change', function (evt) {
    var t = evt.target;
    if (t && t.tagName === 'INPUT' && t.type === 'file' && t.webkitdirectory) {
        globalThis.__dirChangeCount++;
        console.log('[dir:js-native] change event fired, files.length=' + (t.files ? t.files.length : -1));
    }
}, true);

// Stores the auto-test summary; read by an external test harness.
window.__setClientIssueResults = function (json) {
    window.__clientIssueResults = JSON.parse(json);
    console.log('[clientissues] auto-test results: ' + JSON.stringify(window.__clientIssueResults));
};