"use strict";
const path = require("path");
const { app } = require("electron");
const coreclrhosting = require("coreclr-hosting");

process.chdir(__dirname);
global.electron = require("electron");
global.appRoot = __dirname;
global.preloadScriptPath = path.resolve(__dirname, "preload.js");

app.on("browser-window-created", (_e, win) => {
    win.webContents.on("render-process-gone", (_e2, d) => console.log("RENDERER GONE: " + (d && d.reason)));
    win.webContents.once("did-finish-load", async () => {
        const wc = win.webContents;
        const NL = "\n";
        const code = [
            "(async () => {",
            "  await new Promise((resolve, reject) => { const t0 = Date.now(); const p = setInterval(() => { if (window.Blazor && window.Blazor._internal) { clearInterval(p); resolve(); } else if (Date.now() - t0 > 40000) { clearInterval(p); reject(new Error('boot')); } }, 400); });",
            "  await new Promise(r => setTimeout(r, 1500));",
            "  const link = Array.from(document.querySelectorAll('a')).find(a => (a.textContent || '').includes('Virtualization'));",
            "  if (!link) throw new Error('no-link');",
            "  link.click();",
            "  await new Promise(r => setTimeout(r, 2500));",
            "  let scroller = null;",
            "  Array.from(document.querySelectorAll('div')).forEach(function (d) { if (d.style && d.style.height.indexOf('300px') >= 0) scroller = d; });",
            "  if (!scroller) throw new Error('no-scroller');",
            "  const maxIndex = function () {",
            "    let m = -1;",
            "    Array.from(scroller.querySelectorAll('div')).forEach(function (d) {",
            "      var mm = (d.textContent || '').match(/B (\\d+)/);",
            "      if (mm) { var v = parseInt(mm[1], 10); if (v > m) m = v; }",
            "    });",
            "    return m;",
            "  };",
            "  const snap = function (tag) { return tag + '{idx=' + maxIndex() + ' rows=' + scroller.querySelectorAll('[class*=flex-row]').length + '}'; };",
            "  const steps = [];",
            "  // Gradual scroll to bottom (like a real user dragging).",
            "  for (let i = 0; i < 40; i++) { scroller.scrollTop += 8500; await new Promise(r => setTimeout(r, 200)); }",
            "  scroller.scrollTop = scroller.scrollHeight; await new Promise(r => setTimeout(r, 1500));",
            "  steps.push(snap('end'));",
            "  // Gradual scroll back up to top.",
            "  for (let i = 0; i < 40; i++) { scroller.scrollTop -= 8500; if (scroller.scrollTop < 0) scroller.scrollTop = 0; await new Promise(r => setTimeout(r, 200)); }",
            "  scroller.scrollTop = 0; await new Promise(r => setTimeout(r, 1500));",
            "  steps.push(snap('top'));",
            "  // Scroll down a little again.",
            "  for (let i = 0; i < 10; i++) { scroller.scrollTop += 8500; await new Promise(r => setTimeout(r, 200)); }",
            "  await new Promise(r => setTimeout(r, 1000));",
            "  steps.push(snap('mid'));",
            "  return steps.join(' | ');",
            "})().catch(function (e) { return 'ERR:' + JSON.stringify(e && (e.stack || e.message)); })"
        ].join(NL);
        try {
            const out = await wc.executeJavaScript(code);
            console.log("VIRTUALIZE_RESULT: " + out);
        } catch (e) {
            console.log("VIRTUALIZE_OUTER_ERR: " + JSON.stringify(e && (e.stack || e.message)));
        }
        console.log("CRASHED: " + crashed);
        app.exit(0);
    });
});

coreclrhosting.runCoreApp(path.join(__dirname, "LocalService/bin/Debug/net10.0/LocalService.dll"));