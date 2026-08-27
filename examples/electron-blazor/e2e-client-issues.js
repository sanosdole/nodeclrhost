// Automated e2e driver for the client-issue repro page.
// Run with: electron e2e-client-issues.js   (from the example root; npm run build first)
"use strict";
const path = require("path");
const { app } = require("electron");
const coreclrhosting = require("coreclr-hosting");

process.chdir(__dirname);
global.electron = require("electron");
global.appRoot = __dirname;
global.preloadScriptPath = path.resolve(__dirname, "preload.js");

const results = [];
let failed = false;
function report(name, ok, detail) {
    results.push({ name, ok, detail: String(detail) });
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
            "  const link = Array.from(document.querySelectorAll('a')).find(a => (a.textContent || '').includes('Client issues'));",
            "  if (!link) throw new Error('no-clientissues-link');",
            "  link.click();",
            "  await new Promise(r => setTimeout(r, 2000));",
            "  const nullBtn = Array.from(document.querySelectorAll('button')).find(b => b.textContent.includes('Invoke null'));",
            "  if (nullBtn) nullBtn.click();",
            "  const nullResult = await new Promise(res => { const t0 = Date.now(); const poll = setInterval(() => { const el = document.getElementById('nullResult'); if (el && el.textContent && !el.textContent.includes('invoking')) { clearInterval(poll); res(el.textContent); } else if (Date.now() - t0 > 10000) { clearInterval(poll); res('timeout'); } }, 200); });",
            "  const propBtn = Array.from(document.querySelectorAll('button')).find(b => b.textContent.includes('Read/write property'));",
            "  if (propBtn) propBtn.click();",
            "  const propResult = await new Promise(res => { const t0 = Date.now(); const poll = setInterval(() => { const el = document.getElementById('propResult'); if (el && el.textContent && !el.textContent.includes('invoking')) { clearInterval(poll); res(el.textContent); } else if (Date.now() - t0 > 10000) { clearInterval(poll); res('timeout'); } }, 200); });",
            "  return 'null=' + nullResult + ' prop=' + propResult;",
            "})().catch(function (e) { return 'ERR:' + JSON.stringify(e && (e.stack || e.message)); })"
        ].join(NL);

        try {
            const out = await wc.executeJavaScript(code);
            const mNull = out.match(/null=(.+?) prop=/);
            const mProp = out.match(/prop=(.+)$/);
            const nullR = mNull ? mNull[1] : out;
            const propR = mProp ? mProp[1] : out;
            report("issue2 null IJSObjectReference", /OK: returned null/i.test(nullR), nullR);
            report("issue #3 property get/set", /OK: counter=7/.test(propR), propR);
        } catch (ex) {
            report("e2e driver", false, ex && ex.message);
        }

        console.log("=====RESULT=====");
        console.log(JSON.stringify(results));
        app.exit(failed ? 1 : 0);
    });
});

coreclrhosting.runCoreApp(path.join(__dirname, "LocalService/bin/Debug/net10.0/LocalService.dll"));