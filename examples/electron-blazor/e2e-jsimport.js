// Automated e2e driver for the "Import an ES module via IJSRuntime" example on the JSInterop page.
// Run with: electron e2e-jsimport.js   (from the example root; npm run build first)
"use strict";
const path = require("path");
const { app } = require("electron");
const coreclrhosting = require("coreclr-hosting");

process.chdir(__dirname);
global.electron = require("electron");
global.appRoot = __dirname;
global.preloadScriptPath = path.resolve(__dirname, "preload.js");

let failed = false;
function report(name, ok, detail) {
    console.log((ok ? "PASS" : "FAIL") + "  " + name + "  =>  " + String(detail));
    if (!ok) failed = true;
}

app.on("browser-window-created", (_event, win) => {
    win.webContents.once("did-finish-load", async () => {
        const wc = win.webContents;
        const NL = "\n";
        const code = [
            "(async () => {",
            "  await new Promise((resolve, reject) => { const t0 = Date.now(); const poll = setInterval(() => { if (window.Blazor && window.Blazor._internal) { clearInterval(poll); resolve(true); } else if (Date.now() - t0 > 40000) { clearInterval(poll); reject(new Error('blazor-boot-timeout')); } }, 400); });",
            "  await new Promise(r => setTimeout(r, 1500));",
            "  const link = Array.from(document.querySelectorAll('a')).find(a => (a.textContent || '').trim() === 'JSInterop');",
            "  if (!link) throw new Error('no-jsinterop-link');",
            "  link.click();",
            "  await new Promise(r => setTimeout(r, 2000));",
            "  const importBtn = Array.from(document.querySelectorAll('button')).find(b => b.textContent.includes('Import exampleJsImport.js'));",
            "  if (!importBtn) throw new Error('no-import-button');",
            "  importBtn.click();",
            "  const result = await new Promise(res => { const t0 = Date.now(); const poll = setInterval(() => { const el = document.getElementById('moduleimportresult'); if (el && el.textContent && el.textContent.length > 0) { clearInterval(poll); res(el.textContent); } else if (Date.now() - t0 > 15000) { clearInterval(poll); res('timeout'); } }, 200); });",
            "  return result;",
            "})().catch(function (e) { return 'ERR:' + JSON.stringify(e && (e.stack || e.message)); })"
        ].join(NL);

        try {
            const out = await wc.executeJavaScript(code);
            report("IJSRuntime module import + invoke", /Hello from exampleJsImport\.js/.test(out), out);
        } catch (ex) {
            report("e2e driver", false, ex && ex.message);
        }

        console.log("=====RESULT=====");
        app.exit(failed ? 1 : 0);
    });
});

coreclrhosting.runCoreApp(path.join(__dirname, "LocalService/bin/Debug/net10.0/LocalService.dll"));