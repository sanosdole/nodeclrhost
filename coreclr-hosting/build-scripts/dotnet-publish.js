var process = require('process');
var exec = require('child_process').exec;
function puts(error, stdout, stderr) { 
   console.log(stdout); 
}

var os = require('os');
let rtId = "win10-x64";
if (os.type() === 'Linux') 
   rtId = "linux-x64"; 
else if (os.type() === 'Darwin') 
   rtId = "osx-x64"; 
else if (os.type() === 'Windows_NT') 
   rtId = "win-x64";
else
   throw new Error("Unsupported OS found: " + os.type());

let command = "dotnet publish --self-contained -r " + rtId + " " + process.argv[2] + " -o " + process.argv[3];
console.log("Executing: " + command);
exec(command, puts);