window.exampleJsFunctions = {
    getInputValue: function (inputElem) {
        
      return inputElem.value;
    },
    displayWelcome: function (welcomeMessage) {
      document.getElementById('welcome').innerText = welcomeMessage;
    },
    returnArrayAsyncJs: function () {
      DotNet.invokeMethodAsync('BlazorApp', 'ReturnArrayAsync')
        .then(data => {
          data.push(4);
            console.log(data);
        });
    },
    sayHello: function (dotnetHelper) {
      return dotnetHelper.invokeMethodAsync('SayHello')      
        .then(r => console.log(r))
        .then(r => DotNet.invokeMethodAsync('BlazorApp', 'AsyncVoidTest', dotnetHelper));
    }
  };