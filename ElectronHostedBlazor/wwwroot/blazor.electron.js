/******/ (function(modules) { // webpackBootstrap
/******/ 	// The module cache
/******/ 	var installedModules = {};
/******/
/******/ 	// The require function
/******/ 	function __webpack_require__(moduleId) {
/******/
/******/ 		// Check if module is in cache
/******/ 		if(installedModules[moduleId]) {
/******/ 			return installedModules[moduleId].exports;
/******/ 		}
/******/ 		// Create a new module (and put it into the cache)
/******/ 		var module = installedModules[moduleId] = {
/******/ 			i: moduleId,
/******/ 			l: false,
/******/ 			exports: {}
/******/ 		};
/******/
/******/ 		// Execute the module function
/******/ 		modules[moduleId].call(module.exports, module, module.exports, __webpack_require__);
/******/
/******/ 		// Flag the module as loaded
/******/ 		module.l = true;
/******/
/******/ 		// Return the exports of the module
/******/ 		return module.exports;
/******/ 	}
/******/
/******/
/******/ 	// expose the modules object (__webpack_modules__)
/******/ 	__webpack_require__.m = modules;
/******/
/******/ 	// expose the module cache
/******/ 	__webpack_require__.c = installedModules;
/******/
/******/ 	// define getter function for harmony exports
/******/ 	__webpack_require__.d = function(exports, name, getter) {
/******/ 		if(!__webpack_require__.o(exports, name)) {
/******/ 			Object.defineProperty(exports, name, { enumerable: true, get: getter });
/******/ 		}
/******/ 	};
/******/
/******/ 	// define __esModule on exports
/******/ 	__webpack_require__.r = function(exports) {
/******/ 		if(typeof Symbol !== 'undefined' && Symbol.toStringTag) {
/******/ 			Object.defineProperty(exports, Symbol.toStringTag, { value: 'Module' });
/******/ 		}
/******/ 		Object.defineProperty(exports, '__esModule', { value: true });
/******/ 	};
/******/
/******/ 	// create a fake namespace object
/******/ 	// mode & 1: value is a module id, require it
/******/ 	// mode & 2: merge all properties of value into the ns
/******/ 	// mode & 4: return value when already ns object
/******/ 	// mode & 8|1: behave like require
/******/ 	__webpack_require__.t = function(value, mode) {
/******/ 		if(mode & 1) value = __webpack_require__(value);
/******/ 		if(mode & 8) return value;
/******/ 		if((mode & 4) && typeof value === 'object' && value && value.__esModule) return value;
/******/ 		var ns = Object.create(null);
/******/ 		__webpack_require__.r(ns);
/******/ 		Object.defineProperty(ns, 'default', { enumerable: true, value: value });
/******/ 		if(mode & 2 && typeof value != 'string') for(var key in value) __webpack_require__.d(ns, key, function(key) { return value[key]; }.bind(null, key));
/******/ 		return ns;
/******/ 	};
/******/
/******/ 	// getDefaultExport function for compatibility with non-harmony modules
/******/ 	__webpack_require__.n = function(module) {
/******/ 		var getter = module && module.__esModule ?
/******/ 			function getDefault() { return module['default']; } :
/******/ 			function getModuleExports() { return module; };
/******/ 		__webpack_require__.d(getter, 'a', getter);
/******/ 		return getter;
/******/ 	};
/******/
/******/ 	// Object.prototype.hasOwnProperty.call
/******/ 	__webpack_require__.o = function(object, property) { return Object.prototype.hasOwnProperty.call(object, property); };
/******/
/******/ 	// __webpack_public_path__
/******/ 	__webpack_require__.p = "";
/******/
/******/
/******/ 	// Load entry module and return exports
/******/ 	return __webpack_require__(__webpack_require__.s = "./Boot.Electron.ts");
/******/ })
/************************************************************************/
/******/ ({

/***/ "../../coreclr-hosting/bindings.js":
/*!********************************************************************************!*\
  !*** /Users/danielmartin/nodeclrhost2/nodeclrhost/coreclr-hosting/bindings.js ***!
  \********************************************************************************/
/*! no static exports found */
/***/ (function(module, exports, __webpack_require__) {

/* WEBPACK VAR INJECTION */(function(process) {var coreclrHosting;

if (process.env.DEBUG) {
    coreclrHosting = __webpack_require__(/*! ./build/Debug/coreclr-hosting.node */ "../../coreclr-hosting/build/Debug/coreclr-hosting.node");
} else {
    coreclrHosting = __webpack_require__(/*! ./build/Release/coreclr-hosting.node */ "../../coreclr-hosting/build/Release/coreclr-hosting.node");
}

module.exports = coreclrHosting;

/* WEBPACK VAR INJECTION */}.call(this, __webpack_require__(/*! ./../ElectronHostedBlazor/node_modules/process/browser.js */ "../node_modules/process/browser.js")))

/***/ }),

/***/ "../../coreclr-hosting/build/Debug/coreclr-hosting.node":
/*!*****************************************************************************************************!*\
  !*** /Users/danielmartin/nodeclrhost2/nodeclrhost/coreclr-hosting/build/Debug/coreclr-hosting.node ***!
  \*****************************************************************************************************/
/*! no static exports found */
/***/ (function(module, exports, __webpack_require__) {

/* WEBPACK VAR INJECTION */(function(global, module) {try {global.process.dlopen(module, "/Users/danielmartin/nodeclrhost2/nodeclrhost/coreclr-hosting/build/Debug/coreclr-hosting.node"); } catch(e) {throw new Error('Cannot open ' + "/Users/danielmartin/nodeclrhost2/nodeclrhost/coreclr-hosting/build/Debug/coreclr-hosting.node" + ': ' + e);}
/* WEBPACK VAR INJECTION */}.call(this, __webpack_require__(/*! ./../../../ElectronHostedBlazor/node_modules/webpack/buildin/global.js */ "../node_modules/webpack/buildin/global.js"), __webpack_require__(/*! ./../../../ElectronHostedBlazor/node_modules/webpack/buildin/module.js */ "../node_modules/webpack/buildin/module.js")(module)))

/***/ }),

/***/ "../../coreclr-hosting/build/Release/coreclr-hosting.node":
/*!*******************************************************************************************************!*\
  !*** /Users/danielmartin/nodeclrhost2/nodeclrhost/coreclr-hosting/build/Release/coreclr-hosting.node ***!
  \*******************************************************************************************************/
/*! no static exports found */
/***/ (function(module, exports, __webpack_require__) {

/* WEBPACK VAR INJECTION */(function(global, module) {try {global.process.dlopen(module, "/Users/danielmartin/nodeclrhost2/nodeclrhost/coreclr-hosting/build/Release/coreclr-hosting.node"); } catch(e) {throw new Error('Cannot open ' + "/Users/danielmartin/nodeclrhost2/nodeclrhost/coreclr-hosting/build/Release/coreclr-hosting.node" + ': ' + e);}
/* WEBPACK VAR INJECTION */}.call(this, __webpack_require__(/*! ./../../../ElectronHostedBlazor/node_modules/webpack/buildin/global.js */ "../node_modules/webpack/buildin/global.js"), __webpack_require__(/*! ./../../../ElectronHostedBlazor/node_modules/webpack/buildin/module.js */ "../node_modules/webpack/buildin/module.js")(module)))

/***/ }),

/***/ "../node_modules/@dotnet/jsinterop/dist/Microsoft.JSInterop.js":
/*!*********************************************************************!*\
  !*** ../node_modules/@dotnet/jsinterop/dist/Microsoft.JSInterop.js ***!
  \*********************************************************************/
/*! no static exports found */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

// This is a single-file self-contained module to avoid the need for a Webpack build
var DotNet;
(function (DotNet) {
    window.DotNet = DotNet; // Ensure reachable from anywhere
    var jsonRevivers = [];
    var pendingAsyncCalls = {};
    var cachedJSFunctions = {};
    var nextAsyncCallId = 1; // Start at 1 because zero signals "no response needed"
    var dotNetDispatcher = null;
    /**
     * Sets the specified .NET call dispatcher as the current instance so that it will be used
     * for future invocations.
     *
     * @param dispatcher An object that can dispatch calls from JavaScript to a .NET runtime.
     */
    function attachDispatcher(dispatcher) {
        dotNetDispatcher = dispatcher;
    }
    DotNet.attachDispatcher = attachDispatcher;
    /**
     * Adds a JSON reviver callback that will be used when parsing arguments received from .NET.
     * @param reviver The reviver to add.
     */
    function attachReviver(reviver) {
        jsonRevivers.push(reviver);
    }
    DotNet.attachReviver = attachReviver;
    /**
     * Invokes the specified .NET public method synchronously. Not all hosting scenarios support
     * synchronous invocation, so if possible use invokeMethodAsync instead.
     *
     * @param assemblyName The short name (without key/version or .dll extension) of the .NET assembly containing the method.
     * @param methodIdentifier The identifier of the method to invoke. The method must have a [JSInvokable] attribute specifying this identifier.
     * @param args Arguments to pass to the method, each of which must be JSON-serializable.
     * @returns The result of the operation.
     */
    function invokeMethod(assemblyName, methodIdentifier) {
        var args = [];
        for (var _i = 2; _i < arguments.length; _i++) {
            args[_i - 2] = arguments[_i];
        }
        return invokePossibleInstanceMethod(assemblyName, methodIdentifier, null, args);
    }
    DotNet.invokeMethod = invokeMethod;
    /**
     * Invokes the specified .NET public method asynchronously.
     *
     * @param assemblyName The short name (without key/version or .dll extension) of the .NET assembly containing the method.
     * @param methodIdentifier The identifier of the method to invoke. The method must have a [JSInvokable] attribute specifying this identifier.
     * @param args Arguments to pass to the method, each of which must be JSON-serializable.
     * @returns A promise representing the result of the operation.
     */
    function invokeMethodAsync(assemblyName, methodIdentifier) {
        var args = [];
        for (var _i = 2; _i < arguments.length; _i++) {
            args[_i - 2] = arguments[_i];
        }
        return invokePossibleInstanceMethodAsync(assemblyName, methodIdentifier, null, args);
    }
    DotNet.invokeMethodAsync = invokeMethodAsync;
    function invokePossibleInstanceMethod(assemblyName, methodIdentifier, dotNetObjectId, args) {
        var dispatcher = getRequiredDispatcher();
        if (dispatcher.invokeDotNetFromJS) {
            var argsJson = JSON.stringify(args, argReplacer);
            var resultJson = dispatcher.invokeDotNetFromJS(assemblyName, methodIdentifier, dotNetObjectId, argsJson);
            return resultJson ? parseJsonWithRevivers(resultJson) : null;
        }
        else {
            throw new Error('The current dispatcher does not support synchronous calls from JS to .NET. Use invokeMethodAsync instead.');
        }
    }
    function invokePossibleInstanceMethodAsync(assemblyName, methodIdentifier, dotNetObjectId, args) {
        if (assemblyName && dotNetObjectId) {
            throw new Error("For instance method calls, assemblyName should be null. Received '" + assemblyName + "'.");
        }
        var asyncCallId = nextAsyncCallId++;
        var resultPromise = new Promise(function (resolve, reject) {
            pendingAsyncCalls[asyncCallId] = { resolve: resolve, reject: reject };
        });
        try {
            var argsJson = JSON.stringify(args, argReplacer);
            getRequiredDispatcher().beginInvokeDotNetFromJS(asyncCallId, assemblyName, methodIdentifier, dotNetObjectId, argsJson);
        }
        catch (ex) {
            // Synchronous failure
            completePendingCall(asyncCallId, false, ex);
        }
        return resultPromise;
    }
    function getRequiredDispatcher() {
        if (dotNetDispatcher !== null) {
            return dotNetDispatcher;
        }
        throw new Error('No .NET call dispatcher has been set.');
    }
    function completePendingCall(asyncCallId, success, resultOrError) {
        if (!pendingAsyncCalls.hasOwnProperty(asyncCallId)) {
            throw new Error("There is no pending async call with ID " + asyncCallId + ".");
        }
        var asyncCall = pendingAsyncCalls[asyncCallId];
        delete pendingAsyncCalls[asyncCallId];
        if (success) {
            asyncCall.resolve(resultOrError);
        }
        else {
            asyncCall.reject(resultOrError);
        }
    }
    /**
     * Receives incoming calls from .NET and dispatches them to JavaScript.
     */
    DotNet.jsCallDispatcher = {
        /**
         * Finds the JavaScript function matching the specified identifier.
         *
         * @param identifier Identifies the globally-reachable function to be returned.
         * @returns A Function instance.
         */
        findJSFunction: findJSFunction,
        /**
         * Invokes the specified synchronous JavaScript function.
         *
         * @param identifier Identifies the globally-reachable function to invoke.
         * @param argsJson JSON representation of arguments to be passed to the function.
         * @returns JSON representation of the invocation result.
         */
        invokeJSFromDotNet: function (identifier, argsJson) {
            var result = findJSFunction(identifier).apply(null, parseJsonWithRevivers(argsJson));
            return result === null || result === undefined
                ? null
                : JSON.stringify(result, argReplacer);
        },
        /**
         * Invokes the specified synchronous or asynchronous JavaScript function.
         *
         * @param asyncHandle A value identifying the asynchronous operation. This value will be passed back in a later call to endInvokeJSFromDotNet.
         * @param identifier Identifies the globally-reachable function to invoke.
         * @param argsJson JSON representation of arguments to be passed to the function.
         */
        beginInvokeJSFromDotNet: function (asyncHandle, identifier, argsJson) {
            // Coerce synchronous functions into async ones, plus treat
            // synchronous exceptions the same as async ones
            var promise = new Promise(function (resolve) {
                var synchronousResultOrPromise = findJSFunction(identifier).apply(null, parseJsonWithRevivers(argsJson));
                resolve(synchronousResultOrPromise);
            });
            // We only listen for a result if the caller wants to be notified about it
            if (asyncHandle) {
                // On completion, dispatch result back to .NET
                // Not using "await" because it codegens a lot of boilerplate
                promise.then(function (result) { return getRequiredDispatcher().endInvokeJSFromDotNet(asyncHandle, true, JSON.stringify([asyncHandle, true, result], argReplacer)); }, function (error) { return getRequiredDispatcher().endInvokeJSFromDotNet(asyncHandle, false, JSON.stringify([asyncHandle, false, formatError(error)])); });
            }
        },
        /**
         * Receives notification that an async call from JS to .NET has completed.
         * @param asyncCallId The identifier supplied in an earlier call to beginInvokeDotNetFromJS.
         * @param success A flag to indicate whether the operation completed successfully.
         * @param resultOrExceptionMessage Either the operation result or an error message.
         */
        endInvokeDotNetFromJS: function (asyncCallId, success, resultOrExceptionMessage) {
            var resultOrError = success ? resultOrExceptionMessage : new Error(resultOrExceptionMessage);
            completePendingCall(parseInt(asyncCallId), success, resultOrError);
        }
    };
    function parseJsonWithRevivers(json) {
        return json ? JSON.parse(json, function (key, initialValue) {
            // Invoke each reviver in order, passing the output from the previous reviver,
            // so that each one gets a chance to transform the value
            return jsonRevivers.reduce(function (latestValue, reviver) { return reviver(key, latestValue); }, initialValue);
        }) : null;
    }
    function formatError(error) {
        if (error instanceof Error) {
            return error.message + "\n" + error.stack;
        }
        else {
            return error ? error.toString() : 'null';
        }
    }
    function findJSFunction(identifier) {
        if (cachedJSFunctions.hasOwnProperty(identifier)) {
            return cachedJSFunctions[identifier];
        }
        var result = window;
        var resultIdentifier = 'window';
        var lastSegmentValue;
        identifier.split('.').forEach(function (segment) {
            if (segment in result) {
                lastSegmentValue = result;
                result = result[segment];
                resultIdentifier += '.' + segment;
            }
            else {
                throw new Error("Could not find '" + segment + "' in '" + resultIdentifier + "'.");
            }
        });
        if (result instanceof Function) {
            result = result.bind(lastSegmentValue);
            cachedJSFunctions[identifier] = result;
            return result;
        }
        else {
            throw new Error("The value '" + resultIdentifier + "' is not a function.");
        }
    }
    var DotNetObject = /** @class */ (function () {
        function DotNetObject(_id) {
            this._id = _id;
        }
        DotNetObject.prototype.invokeMethod = function (methodIdentifier) {
            var args = [];
            for (var _i = 1; _i < arguments.length; _i++) {
                args[_i - 1] = arguments[_i];
            }
            return invokePossibleInstanceMethod(null, methodIdentifier, this._id, args);
        };
        DotNetObject.prototype.invokeMethodAsync = function (methodIdentifier) {
            var args = [];
            for (var _i = 1; _i < arguments.length; _i++) {
                args[_i - 1] = arguments[_i];
            }
            return invokePossibleInstanceMethodAsync(null, methodIdentifier, this._id, args);
        };
        DotNetObject.prototype.dispose = function () {
            var promise = invokePossibleInstanceMethodAsync(null, '__Dispose', this._id, null);
            promise.catch(function (error) { return console.error(error); });
        };
        DotNetObject.prototype.serializeAsArg = function () {
            return { __dotNetObject: this._id };
        };
        return DotNetObject;
    }());
    var dotNetObjectRefKey = '__dotNetObject';
    attachReviver(function reviveDotNetObject(key, value) {
        if (value && typeof value === 'object' && value.hasOwnProperty(dotNetObjectRefKey)) {
            return new DotNetObject(value.__dotNetObject);
        }
        // Unrecognized - let another reviver handle it
        return value;
    });
    function argReplacer(key, value) {
        return value instanceof DotNetObject ? value.serializeAsArg() : value;
    }
})(DotNet || (DotNet = {}));
//# sourceMappingURL=Microsoft.JSInterop.js.map

/***/ }),

/***/ "../node_modules/process/browser.js":
/*!******************************************!*\
  !*** ../node_modules/process/browser.js ***!
  \******************************************/
/*! no static exports found */
/***/ (function(module, exports) {

// shim for using process in browser
var process = module.exports = {};

// cached from whatever global is present so that test runners that stub it
// don't break things.  But we need to wrap it in a try catch in case it is
// wrapped in strict mode code which doesn't define any globals.  It's inside a
// function because try/catches deoptimize in certain engines.

var cachedSetTimeout;
var cachedClearTimeout;

function defaultSetTimout() {
    throw new Error('setTimeout has not been defined');
}
function defaultClearTimeout () {
    throw new Error('clearTimeout has not been defined');
}
(function () {
    try {
        if (typeof setTimeout === 'function') {
            cachedSetTimeout = setTimeout;
        } else {
            cachedSetTimeout = defaultSetTimout;
        }
    } catch (e) {
        cachedSetTimeout = defaultSetTimout;
    }
    try {
        if (typeof clearTimeout === 'function') {
            cachedClearTimeout = clearTimeout;
        } else {
            cachedClearTimeout = defaultClearTimeout;
        }
    } catch (e) {
        cachedClearTimeout = defaultClearTimeout;
    }
} ())
function runTimeout(fun) {
    if (cachedSetTimeout === setTimeout) {
        //normal enviroments in sane situations
        return setTimeout(fun, 0);
    }
    // if setTimeout wasn't available but was latter defined
    if ((cachedSetTimeout === defaultSetTimout || !cachedSetTimeout) && setTimeout) {
        cachedSetTimeout = setTimeout;
        return setTimeout(fun, 0);
    }
    try {
        // when when somebody has screwed with setTimeout but no I.E. maddness
        return cachedSetTimeout(fun, 0);
    } catch(e){
        try {
            // When we are in I.E. but the script has been evaled so I.E. doesn't trust the global object when called normally
            return cachedSetTimeout.call(null, fun, 0);
        } catch(e){
            // same as above but when it's a version of I.E. that must have the global object for 'this', hopfully our context correct otherwise it will throw a global error
            return cachedSetTimeout.call(this, fun, 0);
        }
    }


}
function runClearTimeout(marker) {
    if (cachedClearTimeout === clearTimeout) {
        //normal enviroments in sane situations
        return clearTimeout(marker);
    }
    // if clearTimeout wasn't available but was latter defined
    if ((cachedClearTimeout === defaultClearTimeout || !cachedClearTimeout) && clearTimeout) {
        cachedClearTimeout = clearTimeout;
        return clearTimeout(marker);
    }
    try {
        // when when somebody has screwed with setTimeout but no I.E. maddness
        return cachedClearTimeout(marker);
    } catch (e){
        try {
            // When we are in I.E. but the script has been evaled so I.E. doesn't  trust the global object when called normally
            return cachedClearTimeout.call(null, marker);
        } catch (e){
            // same as above but when it's a version of I.E. that must have the global object for 'this', hopfully our context correct otherwise it will throw a global error.
            // Some versions of I.E. have different rules for clearTimeout vs setTimeout
            return cachedClearTimeout.call(this, marker);
        }
    }



}
var queue = [];
var draining = false;
var currentQueue;
var queueIndex = -1;

function cleanUpNextTick() {
    if (!draining || !currentQueue) {
        return;
    }
    draining = false;
    if (currentQueue.length) {
        queue = currentQueue.concat(queue);
    } else {
        queueIndex = -1;
    }
    if (queue.length) {
        drainQueue();
    }
}

function drainQueue() {
    if (draining) {
        return;
    }
    var timeout = runTimeout(cleanUpNextTick);
    draining = true;

    var len = queue.length;
    while(len) {
        currentQueue = queue;
        queue = [];
        while (++queueIndex < len) {
            if (currentQueue) {
                currentQueue[queueIndex].run();
            }
        }
        queueIndex = -1;
        len = queue.length;
    }
    currentQueue = null;
    draining = false;
    runClearTimeout(timeout);
}

process.nextTick = function (fun) {
    var args = new Array(arguments.length - 1);
    if (arguments.length > 1) {
        for (var i = 1; i < arguments.length; i++) {
            args[i - 1] = arguments[i];
        }
    }
    queue.push(new Item(fun, args));
    if (queue.length === 1 && !draining) {
        runTimeout(drainQueue);
    }
};

// v8 likes predictible objects
function Item(fun, array) {
    this.fun = fun;
    this.array = array;
}
Item.prototype.run = function () {
    this.fun.apply(null, this.array);
};
process.title = 'browser';
process.browser = true;
process.env = {};
process.argv = [];
process.version = ''; // empty string to avoid regexp issues
process.versions = {};

function noop() {}

process.on = noop;
process.addListener = noop;
process.once = noop;
process.off = noop;
process.removeListener = noop;
process.removeAllListeners = noop;
process.emit = noop;
process.prependListener = noop;
process.prependOnceListener = noop;

process.listeners = function (name) { return [] }

process.binding = function (name) {
    throw new Error('process.binding is not supported');
};

process.cwd = function () { return '/' };
process.chdir = function (dir) {
    throw new Error('process.chdir is not supported');
};
process.umask = function() { return 0; };


/***/ }),

/***/ "../node_modules/webpack/buildin/global.js":
/*!*************************************************!*\
  !*** ../node_modules/webpack/buildin/global.js ***!
  \*************************************************/
/*! no static exports found */
/***/ (function(module, exports) {

var g;

// This works in non-strict mode
g = (function() {
	return this;
})();

try {
	// This works if eval is allowed (see CSP)
	g = g || new Function("return this")();
} catch (e) {
	// This works if the window reference is available
	if (typeof window === "object") g = window;
}

// g can still be undefined, but nothing to do about it...
// We return undefined, instead of nothing here, so it's
// easier to handle this case. if(!global) { ...}

module.exports = g;


/***/ }),

/***/ "../node_modules/webpack/buildin/module.js":
/*!*************************************************!*\
  !*** ../node_modules/webpack/buildin/module.js ***!
  \*************************************************/
/*! no static exports found */
/***/ (function(module, exports) {

module.exports = function(module) {
	if (!module.webpackPolyfill) {
		module.deprecate = function() {};
		module.paths = [];
		// module.parent = undefined by default
		if (!module.children) module.children = [];
		Object.defineProperty(module, "loaded", {
			enumerable: true,
			get: function() {
				return module.l;
			}
		});
		Object.defineProperty(module, "id", {
			enumerable: true,
			get: function() {
				return module.i;
			}
		});
		module.webpackPolyfill = 1;
	}
	return module;
};


/***/ }),

/***/ "./Boot.Electron.ts":
/*!**************************!*\
  !*** ./Boot.Electron.ts ***!
  \**************************/
/*! no static exports found */
/***/ (function(module, exports, __webpack_require__) {

"use strict";
/* WEBPACK VAR INJECTION */(function(process) {
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
Object.defineProperty(exports, "__esModule", { value: true });
__webpack_require__(/*! @dotnet/jsinterop */ "../node_modules/@dotnet/jsinterop/dist/Microsoft.JSInterop.js");
__webpack_require__(/*! ./GlobalExports */ "./GlobalExports.ts");
const Renderer_1 = __webpack_require__(/*! ./Rendering/Renderer */ "./Rendering/Renderer.ts");
const OutOfProcessRenderBatch_1 = __webpack_require__(/*! ./Rendering/RenderBatch/OutOfProcessRenderBatch */ "./Rendering/RenderBatch/OutOfProcessRenderBatch.ts");
const BootCommon_1 = __webpack_require__(/*! ./BootCommon */ "./BootCommon.ts");
const RendererEventDispatcher_1 = __webpack_require__(/*! ./Rendering/RendererEventDispatcher */ "./Rendering/RendererEventDispatcher.ts");
const coreclrhosting = __webpack_require__(/*! coreclr-hosting */ "../../coreclr-hosting/bindings.js");
let started = false;
function runApp(basePath, appFile) {
    function boot(options) {
        return __awaiter(this, void 0, void 0, function* () {
            if (started) {
                throw new Error('Blazor has already started.');
            }
            started = true;
            RendererEventDispatcher_1.setEventDispatcher((eventDescriptor, eventArgs) => window['Blazor']._internal.HandleRendererEvent(eventDescriptor, JSON.stringify(eventArgs)));
            //DotNet.invokeMethodAsync('Microsoft.AspNetCore.Blazor', 'DispatchEvent', eventDescriptor, JSON.stringify(eventArgs)));
            // Configure environment for execution under Mono WebAssembly with shared-memory rendering
            /*const platform = Environment.setPlatform(monoPlatform);
            window['Blazor'].platform = platform;
            window['Blazor']._internal.renderBatch = (browserRendererId: number, batchAddress: Pointer) => {
              renderBatch(browserRendererId, new SharedMemoryRenderBatch(batchAddress));
            };*/
            // TODO DM 26.08.2019: Navigation is different on trunk compared to preview 8
            // Configure navigation via JS Interop
            /*window['Blazor']._internal.navigationManager.listenForNavigationEvents(async (uri: string, intercepted: boolean): Promise<void> => {
              await DotNet.invokeMethodAsync(
                'Microsoft.AspNetCore.Blazor',
                'NotifyLocationChanged',
                uri,
                intercepted
              );
            });*/
            // DM 21.08.2019: Setting up the renderer
            window['Blazor']._internal.renderBatch = (browserRendererId, batchAddress) => {
                try {
                    var typedArray = new Uint8Array(batchAddress);
                    console.info("rendering batch of size " + typedArray.byteLength + " and first byte " + typedArray[0]);
                    Renderer_1.renderBatch(browserRendererId, new OutOfProcessRenderBatch_1.OutOfProcessRenderBatch(typedArray));
                }
                catch (error) {
                    console.error(error);
                }
            };
            // TODO: module.exports | do not use __dirname | use function arguments!
            // DM 21.08.2019: Start the blazor app
            console.info("Running in process " + process.pid);
            console.info("Starting from " + basePath);
            var result = coreclrhosting.runCoreApp(basePath, appFile);
            console.info("Main returned: " + result);
            //console.info(window["Blazor"]._internal);
            /*
              // Fetch the boot JSON file
              const bootConfig = await fetchBootConfigAsync();
              const embeddedResourcesPromise = loadEmbeddedResourcesAsync(bootConfig);
            
              if (!bootConfig.linkerEnabled) {
                console.info('Blazor is running in dev mode without IL stripping. To make the bundle size significantly smaller, publish the application or see https://go.microsoft.com/fwlink/?linkid=870414');
              }
            
              // Determine the URLs of the assemblies we want to load, then begin fetching them all
              const loadAssemblyUrls = [bootConfig.main]
                .concat(bootConfig.assemblyReferences)
                .map(filename => `_framework/_bin/${filename}`);
            
              try {
                await platform.start(loadAssemblyUrls);
              } catch (ex) {
                throw new Error(`Failed to start platform. Reason: ${ex}`);
              }
            
              // Before we start running .NET code, be sure embedded content resources are all loaded
              await embeddedResourcesPromise;
            
              // Start up the application
              const mainAssemblyName = getAssemblyNameFromUrl(bootConfig.main);
              platform.callEntryPoint(mainAssemblyName, bootConfig.entryPoint, []);
              */
        });
    }
    window['Blazor'].start = boot;
    if (BootCommon_1.shouldAutoStart()) {
        boot();
    }
}
exports.runApp = runApp;
exports.runBlazorApp = runApp;
window['runBlazorApp'] = runApp;

/* WEBPACK VAR INJECTION */}.call(this, __webpack_require__(/*! ./../node_modules/process/browser.js */ "../node_modules/process/browser.js")))

/***/ }),

/***/ "./BootCommon.ts":
/*!***********************!*\
  !*** ./BootCommon.ts ***!
  \***********************/
/*! no static exports found */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
Object.defineProperty(exports, "__esModule", { value: true });
function fetchBootConfigAsync() {
    return __awaiter(this, void 0, void 0, function* () {
        // Later we might make the location of this configurable (e.g., as an attribute on the <script>
        // element that's importing this file), but currently there isn't a use case for that.
        const bootConfigResponse = yield fetch('_framework/blazor.boot.json', { method: 'Get', credentials: 'include' });
        return bootConfigResponse.json();
    });
}
exports.fetchBootConfigAsync = fetchBootConfigAsync;
function loadEmbeddedResourcesAsync(bootConfig) {
    const cssLoadingPromises = bootConfig.cssReferences.map(cssReference => {
        const linkElement = document.createElement('link');
        linkElement.rel = 'stylesheet';
        linkElement.href = cssReference;
        return loadResourceFromElement(linkElement);
    });
    const jsLoadingPromises = bootConfig.jsReferences.map(jsReference => {
        const scriptElement = document.createElement('script');
        scriptElement.src = jsReference;
        return loadResourceFromElement(scriptElement);
    });
    return Promise.all(cssLoadingPromises.concat(jsLoadingPromises));
}
exports.loadEmbeddedResourcesAsync = loadEmbeddedResourcesAsync;
function loadResourceFromElement(element) {
    return new Promise((resolve, reject) => {
        element.onload = resolve;
        element.onerror = reject;
        document.head.appendChild(element);
    });
}
// Tells you if the script was added without <script src="..." autostart="false"></script>
function shouldAutoStart() {
    return !!(document &&
        document.currentScript &&
        document.currentScript.getAttribute('autostart') !== 'false');
}
exports.shouldAutoStart = shouldAutoStart;


/***/ }),

/***/ "./Environment.ts":
/*!************************!*\
  !*** ./Environment.ts ***!
  \************************/
/*! no static exports found */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

Object.defineProperty(exports, "__esModule", { value: true });
function setPlatform(platformInstance) {
    exports.platform = platformInstance;
    return exports.platform;
}
exports.setPlatform = setPlatform;


/***/ }),

/***/ "./GlobalExports.ts":
/*!**************************!*\
  !*** ./GlobalExports.ts ***!
  \**************************/
/*! no static exports found */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

Object.defineProperty(exports, "__esModule", { value: true });
const NavigationManager_1 = __webpack_require__(/*! ./Services/NavigationManager */ "./Services/NavigationManager.ts");
const Http_1 = __webpack_require__(/*! ./Services/Http */ "./Services/Http.ts");
const Renderer_1 = __webpack_require__(/*! ./Rendering/Renderer */ "./Rendering/Renderer.ts");
// Make the following APIs available in global scope for invocation from JS
window['Blazor'] = {
    navigateTo: NavigationManager_1.navigateTo,
    _internal: {
        attachRootComponentToElement: Renderer_1.attachRootComponentToElement,
        http: Http_1.internalFunctions,
        navigationManager: NavigationManager_1.internalFunctions,
    },
};


/***/ }),

/***/ "./Platform/Platform.ts":
/*!******************************!*\
  !*** ./Platform/Platform.ts ***!
  \******************************/
/*! no static exports found */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

Object.defineProperty(exports, "__esModule", { value: true });


/***/ }),

/***/ "./Rendering/BrowserRenderer.ts":
/*!**************************************!*\
  !*** ./Rendering/BrowserRenderer.ts ***!
  \**************************************/
/*! no static exports found */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

Object.defineProperty(exports, "__esModule", { value: true });
const RenderBatch_1 = __webpack_require__(/*! ./RenderBatch/RenderBatch */ "./Rendering/RenderBatch/RenderBatch.ts");
const EventDelegator_1 = __webpack_require__(/*! ./EventDelegator */ "./Rendering/EventDelegator.ts");
const LogicalElements_1 = __webpack_require__(/*! ./LogicalElements */ "./Rendering/LogicalElements.ts");
const ElementReferenceCapture_1 = __webpack_require__(/*! ./ElementReferenceCapture */ "./Rendering/ElementReferenceCapture.ts");
const RendererEventDispatcher_1 = __webpack_require__(/*! ./RendererEventDispatcher */ "./Rendering/RendererEventDispatcher.ts");
const selectValuePropname = '_blazorSelectValue';
const sharedTemplateElemForParsing = document.createElement('template');
const sharedSvgElemForParsing = document.createElementNS('http://www.w3.org/2000/svg', 'g');
const preventDefaultEvents = { submit: true };
const rootComponentsPendingFirstRender = {};
class BrowserRenderer {
    constructor(browserRendererId) {
        this.childComponentLocations = {};
        this.browserRendererId = browserRendererId;
        this.eventDelegator = new EventDelegator_1.EventDelegator((event, eventHandlerId, eventArgs, eventFieldInfo) => {
            raiseEvent(event, this.browserRendererId, eventHandlerId, eventArgs, eventFieldInfo);
        });
    }
    attachRootComponentToLogicalElement(componentId, element) {
        this.attachComponentToElement(componentId, element);
        rootComponentsPendingFirstRender[componentId] = element;
    }
    updateComponent(batch, componentId, edits, referenceFrames) {
        const element = this.childComponentLocations[componentId];
        if (!element) {
            throw new Error(`No element is currently associated with component ${componentId}`);
        }
        // On the first render for each root component, clear any existing content (e.g., prerendered)
        const rootElementToClear = rootComponentsPendingFirstRender[componentId];
        if (rootElementToClear) {
            const rootElementToClearEnd = LogicalElements_1.getLogicalSiblingEnd(rootElementToClear);
            delete rootComponentsPendingFirstRender[componentId];
            if (!rootElementToClearEnd) {
                clearElement(rootElementToClear);
            }
            else {
                clearBetween(rootElementToClear, rootElementToClearEnd);
            }
        }
        const ownerDocument = LogicalElements_1.getClosestDomElement(element).ownerDocument;
        const activeElementBefore = ownerDocument && ownerDocument.activeElement;
        this.applyEdits(batch, componentId, element, 0, edits, referenceFrames);
        // Try to restore focus in case it was lost due to an element move
        if ((activeElementBefore instanceof HTMLElement) && ownerDocument && ownerDocument.activeElement !== activeElementBefore) {
            activeElementBefore.focus();
        }
    }
    disposeComponent(componentId) {
        delete this.childComponentLocations[componentId];
    }
    disposeEventHandler(eventHandlerId) {
        this.eventDelegator.removeListener(eventHandlerId);
    }
    attachComponentToElement(componentId, element) {
        this.childComponentLocations[componentId] = element;
    }
    applyEdits(batch, componentId, parent, childIndex, edits, referenceFrames) {
        let currentDepth = 0;
        let childIndexAtCurrentDepth = childIndex;
        let permutationList;
        const arrayBuilderSegmentReader = batch.arrayBuilderSegmentReader;
        const editReader = batch.editReader;
        const frameReader = batch.frameReader;
        const editsValues = arrayBuilderSegmentReader.values(edits);
        const editsOffset = arrayBuilderSegmentReader.offset(edits);
        const editsLength = arrayBuilderSegmentReader.count(edits);
        const maxEditIndexExcl = editsOffset + editsLength;
        for (let editIndex = editsOffset; editIndex < maxEditIndexExcl; editIndex++) {
            const edit = batch.diffReader.editsEntry(editsValues, editIndex);
            const editType = editReader.editType(edit);
            switch (editType) {
                case RenderBatch_1.EditType.prependFrame: {
                    const frameIndex = editReader.newTreeIndex(edit);
                    const frame = batch.referenceFramesEntry(referenceFrames, frameIndex);
                    const siblingIndex = editReader.siblingIndex(edit);
                    this.insertFrame(batch, componentId, parent, childIndexAtCurrentDepth + siblingIndex, referenceFrames, frame, frameIndex);
                    break;
                }
                case RenderBatch_1.EditType.removeFrame: {
                    const siblingIndex = editReader.siblingIndex(edit);
                    LogicalElements_1.removeLogicalChild(parent, childIndexAtCurrentDepth + siblingIndex);
                    break;
                }
                case RenderBatch_1.EditType.setAttribute: {
                    const frameIndex = editReader.newTreeIndex(edit);
                    const frame = batch.referenceFramesEntry(referenceFrames, frameIndex);
                    const siblingIndex = editReader.siblingIndex(edit);
                    const element = LogicalElements_1.getLogicalChild(parent, childIndexAtCurrentDepth + siblingIndex);
                    if (element instanceof Element) {
                        this.applyAttribute(batch, componentId, element, frame);
                    }
                    else {
                        throw new Error('Cannot set attribute on non-element child');
                    }
                    break;
                }
                case RenderBatch_1.EditType.removeAttribute: {
                    // Note that we don't have to dispose the info we track about event handlers here, because the
                    // disposed event handler IDs are delivered separately (in the 'disposedEventHandlerIds' array)
                    const siblingIndex = editReader.siblingIndex(edit);
                    const element = LogicalElements_1.getLogicalChild(parent, childIndexAtCurrentDepth + siblingIndex);
                    if (element instanceof HTMLElement) {
                        const attributeName = editReader.removedAttributeName(edit);
                        // First try to remove any special property we use for this attribute
                        if (!this.tryApplySpecialProperty(batch, element, attributeName, null)) {
                            // If that's not applicable, it's a regular DOM attribute so remove that
                            element.removeAttribute(attributeName);
                        }
                    }
                    else {
                        throw new Error('Cannot remove attribute from non-element child');
                    }
                    break;
                }
                case RenderBatch_1.EditType.updateText: {
                    const frameIndex = editReader.newTreeIndex(edit);
                    const frame = batch.referenceFramesEntry(referenceFrames, frameIndex);
                    const siblingIndex = editReader.siblingIndex(edit);
                    const textNode = LogicalElements_1.getLogicalChild(parent, childIndexAtCurrentDepth + siblingIndex);
                    if (textNode instanceof Text) {
                        textNode.textContent = frameReader.textContent(frame);
                    }
                    else {
                        throw new Error('Cannot set text content on non-text child');
                    }
                    break;
                }
                case RenderBatch_1.EditType.updateMarkup: {
                    const frameIndex = editReader.newTreeIndex(edit);
                    const frame = batch.referenceFramesEntry(referenceFrames, frameIndex);
                    const siblingIndex = editReader.siblingIndex(edit);
                    LogicalElements_1.removeLogicalChild(parent, childIndexAtCurrentDepth + siblingIndex);
                    this.insertMarkup(batch, parent, childIndexAtCurrentDepth + siblingIndex, frame);
                    break;
                }
                case RenderBatch_1.EditType.stepIn: {
                    const siblingIndex = editReader.siblingIndex(edit);
                    parent = LogicalElements_1.getLogicalChild(parent, childIndexAtCurrentDepth + siblingIndex);
                    currentDepth++;
                    childIndexAtCurrentDepth = 0;
                    break;
                }
                case RenderBatch_1.EditType.stepOut: {
                    parent = LogicalElements_1.getLogicalParent(parent);
                    currentDepth--;
                    childIndexAtCurrentDepth = currentDepth === 0 ? childIndex : 0; // The childIndex is only ever nonzero at zero depth
                    break;
                }
                case RenderBatch_1.EditType.permutationListEntry: {
                    permutationList = permutationList || [];
                    permutationList.push({
                        fromSiblingIndex: childIndexAtCurrentDepth + editReader.siblingIndex(edit),
                        toSiblingIndex: childIndexAtCurrentDepth + editReader.moveToSiblingIndex(edit),
                    });
                    break;
                }
                case RenderBatch_1.EditType.permutationListEnd: {
                    LogicalElements_1.permuteLogicalChildren(parent, permutationList);
                    permutationList = undefined;
                    break;
                }
                default: {
                    const unknownType = editType; // Compile-time verification that the switch was exhaustive
                    throw new Error(`Unknown edit type: ${unknownType}`);
                }
            }
        }
    }
    insertFrame(batch, componentId, parent, childIndex, frames, frame, frameIndex) {
        const frameReader = batch.frameReader;
        const frameType = frameReader.frameType(frame);
        switch (frameType) {
            case RenderBatch_1.FrameType.element:
                this.insertElement(batch, componentId, parent, childIndex, frames, frame, frameIndex);
                return 1;
            case RenderBatch_1.FrameType.text:
                this.insertText(batch, parent, childIndex, frame);
                return 1;
            case RenderBatch_1.FrameType.attribute:
                throw new Error('Attribute frames should only be present as leading children of element frames.');
            case RenderBatch_1.FrameType.component:
                this.insertComponent(batch, parent, childIndex, frame);
                return 1;
            case RenderBatch_1.FrameType.region:
                return this.insertFrameRange(batch, componentId, parent, childIndex, frames, frameIndex + 1, frameIndex + frameReader.subtreeLength(frame));
            case RenderBatch_1.FrameType.elementReferenceCapture:
                if (parent instanceof Element) {
                    ElementReferenceCapture_1.applyCaptureIdToElement(parent, frameReader.elementReferenceCaptureId(frame));
                    return 0; // A "capture" is a child in the diff, but has no node in the DOM
                }
                else {
                    throw new Error('Reference capture frames can only be children of element frames.');
                }
            case RenderBatch_1.FrameType.markup:
                this.insertMarkup(batch, parent, childIndex, frame);
                return 1;
            default:
                const unknownType = frameType; // Compile-time verification that the switch was exhaustive
                throw new Error(`Unknown frame type: ${unknownType}`);
        }
    }
    insertElement(batch, componentId, parent, childIndex, frames, frame, frameIndex) {
        const frameReader = batch.frameReader;
        const tagName = frameReader.elementName(frame);
        const newDomElementRaw = tagName === 'svg' || LogicalElements_1.isSvgElement(parent) ?
            document.createElementNS('http://www.w3.org/2000/svg', tagName) :
            document.createElement(tagName);
        const newElement = LogicalElements_1.toLogicalElement(newDomElementRaw);
        LogicalElements_1.insertLogicalChild(newDomElementRaw, parent, childIndex);
        // Apply attributes
        const descendantsEndIndexExcl = frameIndex + frameReader.subtreeLength(frame);
        for (let descendantIndex = frameIndex + 1; descendantIndex < descendantsEndIndexExcl; descendantIndex++) {
            const descendantFrame = batch.referenceFramesEntry(frames, descendantIndex);
            if (frameReader.frameType(descendantFrame) === RenderBatch_1.FrameType.attribute) {
                this.applyAttribute(batch, componentId, newDomElementRaw, descendantFrame);
            }
            else {
                // As soon as we see a non-attribute child, all the subsequent child frames are
                // not attributes, so bail out and insert the remnants recursively
                this.insertFrameRange(batch, componentId, newElement, 0, frames, descendantIndex, descendantsEndIndexExcl);
                break;
            }
        }
        // We handle setting 'value' on a <select> in two different ways:
        // [1] When inserting a corresponding <option>, in case you're dynamically adding options
        // [2] After we finish inserting the <select>, in case the descendant options are being
        //     added as an opaque markup block rather than individually
        // Right here we implement [2]
        if (newDomElementRaw instanceof HTMLSelectElement && selectValuePropname in newDomElementRaw) {
            const selectValue = newDomElementRaw[selectValuePropname];
            newDomElementRaw.value = selectValue;
            delete newDomElementRaw[selectValuePropname];
        }
    }
    insertComponent(batch, parent, childIndex, frame) {
        const containerElement = LogicalElements_1.createAndInsertLogicalContainer(parent, childIndex);
        // All we have to do is associate the child component ID with its location. We don't actually
        // do any rendering here, because the diff for the child will appear later in the render batch.
        const childComponentId = batch.frameReader.componentId(frame);
        this.attachComponentToElement(childComponentId, containerElement);
    }
    insertText(batch, parent, childIndex, textFrame) {
        const textContent = batch.frameReader.textContent(textFrame);
        const newTextNode = document.createTextNode(textContent);
        LogicalElements_1.insertLogicalChild(newTextNode, parent, childIndex);
    }
    insertMarkup(batch, parent, childIndex, markupFrame) {
        const markupContainer = LogicalElements_1.createAndInsertLogicalContainer(parent, childIndex);
        const markupContent = batch.frameReader.markupContent(markupFrame);
        const parsedMarkup = parseMarkup(markupContent, LogicalElements_1.isSvgElement(parent));
        let logicalSiblingIndex = 0;
        while (parsedMarkup.firstChild) {
            LogicalElements_1.insertLogicalChild(parsedMarkup.firstChild, markupContainer, logicalSiblingIndex++);
        }
    }
    applyAttribute(batch, componentId, toDomElement, attributeFrame) {
        const frameReader = batch.frameReader;
        const attributeName = frameReader.attributeName(attributeFrame);
        const browserRendererId = this.browserRendererId;
        const eventHandlerId = frameReader.attributeEventHandlerId(attributeFrame);
        if (eventHandlerId) {
            const firstTwoChars = attributeName.substring(0, 2);
            const eventName = attributeName.substring(2);
            if (firstTwoChars !== 'on' || !eventName) {
                throw new Error(`Attribute has nonzero event handler ID, but attribute name '${attributeName}' does not start with 'on'.`);
            }
            this.eventDelegator.setListener(toDomElement, eventName, eventHandlerId, componentId);
            return;
        }
        // First see if we have special handling for this attribute
        if (!this.tryApplySpecialProperty(batch, toDomElement, attributeName, attributeFrame)) {
            // If not, treat it as a regular string-valued attribute
            toDomElement.setAttribute(attributeName, frameReader.attributeValue(attributeFrame));
        }
    }
    tryApplySpecialProperty(batch, element, attributeName, attributeFrame) {
        switch (attributeName) {
            case 'value':
                return this.tryApplyValueProperty(batch, element, attributeFrame);
            case 'checked':
                return this.tryApplyCheckedProperty(batch, element, attributeFrame);
            default:
                return false;
        }
    }
    tryApplyValueProperty(batch, element, attributeFrame) {
        // Certain elements have built-in behaviour for their 'value' property
        const frameReader = batch.frameReader;
        switch (element.tagName) {
            case 'INPUT':
            case 'SELECT':
            case 'TEXTAREA': {
                const value = attributeFrame ? frameReader.attributeValue(attributeFrame) : null;
                element.value = value;
                if (element.tagName === 'SELECT') {
                    // <select> is special, in that anything we write to .value will be lost if there
                    // isn't yet a matching <option>. To maintain the expected behavior no matter the
                    // element insertion/update order, preserve the desired value separately so
                    // we can recover it when inserting any matching <option> or after inserting an
                    // entire markup block of descendants.
                    element[selectValuePropname] = value;
                }
                return true;
            }
            case 'OPTION': {
                const value = attributeFrame ? frameReader.attributeValue(attributeFrame) : null;
                if (value) {
                    element.setAttribute('value', value);
                }
                else {
                    element.removeAttribute('value');
                }
                // See above for why we have this special handling for <select>/<option>
                // Note that this is only one of the two cases where we set the value on a <select>
                const selectElem = this.findClosestAncestorSelectElement(element);
                if (selectElem && (selectValuePropname in selectElem) && selectElem[selectValuePropname] === value) {
                    this.tryApplyValueProperty(batch, selectElem, attributeFrame);
                    delete selectElem[selectValuePropname];
                }
                return true;
            }
            default:
                return false;
        }
    }
    tryApplyCheckedProperty(batch, element, attributeFrame) {
        // Certain elements have built-in behaviour for their 'checked' property
        if (element.tagName === 'INPUT') {
            const value = attributeFrame ? batch.frameReader.attributeValue(attributeFrame) : null;
            element.checked = value !== null;
            return true;
        }
        else {
            return false;
        }
    }
    findClosestAncestorSelectElement(element) {
        while (element) {
            if (element instanceof HTMLSelectElement) {
                return element;
            }
            else {
                element = element.parentElement;
            }
        }
        return null;
    }
    insertFrameRange(batch, componentId, parent, childIndex, frames, startIndex, endIndexExcl) {
        const origChildIndex = childIndex;
        for (let index = startIndex; index < endIndexExcl; index++) {
            const frame = batch.referenceFramesEntry(frames, index);
            const numChildrenInserted = this.insertFrame(batch, componentId, parent, childIndex, frames, frame, index);
            childIndex += numChildrenInserted;
            // Skip over any descendants, since they are already dealt with recursively
            index += countDescendantFrames(batch, frame);
        }
        return (childIndex - origChildIndex); // Total number of children inserted
    }
}
exports.BrowserRenderer = BrowserRenderer;
function parseMarkup(markup, isSvg) {
    if (isSvg) {
        sharedSvgElemForParsing.innerHTML = markup || ' ';
        return sharedSvgElemForParsing;
    }
    else {
        sharedTemplateElemForParsing.innerHTML = markup || ' ';
        return sharedTemplateElemForParsing.content;
    }
}
function countDescendantFrames(batch, frame) {
    const frameReader = batch.frameReader;
    switch (frameReader.frameType(frame)) {
        // The following frame types have a subtree length. Other frames may use that memory slot
        // to mean something else, so we must not read it. We should consider having nominal subtypes
        // of RenderTreeFramePointer that prevent access to non-applicable fields.
        case RenderBatch_1.FrameType.component:
        case RenderBatch_1.FrameType.element:
        case RenderBatch_1.FrameType.region:
            return frameReader.subtreeLength(frame) - 1;
        default:
            return 0;
    }
}
function raiseEvent(event, browserRendererId, eventHandlerId, eventArgs, eventFieldInfo) {
    if (preventDefaultEvents[event.type]) {
        event.preventDefault();
    }
    const eventDescriptor = {
        browserRendererId,
        eventHandlerId,
        eventArgsType: eventArgs.type,
        eventFieldInfo: eventFieldInfo,
    };
    RendererEventDispatcher_1.dispatchEvent(eventDescriptor, eventArgs.data);
}
function clearElement(element) {
    let childNode;
    while (childNode = element.firstChild) {
        element.removeChild(childNode);
    }
}
function clearBetween(start, end) {
    const logicalParent = LogicalElements_1.getLogicalParent(start);
    if (!logicalParent) {
        throw new Error("Can't clear between nodes. The start node does not have a logical parent.");
    }
    const children = LogicalElements_1.getLogicalChildrenArray(logicalParent);
    const removeStart = children.indexOf(start) + 1;
    const endIndex = children.indexOf(end);
    // We remove the end component comment from the DOM as we don't need it after this point.
    for (let i = removeStart; i <= endIndex; i++) {
        LogicalElements_1.removeLogicalChild(logicalParent, removeStart);
    }
    // We sanitize the start comment by removing all the information from it now that we don't need it anymore
    // as it adds noise to the DOM.
    start.textContent = '!';
}


/***/ }),

/***/ "./Rendering/ElementReferenceCapture.ts":
/*!**********************************************!*\
  !*** ./Rendering/ElementReferenceCapture.ts ***!
  \**********************************************/
/*! no static exports found */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

Object.defineProperty(exports, "__esModule", { value: true });
function applyCaptureIdToElement(element, referenceCaptureId) {
    element.setAttribute(getCaptureIdAttributeName(referenceCaptureId), '');
}
exports.applyCaptureIdToElement = applyCaptureIdToElement;
function getElementByCaptureId(referenceCaptureId) {
    const selector = `[${getCaptureIdAttributeName(referenceCaptureId)}]`;
    return document.querySelector(selector);
}
function getCaptureIdAttributeName(referenceCaptureId) {
    return `_bl_${referenceCaptureId}`;
}
// Support receiving ElementRef instances as args in interop calls
const elementRefKey = '__internalId'; // Keep in sync with ElementRef.cs
DotNet.attachReviver((key, value) => {
    if (value && typeof value === 'object' && value.hasOwnProperty(elementRefKey) && typeof value[elementRefKey] === 'string') {
        return getElementByCaptureId(value[elementRefKey]);
    }
    else {
        return value;
    }
});


/***/ }),

/***/ "./Rendering/EventDelegator.ts":
/*!*************************************!*\
  !*** ./Rendering/EventDelegator.ts ***!
  \*************************************/
/*! no static exports found */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

Object.defineProperty(exports, "__esModule", { value: true });
const EventForDotNet_1 = __webpack_require__(/*! ./EventForDotNet */ "./Rendering/EventForDotNet.ts");
const EventFieldInfo_1 = __webpack_require__(/*! ./EventFieldInfo */ "./Rendering/EventFieldInfo.ts");
const nonBubblingEvents = toLookup([
    'abort',
    'blur',
    'change',
    'error',
    'focus',
    'load',
    'loadend',
    'loadstart',
    'mouseenter',
    'mouseleave',
    'progress',
    'reset',
    'scroll',
    'submit',
    'unload',
    'DOMNodeInsertedIntoDocument',
    'DOMNodeRemovedFromDocument',
]);
// Responsible for adding/removing the eventInfo on an expando property on DOM elements, and
// calling an EventInfoStore that deals with registering/unregistering the underlying delegated
// event listeners as required (and also maps actual events back to the given callback).
class EventDelegator {
    constructor(onEvent) {
        this.onEvent = onEvent;
        const eventDelegatorId = ++EventDelegator.nextEventDelegatorId;
        this.eventsCollectionKey = `_blazorEvents_${eventDelegatorId}`;
        this.eventInfoStore = new EventInfoStore(this.onGlobalEvent.bind(this));
    }
    setListener(element, eventName, eventHandlerId, renderingComponentId) {
        // Ensure we have a place to store event info for this element
        let infoForElement = element[this.eventsCollectionKey];
        if (!infoForElement) {
            infoForElement = element[this.eventsCollectionKey] = {};
        }
        if (infoForElement.hasOwnProperty(eventName)) {
            // We can cheaply update the info on the existing object and don't need any other housekeeping
            const oldInfo = infoForElement[eventName];
            this.eventInfoStore.update(oldInfo.eventHandlerId, eventHandlerId);
        }
        else {
            // Go through the whole flow which might involve registering a new global handler
            const newInfo = { element, eventName, eventHandlerId, renderingComponentId };
            this.eventInfoStore.add(newInfo);
            infoForElement[eventName] = newInfo;
        }
    }
    removeListener(eventHandlerId) {
        // This method gets called whenever the .NET-side code reports that a certain event handler
        // has been disposed. However we will already have disposed the info about that handler if
        // the eventHandlerId for the (element,eventName) pair was replaced during diff application.
        const info = this.eventInfoStore.remove(eventHandlerId);
        if (info) {
            // Looks like this event handler wasn't already disposed
            // Remove the associated data from the DOM element
            const element = info.element;
            if (element.hasOwnProperty(this.eventsCollectionKey)) {
                const elementEventInfos = element[this.eventsCollectionKey];
                delete elementEventInfos[info.eventName];
                if (Object.getOwnPropertyNames(elementEventInfos).length === 0) {
                    delete element[this.eventsCollectionKey];
                }
            }
        }
    }
    onGlobalEvent(evt) {
        if (!(evt.target instanceof Element)) {
            return;
        }
        // Scan up the element hierarchy, looking for any matching registered event handlers
        let candidateElement = evt.target;
        let eventArgs = null; // Populate lazily
        const eventIsNonBubbling = nonBubblingEvents.hasOwnProperty(evt.type);
        while (candidateElement) {
            if (candidateElement.hasOwnProperty(this.eventsCollectionKey)) {
                const handlerInfos = candidateElement[this.eventsCollectionKey];
                if (handlerInfos.hasOwnProperty(evt.type)) {
                    // We are going to raise an event for this element, so prepare info needed by the .NET code
                    if (!eventArgs) {
                        eventArgs = EventForDotNet_1.EventForDotNet.fromDOMEvent(evt);
                    }
                    const handlerInfo = handlerInfos[evt.type];
                    const eventFieldInfo = EventFieldInfo_1.EventFieldInfo.fromEvent(handlerInfo.renderingComponentId, evt);
                    this.onEvent(evt, handlerInfo.eventHandlerId, eventArgs, eventFieldInfo);
                }
            }
            candidateElement = eventIsNonBubbling ? null : candidateElement.parentElement;
        }
    }
}
exports.EventDelegator = EventDelegator;
EventDelegator.nextEventDelegatorId = 0;
// Responsible for adding and removing the global listener when the number of listeners
// for a given event name changes between zero and nonzero
class EventInfoStore {
    constructor(globalListener) {
        this.globalListener = globalListener;
        this.infosByEventHandlerId = {};
        this.countByEventName = {};
    }
    add(info) {
        if (this.infosByEventHandlerId[info.eventHandlerId]) {
            // Should never happen, but we want to know if it does
            throw new Error(`Event ${info.eventHandlerId} is already tracked`);
        }
        this.infosByEventHandlerId[info.eventHandlerId] = info;
        const eventName = info.eventName;
        if (this.countByEventName.hasOwnProperty(eventName)) {
            this.countByEventName[eventName]++;
        }
        else {
            this.countByEventName[eventName] = 1;
            // To make delegation work with non-bubbling events, register a 'capture' listener.
            // We preserve the non-bubbling behavior by only dispatching such events to the targeted element.
            const useCapture = nonBubblingEvents.hasOwnProperty(eventName);
            document.addEventListener(eventName, this.globalListener, useCapture);
        }
    }
    update(oldEventHandlerId, newEventHandlerId) {
        if (this.infosByEventHandlerId.hasOwnProperty(newEventHandlerId)) {
            // Should never happen, but we want to know if it does
            throw new Error(`Event ${newEventHandlerId} is already tracked`);
        }
        // Since we're just updating the event handler ID, there's no need to update the global counts
        const info = this.infosByEventHandlerId[oldEventHandlerId];
        delete this.infosByEventHandlerId[oldEventHandlerId];
        info.eventHandlerId = newEventHandlerId;
        this.infosByEventHandlerId[newEventHandlerId] = info;
    }
    remove(eventHandlerId) {
        const info = this.infosByEventHandlerId[eventHandlerId];
        if (info) {
            delete this.infosByEventHandlerId[eventHandlerId];
            const eventName = info.eventName;
            if (--this.countByEventName[eventName] === 0) {
                delete this.countByEventName[eventName];
                document.removeEventListener(eventName, this.globalListener);
            }
        }
        return info;
    }
}
function toLookup(items) {
    const result = {};
    items.forEach(value => {
        result[value] = true;
    });
    return result;
}


/***/ }),

/***/ "./Rendering/EventFieldInfo.ts":
/*!*************************************!*\
  !*** ./Rendering/EventFieldInfo.ts ***!
  \*************************************/
/*! no static exports found */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

Object.defineProperty(exports, "__esModule", { value: true });
class EventFieldInfo {
    constructor(componentId, fieldValue) {
        this.componentId = componentId;
        this.fieldValue = fieldValue;
    }
    static fromEvent(componentId, event) {
        const elem = event.target;
        if (elem instanceof Element) {
            const fieldData = getFormFieldData(elem);
            if (fieldData) {
                return new EventFieldInfo(componentId, fieldData.value);
            }
        }
        // This event isn't happening on a form field that we can reverse-map back to some incoming attribute
        return null;
    }
}
exports.EventFieldInfo = EventFieldInfo;
function getFormFieldData(elem) {
    // The logic in here should be the inverse of the logic in BrowserRenderer's tryApplySpecialProperty.
    // That is, we're doing the reverse mapping, starting from an HTML property and reconstructing which
    // "special" attribute would have been mapped to that property.
    if (elem instanceof HTMLInputElement) {
        return (elem.type && elem.type.toLowerCase() === 'checkbox')
            ? { value: elem.checked }
            : { value: elem.value };
    }
    if (elem instanceof HTMLSelectElement || elem instanceof HTMLTextAreaElement) {
        return { value: elem.value };
    }
    return null;
}


/***/ }),

/***/ "./Rendering/EventForDotNet.ts":
/*!*************************************!*\
  !*** ./Rendering/EventForDotNet.ts ***!
  \*************************************/
/*! no static exports found */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

Object.defineProperty(exports, "__esModule", { value: true });
class EventForDotNet {
    constructor(type, data) {
        this.type = type;
        this.data = data;
    }
    static fromDOMEvent(event) {
        const element = event.target;
        switch (event.type) {
            case 'input':
            case 'change': {
                const targetIsCheckbox = isCheckbox(element);
                const newValue = targetIsCheckbox ? !!element['checked'] : element['value'];
                return new EventForDotNet('change', { type: event.type, value: newValue });
            }
            case 'copy':
            case 'cut':
            case 'paste':
                return new EventForDotNet('clipboard', { type: event.type });
            case 'drag':
            case 'dragend':
            case 'dragenter':
            case 'dragleave':
            case 'dragover':
            case 'dragstart':
            case 'drop':
                return new EventForDotNet('drag', parseDragEvent(event));
            case 'focus':
            case 'blur':
            case 'focusin':
            case 'focusout':
                return new EventForDotNet('focus', { type: event.type });
            case 'keydown':
            case 'keyup':
            case 'keypress':
                return new EventForDotNet('keyboard', parseKeyboardEvent(event));
            case 'contextmenu':
            case 'click':
            case 'mouseover':
            case 'mouseout':
            case 'mousemove':
            case 'mousedown':
            case 'mouseup':
            case 'dblclick':
                return new EventForDotNet('mouse', parseMouseEvent(event));
            case 'error':
                return new EventForDotNet('error', parseErrorEvent(event));
            case 'loadstart':
            case 'timeout':
            case 'abort':
            case 'load':
            case 'loadend':
            case 'progress':
                return new EventForDotNet('progress', parseProgressEvent(event));
            case 'touchcancel':
            case 'touchend':
            case 'touchmove':
            case 'touchenter':
            case 'touchleave':
            case 'touchstart':
                return new EventForDotNet('touch', parseTouchEvent(event));
            case 'gotpointercapture':
            case 'lostpointercapture':
            case 'pointercancel':
            case 'pointerdown':
            case 'pointerenter':
            case 'pointerleave':
            case 'pointermove':
            case 'pointerout':
            case 'pointerover':
            case 'pointerup':
                return new EventForDotNet('pointer', parsePointerEvent(event));
            case 'wheel':
            case 'mousewheel':
                return new EventForDotNet('wheel', parseWheelEvent(event));
            default:
                return new EventForDotNet('unknown', { type: event.type });
        }
    }
}
exports.EventForDotNet = EventForDotNet;
function parseDragEvent(event) {
    return Object.assign(Object.assign({}, parseMouseEvent(event)), { dataTransfer: event.dataTransfer });
}
function parseWheelEvent(event) {
    return Object.assign(Object.assign({}, parseMouseEvent(event)), { deltaX: event.deltaX, deltaY: event.deltaY, deltaZ: event.deltaZ, deltaMode: event.deltaMode });
}
function parseErrorEvent(event) {
    return {
        type: event.type,
        message: event.message,
        filename: event.filename,
        lineno: event.lineno,
        colno: event.colno,
    };
}
function parseProgressEvent(event) {
    return {
        type: event.type,
        lengthComputable: event.lengthComputable,
        loaded: event.loaded,
        total: event.total,
    };
}
function parseTouchEvent(event) {
    function parseTouch(touchList) {
        const touches = [];
        for (let i = 0; i < touchList.length; i++) {
            const touch = touchList[i];
            touches.push({
                identifier: touch.identifier,
                clientX: touch.clientX,
                clientY: touch.clientY,
                screenX: touch.screenX,
                screenY: touch.screenY,
                pageX: touch.pageX,
                pageY: touch.pageY,
            });
        }
        return touches;
    }
    return {
        type: event.type,
        detail: event.detail,
        touches: parseTouch(event.touches),
        targetTouches: parseTouch(event.targetTouches),
        changedTouches: parseTouch(event.changedTouches),
        ctrlKey: event.ctrlKey,
        shiftKey: event.shiftKey,
        altKey: event.altKey,
        metaKey: event.metaKey,
    };
}
function parseKeyboardEvent(event) {
    return {
        type: event.type,
        key: event.key,
        code: event.code,
        location: event.location,
        repeat: event.repeat,
        ctrlKey: event.ctrlKey,
        shiftKey: event.shiftKey,
        altKey: event.altKey,
        metaKey: event.metaKey,
    };
}
function parsePointerEvent(event) {
    return Object.assign(Object.assign({}, parseMouseEvent(event)), { pointerId: event.pointerId, width: event.width, height: event.height, pressure: event.pressure, tiltX: event.tiltX, tiltY: event.tiltY, pointerType: event.pointerType, isPrimary: event.isPrimary });
}
function parseMouseEvent(event) {
    return {
        type: event.type,
        detail: event.detail,
        screenX: event.screenX,
        screenY: event.screenY,
        clientX: event.clientX,
        clientY: event.clientY,
        button: event.button,
        buttons: event.buttons,
        ctrlKey: event.ctrlKey,
        shiftKey: event.shiftKey,
        altKey: event.altKey,
        metaKey: event.metaKey,
    };
}
function isCheckbox(element) {
    return element && element.tagName === 'INPUT' && element.getAttribute('type') === 'checkbox';
}


/***/ }),

/***/ "./Rendering/LogicalElements.ts":
/*!**************************************!*\
  !*** ./Rendering/LogicalElements.ts ***!
  \**************************************/
/*! no static exports found */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

/*
  A LogicalElement plays the same role as an Element instance from the point of view of the
  API consumer. Inserting and removing logical elements updates the browser DOM just the same.

  The difference is that, unlike regular DOM mutation APIs, the LogicalElement APIs don't use
  the underlying DOM structure as the data storage for the element hierarchy. Instead, the
  LogicalElement APIs take care of tracking hierarchical relationships separately. The point
  of this is to permit a logical tree structure in which parent/child relationships don't
  have to be materialized in terms of DOM element parent/child relationships. And the reason
  why we want that is so that hierarchies of Blazor components can be tracked even when those
  components' render output need not be a single literal DOM element.

  Consumers of the API don't need to know about the implementation, but how it's done is:
  - Each LogicalElement is materialized in the DOM as either:
    - A Node instance, for actual Node instances inserted using 'insertLogicalChild' or
      for Element instances promoted to LogicalElement via 'toLogicalElement'
    - A Comment instance, for 'logical container' instances inserted using 'createAndInsertLogicalContainer'
  - Then, on that instance (i.e., the Node or Comment), we store an array of 'logical children'
    instances, e.g.,
      [firstChild, secondChild, thirdChild, ...]
    ... plus we store a reference to the 'logical parent' (if any)
  - The 'logical children' array means we can look up in O(1):
    - The number of logical children (not currently implemented because not required, but trivial)
    - The logical child at any given index
  - Whenever a logical child is added or removed, we update the parent's array of logical children
*/
Object.defineProperty(exports, "__esModule", { value: true });
const logicalChildrenPropname = createSymbolOrFallback('_blazorLogicalChildren');
const logicalParentPropname = createSymbolOrFallback('_blazorLogicalParent');
const logicalEndSiblingPropname = createSymbolOrFallback('_blazorLogicalEnd');
function toLogicalRootCommentElement(start, end) {
    // Now that we support start/end comments as component delimiters we are going to be setting up
    // adding the components rendered output as siblings of the start/end tags (between).
    // For that to work, we need to appropriately configure the parent element to be a logical element
    // with all their children being the child elements.
    // For example, imagine you have
    // <app>
    // <div><p>Static content</p></div>
    // <!-- start component
    // <!-- end component
    // <footer>Some other content</footer>
    // <app>
    // We want the parent element to be something like
    // *app
    // |- *div
    // |- *component
    // |- *footer
    if (!start.parentNode) {
        throw new Error(`Comment not connected to the DOM ${start.textContent}`);
    }
    const parent = start.parentNode;
    const parentLogicalElement = toLogicalElement(parent, /* allow existing contents */ true);
    const children = getLogicalChildrenArray(parentLogicalElement);
    Array.from(parent.childNodes).forEach(n => children.push(n));
    start[logicalParentPropname] = parentLogicalElement;
    // We might not have an end comment in the case of non-prerendered components.
    if (end) {
        start[logicalEndSiblingPropname] = end;
        toLogicalElement(end, /* allowExistingcontents */ true);
    }
    return toLogicalElement(start, /* allowExistingContents */ true);
}
exports.toLogicalRootCommentElement = toLogicalRootCommentElement;
function toLogicalElement(element, allowExistingContents) {
    // Normally it's good to assert that the element has started empty, because that's the usual
    // situation and we probably have a bug if it's not. But for the element that contain prerendered
    // root components, we want to let them keep their content until we replace it.
    if (element.childNodes.length > 0 && !allowExistingContents) {
        throw new Error('New logical elements must start empty, or allowExistingContents must be true');
    }
    element[logicalChildrenPropname] = [];
    return element;
}
exports.toLogicalElement = toLogicalElement;
function createAndInsertLogicalContainer(parent, childIndex) {
    const containerElement = document.createComment('!');
    insertLogicalChild(containerElement, parent, childIndex);
    return containerElement;
}
exports.createAndInsertLogicalContainer = createAndInsertLogicalContainer;
function insertLogicalChild(child, parent, childIndex) {
    const childAsLogicalElement = child;
    if (child instanceof Comment) {
        const existingGrandchildren = getLogicalChildrenArray(childAsLogicalElement);
        if (existingGrandchildren && getLogicalChildrenArray(childAsLogicalElement).length > 0) {
            // There's nothing to stop us implementing support for this scenario, and it's not difficult
            // (after inserting 'child' itself, also iterate through its logical children and physically
            // put them as following-siblings in the DOM). However there's no scenario that requires it
            // presently, so if we did implement it there'd be no good way to have tests for it.
            throw new Error('Not implemented: inserting non-empty logical container');
        }
    }
    if (getLogicalParent(childAsLogicalElement)) {
        // Likewise, we could easily support this scenario too (in this 'if' block, just splice
        // out 'child' from the logical children array of its previous logical parent by using
        // Array.prototype.indexOf to determine its previous sibling index).
        // But again, since there's not currently any scenario that would use it, we would not
        // have any test coverage for such an implementation.
        throw new Error('Not implemented: moving existing logical children');
    }
    const newSiblings = getLogicalChildrenArray(parent);
    if (childIndex < newSiblings.length) {
        // Insert
        const nextSibling = newSiblings[childIndex];
        nextSibling.parentNode.insertBefore(child, nextSibling);
        newSiblings.splice(childIndex, 0, childAsLogicalElement);
    }
    else {
        // Append
        appendDomNode(child, parent);
        newSiblings.push(childAsLogicalElement);
    }
    childAsLogicalElement[logicalParentPropname] = parent;
    if (!(logicalChildrenPropname in childAsLogicalElement)) {
        childAsLogicalElement[logicalChildrenPropname] = [];
    }
}
exports.insertLogicalChild = insertLogicalChild;
function removeLogicalChild(parent, childIndex) {
    const childrenArray = getLogicalChildrenArray(parent);
    const childToRemove = childrenArray.splice(childIndex, 1)[0];
    // If it's a logical container, also remove its descendants
    if (childToRemove instanceof Comment) {
        const grandchildrenArray = getLogicalChildrenArray(childToRemove);
        while (grandchildrenArray.length > 0) {
            removeLogicalChild(childToRemove, 0);
        }
    }
    // Finally, remove the node itself
    const domNodeToRemove = childToRemove;
    domNodeToRemove.parentNode.removeChild(domNodeToRemove);
}
exports.removeLogicalChild = removeLogicalChild;
function getLogicalParent(element) {
    return element[logicalParentPropname] || null;
}
exports.getLogicalParent = getLogicalParent;
function getLogicalSiblingEnd(element) {
    return element[logicalEndSiblingPropname] || null;
}
exports.getLogicalSiblingEnd = getLogicalSiblingEnd;
function getLogicalChild(parent, childIndex) {
    return getLogicalChildrenArray(parent)[childIndex];
}
exports.getLogicalChild = getLogicalChild;
function isSvgElement(element) {
    return getClosestDomElement(element).namespaceURI === 'http://www.w3.org/2000/svg';
}
exports.isSvgElement = isSvgElement;
function getLogicalChildrenArray(element) {
    return element[logicalChildrenPropname];
}
exports.getLogicalChildrenArray = getLogicalChildrenArray;
function permuteLogicalChildren(parent, permutationList) {
    // The permutationList must represent a valid permutation, i.e., the list of 'from' indices
    // is distinct, and the list of 'to' indices is a permutation of it. The algorithm here
    // relies on that assumption.
    // Each of the phases here has to happen separately, because each one is designed not to
    // interfere with the indices or DOM entries used by subsequent phases.
    // Phase 1: track which nodes we will move
    const siblings = getLogicalChildrenArray(parent);
    permutationList.forEach((listEntry) => {
        listEntry.moveRangeStart = siblings[listEntry.fromSiblingIndex];
        listEntry.moveRangeEnd = findLastDomNodeInRange(listEntry.moveRangeStart);
    });
    // Phase 2: insert markers
    permutationList.forEach((listEntry) => {
        const marker = listEntry.moveToBeforeMarker = document.createComment('marker');
        const insertBeforeNode = siblings[listEntry.toSiblingIndex + 1];
        if (insertBeforeNode) {
            insertBeforeNode.parentNode.insertBefore(marker, insertBeforeNode);
        }
        else {
            appendDomNode(marker, parent);
        }
    });
    // Phase 3: move descendants & remove markers
    permutationList.forEach((listEntry) => {
        const insertBefore = listEntry.moveToBeforeMarker;
        const parentDomNode = insertBefore.parentNode;
        const elementToMove = listEntry.moveRangeStart;
        const moveEndNode = listEntry.moveRangeEnd;
        let nextToMove = elementToMove;
        while (nextToMove) {
            const nextNext = nextToMove.nextSibling;
            parentDomNode.insertBefore(nextToMove, insertBefore);
            if (nextToMove === moveEndNode) {
                break;
            }
            else {
                nextToMove = nextNext;
            }
        }
        parentDomNode.removeChild(insertBefore);
    });
    // Phase 4: update siblings index
    permutationList.forEach((listEntry) => {
        siblings[listEntry.toSiblingIndex] = listEntry.moveRangeStart;
    });
}
exports.permuteLogicalChildren = permuteLogicalChildren;
function getClosestDomElement(logicalElement) {
    if (logicalElement instanceof Element) {
        return logicalElement;
    }
    else if (logicalElement instanceof Comment) {
        return logicalElement.parentNode;
    }
    else {
        throw new Error('Not a valid logical element');
    }
}
exports.getClosestDomElement = getClosestDomElement;
function getLogicalNextSibling(element) {
    const siblings = getLogicalChildrenArray(getLogicalParent(element));
    const siblingIndex = Array.prototype.indexOf.call(siblings, element);
    return siblings[siblingIndex + 1] || null;
}
function appendDomNode(child, parent) {
    // This function only puts 'child' into the DOM in the right place relative to 'parent'
    // It does not update the logical children array of anything
    if (parent instanceof Element) {
        parent.appendChild(child);
    }
    else if (parent instanceof Comment) {
        const parentLogicalNextSibling = getLogicalNextSibling(parent);
        if (parentLogicalNextSibling) {
            // Since the parent has a logical next-sibling, its appended child goes right before that
            parentLogicalNextSibling.parentNode.insertBefore(child, parentLogicalNextSibling);
        }
        else {
            // Since the parent has no logical next-sibling, keep recursing upwards until we find
            // a logical ancestor that does have a next-sibling or is a physical element.
            appendDomNode(child, getLogicalParent(parent));
        }
    }
    else {
        // Should never happen
        throw new Error(`Cannot append node because the parent is not a valid logical element. Parent: ${parent}`);
    }
}
// Returns the final node (in depth-first evaluation order) that is a descendant of the logical element.
// As such, the entire subtree is between 'element' and 'findLastDomNodeInRange(element)' inclusive.
function findLastDomNodeInRange(element) {
    if (element instanceof Element) {
        return element;
    }
    const nextSibling = getLogicalNextSibling(element);
    if (nextSibling) {
        // Simple case: not the last logical sibling, so take the node before the next sibling
        return nextSibling.previousSibling;
    }
    else {
        // Harder case: there's no logical next-sibling, so recurse upwards until we find
        // a logical ancestor that does have one, or a physical element
        const logicalParent = getLogicalParent(element);
        return logicalParent instanceof Element
            ? logicalParent.lastChild
            : findLastDomNodeInRange(logicalParent);
    }
}
function createSymbolOrFallback(fallback) {
    return typeof Symbol === 'function' ? Symbol() : fallback;
}


/***/ }),

/***/ "./Rendering/RenderBatch/OutOfProcessRenderBatch.ts":
/*!**********************************************************!*\
  !*** ./Rendering/RenderBatch/OutOfProcessRenderBatch.ts ***!
  \**********************************************************/
/*! no static exports found */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

Object.defineProperty(exports, "__esModule", { value: true });
const Utf8Decoder_1 = __webpack_require__(/*! ./Utf8Decoder */ "./Rendering/RenderBatch/Utf8Decoder.ts");
const updatedComponentsEntryLength = 4; // Each is a single int32 giving the location of the data
const referenceFramesEntryLength = 20; // 1 int for frame type, then 16 bytes for type-specific data
const disposedComponentIdsEntryLength = 4; // Each is an int32 giving the ID
const disposedEventHandlerIdsEntryLength = 8; // Each is an int64 giving the ID
const editsEntryLength = 16; // 4 ints
const stringTableEntryLength = 4; // Each is an int32 giving the string data location, or -1 for null
const uint64HighPartShift = Math.pow(2, 32);
const maxSafeNumberHighPart = Math.pow(2, 21) - 1; // The high-order int32 from Number.MAX_SAFE_INTEGER
class OutOfProcessRenderBatch {
    constructor(batchData) {
        this.batchData = batchData;
        const stringReader = new OutOfProcessStringReader(batchData);
        this.arrayRangeReader = new OutOfProcessArrayRangeReader(batchData);
        this.arrayBuilderSegmentReader = new OutOfProcessArrayBuilderSegmentReader(batchData);
        this.diffReader = new OutOfProcessRenderTreeDiffReader(batchData);
        this.editReader = new OutOfProcessRenderTreeEditReader(batchData, stringReader);
        this.frameReader = new OutOfProcessRenderTreeFrameReader(batchData, stringReader);
    }
    updatedComponents() {
        return readInt32LE(this.batchData, this.batchData.length - 20); // 5th-from-last int32
    }
    referenceFrames() {
        return readInt32LE(this.batchData, this.batchData.length - 16); // 4th-from-last int32
    }
    disposedComponentIds() {
        return readInt32LE(this.batchData, this.batchData.length - 12); // 3rd-from-last int32
    }
    disposedEventHandlerIds() {
        return readInt32LE(this.batchData, this.batchData.length - 8); // 2th-from-last int32
    }
    updatedComponentsEntry(values, index) {
        const tableEntryPos = values + index * updatedComponentsEntryLength;
        return readInt32LE(this.batchData, tableEntryPos);
    }
    referenceFramesEntry(values, index) {
        return values + index * referenceFramesEntryLength;
    }
    disposedComponentIdsEntry(values, index) {
        const entryPos = values + index * disposedComponentIdsEntryLength;
        return readInt32LE(this.batchData, entryPos);
    }
    disposedEventHandlerIdsEntry(values, index) {
        const entryPos = values + index * disposedEventHandlerIdsEntryLength;
        return readUint64LE(this.batchData, entryPos);
    }
}
exports.OutOfProcessRenderBatch = OutOfProcessRenderBatch;
class OutOfProcessRenderTreeDiffReader {
    constructor(batchDataUint8) {
        this.batchDataUint8 = batchDataUint8;
    }
    componentId(diff) {
        // First int32 is componentId
        return readInt32LE(this.batchDataUint8, diff);
    }
    edits(diff) {
        // Entries data starts after the componentId (which is a 4-byte int)
        return (diff + 4);
    }
    editsEntry(values, index) {
        return values + index * editsEntryLength;
    }
}
class OutOfProcessRenderTreeEditReader {
    constructor(batchDataUint8, stringReader) {
        this.batchDataUint8 = batchDataUint8;
        this.stringReader = stringReader;
    }
    editType(edit) {
        return readInt32LE(this.batchDataUint8, edit); // 1st int
    }
    siblingIndex(edit) {
        return readInt32LE(this.batchDataUint8, edit + 4); // 2nd int
    }
    newTreeIndex(edit) {
        return readInt32LE(this.batchDataUint8, edit + 8); // 3rd int
    }
    moveToSiblingIndex(edit) {
        return readInt32LE(this.batchDataUint8, edit + 8); // 3rd int
    }
    removedAttributeName(edit) {
        const stringIndex = readInt32LE(this.batchDataUint8, edit + 12); // 4th int
        return this.stringReader.readString(stringIndex);
    }
}
class OutOfProcessRenderTreeFrameReader {
    constructor(batchDataUint8, stringReader) {
        this.batchDataUint8 = batchDataUint8;
        this.stringReader = stringReader;
    }
    // For render frames, the 2nd-4th ints have different meanings depending on frameType.
    // It's the caller's responsibility not to evaluate properties that aren't applicable to the frameType.
    frameType(frame) {
        return readInt32LE(this.batchDataUint8, frame); // 1st int
    }
    subtreeLength(frame) {
        return readInt32LE(this.batchDataUint8, frame + 4); // 2nd int
    }
    elementReferenceCaptureId(frame) {
        const stringIndex = readInt32LE(this.batchDataUint8, frame + 4); // 2nd int
        return this.stringReader.readString(stringIndex);
    }
    componentId(frame) {
        return readInt32LE(this.batchDataUint8, frame + 8); // 3rd int
    }
    elementName(frame) {
        const stringIndex = readInt32LE(this.batchDataUint8, frame + 8); // 3rd int
        return this.stringReader.readString(stringIndex);
    }
    textContent(frame) {
        const stringIndex = readInt32LE(this.batchDataUint8, frame + 4); // 2nd int
        return this.stringReader.readString(stringIndex);
    }
    markupContent(frame) {
        const stringIndex = readInt32LE(this.batchDataUint8, frame + 4); // 2nd int
        return this.stringReader.readString(stringIndex);
    }
    attributeName(frame) {
        const stringIndex = readInt32LE(this.batchDataUint8, frame + 4); // 2nd int
        return this.stringReader.readString(stringIndex);
    }
    attributeValue(frame) {
        const stringIndex = readInt32LE(this.batchDataUint8, frame + 8); // 3rd int
        return this.stringReader.readString(stringIndex);
    }
    attributeEventHandlerId(frame) {
        return readUint64LE(this.batchDataUint8, frame + 12); // Bytes 12-19
    }
}
class OutOfProcessStringReader {
    constructor(batchDataUint8) {
        this.batchDataUint8 = batchDataUint8;
        // Final int gives start position of the string table
        this.stringTableStartIndex = readInt32LE(batchDataUint8, batchDataUint8.length - 4);
    }
    readString(index) {
        if (index === -1) { // Special value encodes 'null'
            return null;
        }
        else {
            const stringTableEntryPos = readInt32LE(this.batchDataUint8, this.stringTableStartIndex + index * stringTableEntryLength);
            // By default, .NET's BinaryWriter gives LEB128-length-prefixed UTF-8 data.
            // This is convenient enough to decode in JavaScript.
            const numUtf8Bytes = readLEB128(this.batchDataUint8, stringTableEntryPos);
            const charsStart = stringTableEntryPos + numLEB128Bytes(numUtf8Bytes);
            const utf8Data = new Uint8Array(this.batchDataUint8.buffer, this.batchDataUint8.byteOffset + charsStart, numUtf8Bytes);
            return Utf8Decoder_1.decodeUtf8(utf8Data);
        }
    }
}
class OutOfProcessArrayRangeReader {
    constructor(batchDataUint8) {
        this.batchDataUint8 = batchDataUint8;
    }
    count(arrayRange) {
        // First int is count
        return readInt32LE(this.batchDataUint8, arrayRange);
    }
    values(arrayRange) {
        // Entries data starts after the 'count' int (i.e., after 4 bytes)
        return arrayRange + 4;
    }
}
class OutOfProcessArrayBuilderSegmentReader {
    constructor(batchDataUint8) {
        this.batchDataUint8 = batchDataUint8;
    }
    offset(arrayBuilderSegment) {
        // Not used by the out-of-process representation of RenderBatch data.
        // This only exists on the ArrayBuilderSegmentReader for the shared-memory representation.
        return 0;
    }
    count(arrayBuilderSegment) {
        // First int is count
        return readInt32LE(this.batchDataUint8, arrayBuilderSegment);
    }
    values(arrayBuilderSegment) {
        // Entries data starts after the 'count' int (i.e., after 4 bytes)
        return arrayBuilderSegment + 4;
    }
}
function readInt32LE(buffer, position) {
    return (buffer[position])
        | (buffer[position + 1] << 8)
        | (buffer[position + 2] << 16)
        | (buffer[position + 3] << 24);
}
function readUint32LE(buffer, position) {
    return (buffer[position])
        + (buffer[position + 1] << 8)
        + (buffer[position + 2] << 16)
        + ((buffer[position + 3] << 24) >>> 0); // The >>> 0 coerces the value to unsigned
}
function readUint64LE(buffer, position) {
    // This cannot be done using bit-shift operators in JavaScript, because
    // those all implicitly convert to int32
    const highPart = readUint32LE(buffer, position + 4);
    if (highPart > maxSafeNumberHighPart) {
        throw new Error(`Cannot read uint64 with high order part ${highPart}, because the result would exceed Number.MAX_SAFE_INTEGER.`);
    }
    return (highPart * uint64HighPartShift) + readUint32LE(buffer, position);
}
function readLEB128(buffer, position) {
    let result = 0;
    let shift = 0;
    for (let index = 0; index < 4; index++) {
        const byte = buffer[position + index];
        result |= (byte & 127) << shift;
        if (byte < 128) {
            break;
        }
        shift += 7;
    }
    return result;
}
function numLEB128Bytes(value) {
    return value < 128 ? 1
        : value < 16384 ? 2
            : value < 2097152 ? 3 : 4;
}


/***/ }),

/***/ "./Rendering/RenderBatch/RenderBatch.ts":
/*!**********************************************!*\
  !*** ./Rendering/RenderBatch/RenderBatch.ts ***!
  \**********************************************/
/*! no static exports found */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

Object.defineProperty(exports, "__esModule", { value: true });
var EditType;
(function (EditType) {
    // The values must be kept in sync with the .NET equivalent in RenderTreeEditType.cs
    EditType[EditType["prependFrame"] = 1] = "prependFrame";
    EditType[EditType["removeFrame"] = 2] = "removeFrame";
    EditType[EditType["setAttribute"] = 3] = "setAttribute";
    EditType[EditType["removeAttribute"] = 4] = "removeAttribute";
    EditType[EditType["updateText"] = 5] = "updateText";
    EditType[EditType["stepIn"] = 6] = "stepIn";
    EditType[EditType["stepOut"] = 7] = "stepOut";
    EditType[EditType["updateMarkup"] = 8] = "updateMarkup";
    EditType[EditType["permutationListEntry"] = 9] = "permutationListEntry";
    EditType[EditType["permutationListEnd"] = 10] = "permutationListEnd";
})(EditType = exports.EditType || (exports.EditType = {}));
var FrameType;
(function (FrameType) {
    // The values must be kept in sync with the .NET equivalent in RenderTreeFrameType.cs
    FrameType[FrameType["element"] = 1] = "element";
    FrameType[FrameType["text"] = 2] = "text";
    FrameType[FrameType["attribute"] = 3] = "attribute";
    FrameType[FrameType["component"] = 4] = "component";
    FrameType[FrameType["region"] = 5] = "region";
    FrameType[FrameType["elementReferenceCapture"] = 6] = "elementReferenceCapture";
    FrameType[FrameType["markup"] = 8] = "markup";
})(FrameType = exports.FrameType || (exports.FrameType = {}));


/***/ }),

/***/ "./Rendering/RenderBatch/Utf8Decoder.ts":
/*!**********************************************!*\
  !*** ./Rendering/RenderBatch/Utf8Decoder.ts ***!
  \**********************************************/
/*! no static exports found */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

Object.defineProperty(exports, "__esModule", { value: true });
const nativeDecoder = typeof TextDecoder === 'function'
    ? new TextDecoder('utf-8')
    : null;
/*export const decodeUtf8: (bytes: Uint8Array) => string
  = nativeDecoder ? nativeDecoder.decode.bind(nativeDecoder) : decodeImpl;*/
exports.decodeUtf8 = decodeImpl;
/* !
Logic in decodeImpl is derived from fast-text-encoding
https://github.com/samthor/fast-text-encoding

License for fast-text-encoding: Apache 2.0
https://github.com/samthor/fast-text-encoding/blob/master/LICENSE
*/
function decodeImpl(bytes) {
    let pos = 0;
    const len = bytes.length;
    const out = [];
    const substrings = [];
    while (pos < len) {
        const byte1 = bytes[pos++];
        if (byte1 === 0) {
            break; // NULL
        }
        if ((byte1 & 0x80) === 0) { // 1-byte
            out.push(byte1);
        }
        else if ((byte1 & 0xe0) === 0xc0) { // 2-byte
            const byte2 = bytes[pos++] & 0x3f;
            out.push(((byte1 & 0x1f) << 6) | byte2);
        }
        else if ((byte1 & 0xf0) === 0xe0) {
            const byte2 = bytes[pos++] & 0x3f;
            const byte3 = bytes[pos++] & 0x3f;
            out.push(((byte1 & 0x1f) << 12) | (byte2 << 6) | byte3);
        }
        else if ((byte1 & 0xf8) === 0xf0) {
            const byte2 = bytes[pos++] & 0x3f;
            const byte3 = bytes[pos++] & 0x3f;
            const byte4 = bytes[pos++] & 0x3f;
            // this can be > 0xffff, so possibly generate surrogates
            let codepoint = ((byte1 & 0x07) << 0x12) | (byte2 << 0x0c) | (byte3 << 0x06) | byte4;
            if (codepoint > 0xffff) {
                // codepoint &= ~0x10000;
                codepoint -= 0x10000;
                out.push((codepoint >>> 10) & 0x3ff | 0xd800);
                codepoint = 0xdc00 | codepoint & 0x3ff;
            }
            out.push(codepoint);
        }
        else {
            // FIXME: we're ignoring this
        }
        // As a workaround for https://github.com/samthor/fast-text-encoding/issues/1,
        // make sure the 'out' array never gets too long. When it reaches a limit, we
        // stringify what we have so far and append to a list of outputs.
        if (out.length > 1024) {
            substrings.push(String.fromCharCode.apply(null, out));
            out.length = 0;
        }
    }
    substrings.push(String.fromCharCode.apply(null, out));
    return substrings.join('');
}


/***/ }),

/***/ "./Rendering/Renderer.ts":
/*!*******************************!*\
  !*** ./Rendering/Renderer.ts ***!
  \*******************************/
/*! no static exports found */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

Object.defineProperty(exports, "__esModule", { value: true });
/* eslint-disable @typescript-eslint/camelcase */
__webpack_require__(/*! ../Platform/Platform */ "./Platform/Platform.ts");
__webpack_require__(/*! ../Environment */ "./Environment.ts");
const BrowserRenderer_1 = __webpack_require__(/*! ./BrowserRenderer */ "./Rendering/BrowserRenderer.ts");
const LogicalElements_1 = __webpack_require__(/*! ./LogicalElements */ "./Rendering/LogicalElements.ts");
const browserRenderers = {};
let shouldResetScrollAfterNextBatch = false;
function attachRootComponentToLogicalElement(browserRendererId, logicalElement, componentId) {
    let browserRenderer = browserRenderers[browserRendererId];
    if (!browserRenderer) {
        browserRenderer = browserRenderers[browserRendererId] = new BrowserRenderer_1.BrowserRenderer(browserRendererId);
    }
    browserRenderer.attachRootComponentToLogicalElement(componentId, logicalElement);
}
exports.attachRootComponentToLogicalElement = attachRootComponentToLogicalElement;
function attachRootComponentToElement(elementSelector, componentId, browserRendererId) {
    const element = document.querySelector(elementSelector);
    if (!element) {
        throw new Error(`Could not find any element matching selector '${elementSelector}'.`);
    }
    // 'allowExistingContents' to keep any prerendered content until we do the first client-side render
    // Only client-side Blazor supplies a browser renderer ID
    attachRootComponentToLogicalElement(browserRendererId || 0, LogicalElements_1.toLogicalElement(element, /* allow existing contents */ true), componentId);
}
exports.attachRootComponentToElement = attachRootComponentToElement;
function renderBatch(browserRendererId, batch) {
    const browserRenderer = browserRenderers[browserRendererId];
    if (!browserRenderer) {
        throw new Error(`There is no browser renderer with ID ${browserRendererId}.`);
    }
    const arrayRangeReader = batch.arrayRangeReader;
    const updatedComponentsRange = batch.updatedComponents();
    const updatedComponentsValues = arrayRangeReader.values(updatedComponentsRange);
    const updatedComponentsLength = arrayRangeReader.count(updatedComponentsRange);
    const referenceFrames = batch.referenceFrames();
    const referenceFramesValues = arrayRangeReader.values(referenceFrames);
    const diffReader = batch.diffReader;
    for (let i = 0; i < updatedComponentsLength; i++) {
        const diff = batch.updatedComponentsEntry(updatedComponentsValues, i);
        const componentId = diffReader.componentId(diff);
        const edits = diffReader.edits(diff);
        browserRenderer.updateComponent(batch, componentId, edits, referenceFramesValues);
    }
    const disposedComponentIdsRange = batch.disposedComponentIds();
    const disposedComponentIdsValues = arrayRangeReader.values(disposedComponentIdsRange);
    const disposedComponentIdsLength = arrayRangeReader.count(disposedComponentIdsRange);
    for (let i = 0; i < disposedComponentIdsLength; i++) {
        const componentId = batch.disposedComponentIdsEntry(disposedComponentIdsValues, i);
        browserRenderer.disposeComponent(componentId);
    }
    const disposedEventHandlerIdsRange = batch.disposedEventHandlerIds();
    const disposedEventHandlerIdsValues = arrayRangeReader.values(disposedEventHandlerIdsRange);
    const disposedEventHandlerIdsLength = arrayRangeReader.count(disposedEventHandlerIdsRange);
    for (let i = 0; i < disposedEventHandlerIdsLength; i++) {
        const eventHandlerId = batch.disposedEventHandlerIdsEntry(disposedEventHandlerIdsValues, i);
        browserRenderer.disposeEventHandler(eventHandlerId);
    }
    resetScrollIfNeeded();
}
exports.renderBatch = renderBatch;
function resetScrollAfterNextBatch() {
    shouldResetScrollAfterNextBatch = true;
}
exports.resetScrollAfterNextBatch = resetScrollAfterNextBatch;
function resetScrollIfNeeded() {
    if (shouldResetScrollAfterNextBatch) {
        shouldResetScrollAfterNextBatch = false;
        // This assumes the scroller is on the window itself. There isn't a general way to know
        // if some other element is playing the role of the primary scroll region.
        window.scrollTo && window.scrollTo(0, 0);
    }
}


/***/ }),

/***/ "./Rendering/RendererEventDispatcher.ts":
/*!**********************************************!*\
  !*** ./Rendering/RendererEventDispatcher.ts ***!
  \**********************************************/
/*! no static exports found */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

Object.defineProperty(exports, "__esModule", { value: true });
let eventDispatcherInstance;
function dispatchEvent(eventDescriptor, eventArgs) {
    if (!eventDispatcherInstance) {
        throw new Error('eventDispatcher not initialized. Call \'setEventDispatcher\' to configure it.');
    }
    return eventDispatcherInstance(eventDescriptor, eventArgs);
}
exports.dispatchEvent = dispatchEvent;
function setEventDispatcher(newDispatcher) {
    eventDispatcherInstance = newDispatcher;
}
exports.setEventDispatcher = setEventDispatcher;


/***/ }),

/***/ "./Services/Http.ts":
/*!**************************!*\
  !*** ./Services/Http.ts ***!
  \**************************/
/*! no static exports found */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
Object.defineProperty(exports, "__esModule", { value: true });
const Environment_1 = __webpack_require__(/*! ../Environment */ "./Environment.ts");
const httpClientAssembly = 'Microsoft.AspNetCore.Blazor';
const httpClientNamespace = `${httpClientAssembly}.Http`;
const httpClientTypeName = 'WebAssemblyHttpMessageHandler';
let receiveResponseMethod;
let allocateArrayMethod;
// These are the functions we're making available for invocation from .NET
exports.internalFunctions = {
    sendAsync,
};
function sendAsync(id, body, jsonFetchArgs) {
    return __awaiter(this, void 0, void 0, function* () {
        let response;
        let responseData;
        const fetchOptions = JSON.parse(Environment_1.platform.toJavaScriptString(jsonFetchArgs));
        const requestInit = Object.assign(convertToRequestInit(fetchOptions.requestInit), fetchOptions.requestInitOverrides);
        if (body) {
            requestInit.body = Environment_1.platform.toUint8Array(body);
        }
        try {
            response = yield fetch(fetchOptions.requestUri, requestInit);
            responseData = yield response.arrayBuffer();
        }
        catch (ex) {
            dispatchErrorResponse(id, ex.toString());
            return;
        }
        dispatchSuccessResponse(id, response, responseData);
    });
}
function convertToRequestInit(blazorRequestInit) {
    return {
        credentials: blazorRequestInit.credentials,
        method: blazorRequestInit.method,
        headers: blazorRequestInit.headers.map(item => [item.name, item.value])
    };
}
function dispatchSuccessResponse(id, response, responseData) {
    const responseDescriptor = {
        statusCode: response.status,
        statusText: response.statusText,
        headers: [],
    };
    response.headers.forEach((value, name) => {
        responseDescriptor.headers.push({ name: name, value: value });
    });
    if (!allocateArrayMethod) {
        allocateArrayMethod = Environment_1.platform.findMethod(httpClientAssembly, httpClientNamespace, httpClientTypeName, 'AllocateArray');
    }
    // allocate a managed byte[] of the right size
    const dotNetArray = Environment_1.platform.callMethod(allocateArrayMethod, null, [Environment_1.platform.toDotNetString(responseData.byteLength.toString())]);
    // get an Uint8Array view of it
    const array = Environment_1.platform.toUint8Array(dotNetArray);
    // copy the responseData to our managed byte[]
    array.set(new Uint8Array(responseData));
    dispatchResponse(id, Environment_1.platform.toDotNetString(JSON.stringify(responseDescriptor)), dotNetArray, 
    /* errorMessage */ null);
}
function dispatchErrorResponse(id, errorMessage) {
    dispatchResponse(id, 
    /* responseDescriptor */ null, 
    /* responseText */ null, Environment_1.platform.toDotNetString(errorMessage));
}
function dispatchResponse(id, responseDescriptor, responseData, errorMessage) {
    if (!receiveResponseMethod) {
        receiveResponseMethod = Environment_1.platform.findMethod(httpClientAssembly, httpClientNamespace, httpClientTypeName, 'ReceiveResponse');
    }
    Environment_1.platform.callMethod(receiveResponseMethod, null, [
        Environment_1.platform.toDotNetString(id.toString()),
        responseDescriptor,
        responseData,
        errorMessage,
    ]);
}


/***/ }),

/***/ "./Services/NavigationManager.ts":
/*!***************************************!*\
  !*** ./Services/NavigationManager.ts ***!
  \***************************************/
/*! no static exports found */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
Object.defineProperty(exports, "__esModule", { value: true });
__webpack_require__(/*! @dotnet/jsinterop */ "../node_modules/@dotnet/jsinterop/dist/Microsoft.JSInterop.js");
const Renderer_1 = __webpack_require__(/*! ../Rendering/Renderer */ "./Rendering/Renderer.ts");
let hasRegisteredNavigationInterception = false;
let hasRegisteredNavigationEventListeners = false;
// Will be initialized once someone registers
let notifyLocationChangedCallback = null;
// These are the functions we're making available for invocation from .NET
exports.internalFunctions = {
    listenForNavigationEvents,
    enableNavigationInterception,
    navigateTo,
    getBaseURI: () => document.baseURI,
    getLocationHref: () => location.href,
};
function listenForNavigationEvents(callback) {
    notifyLocationChangedCallback = callback;
    if (hasRegisteredNavigationEventListeners) {
        return;
    }
    hasRegisteredNavigationEventListeners = true;
    window.addEventListener('popstate', () => notifyLocationChanged(false));
}
function enableNavigationInterception() {
    if (hasRegisteredNavigationInterception) {
        return;
    }
    hasRegisteredNavigationInterception = true;
    document.addEventListener('click', event => {
        if (event.button !== 0 || eventHasSpecialKey(event)) {
            // Don't stop ctrl/meta-click (etc) from opening links in new tabs/windows
            return;
        }
        // Intercept clicks on all <a> elements where the href is within the <base href> URI space
        // We must explicitly check if it has an 'href' attribute, because if it doesn't, the result might be null or an empty string depending on the browser
        const anchorTarget = findClosestAncestor(event.target, 'A');
        const hrefAttributeName = 'href';
        if (anchorTarget && anchorTarget.hasAttribute(hrefAttributeName)) {
            const targetAttributeValue = anchorTarget.getAttribute('target');
            const opensInSameFrame = !targetAttributeValue || targetAttributeValue === '_self';
            if (!opensInSameFrame) {
                return;
            }
            const href = anchorTarget.getAttribute(hrefAttributeName);
            const absoluteHref = toAbsoluteUri(href);
            if (isWithinBaseUriSpace(absoluteHref)) {
                event.preventDefault();
                performInternalNavigation(absoluteHref, true);
            }
        }
    });
}
function navigateTo(uri, forceLoad) {
    const absoluteUri = toAbsoluteUri(uri);
    if (!forceLoad && isWithinBaseUriSpace(absoluteUri)) {
        // It's an internal URL, so do client-side navigation
        performInternalNavigation(absoluteUri, false);
    }
    else if (forceLoad && location.href === uri) {
        // Force-loading the same URL you're already on requires special handling to avoid
        // triggering browser-specific behavior issues.
        // For details about what this fixes and why, see https://github.com/aspnet/AspNetCore/pull/10839
        const temporaryUri = uri + '?';
        history.replaceState(null, '', temporaryUri);
        location.replace(uri);
    }
    else {
        // It's either an external URL, or forceLoad is requested, so do a full page load
        location.href = uri;
    }
}
exports.navigateTo = navigateTo;
function performInternalNavigation(absoluteInternalHref, interceptedLink) {
    // Since this was *not* triggered by a back/forward gesture (that goes through a different
    // code path starting with a popstate event), we don't want to preserve the current scroll
    // position, so reset it.
    // To avoid ugly flickering effects, we don't want to change the scroll position until the
    // we render the new page. As a best approximation, wait until the next batch.
    Renderer_1.resetScrollAfterNextBatch();
    history.pushState(null, /* ignored title */ '', absoluteInternalHref);
    notifyLocationChanged(interceptedLink);
}
function notifyLocationChanged(interceptedLink) {
    return __awaiter(this, void 0, void 0, function* () {
        if (notifyLocationChangedCallback) {
            yield notifyLocationChangedCallback(location.href, interceptedLink);
        }
    });
}
let testAnchor;
function toAbsoluteUri(relativeUri) {
    testAnchor = testAnchor || document.createElement('a');
    testAnchor.href = relativeUri;
    return testAnchor.href;
}
function findClosestAncestor(element, tagName) {
    return !element
        ? null
        : element.tagName === tagName
            ? element
            : findClosestAncestor(element.parentElement, tagName);
}
function isWithinBaseUriSpace(href) {
    const baseUriWithTrailingSlash = toBaseUriWithTrailingSlash(document.baseURI); // TODO: Might baseURI really be null?
    return href.startsWith(baseUriWithTrailingSlash);
}
function toBaseUriWithTrailingSlash(baseUri) {
    return baseUri.substr(0, baseUri.lastIndexOf('/') + 1);
}
function eventHasSpecialKey(event) {
    return event.ctrlKey || event.shiftKey || event.altKey || event.metaKey;
}


/***/ })

/******/ });
//# sourceMappingURL=blazor.electron.js.map