const path = require('path');
const { Application } = require('spectron');

function initialiseSpectron() {
    let electronPath = path.join(__dirname, "../node_modules", ".bin", "electron");
    const appPath = path.join(__dirname, "../index.js");
    if (process.platform === "win32") {
        electronPath += ".cmd";
    }

    return new Application({
        path: electronPath,
        args: [appPath],
        env: {
            ELECTRON_ENABLE_LOGGING: true,
            ELECTRON_ENABLE_STACK_DUMPING: true,
            NODE_ENV: "development"
        },
        startTimeout: 20000,
        chromeDriverLogPath: 'chromedriverlog.txt'
    });
}

const app = initialiseSpectron();

const chai = require("chai");
chai.should();

describe("blazor-test-app", function () {
    this.timeout(20000);
    // CSS selectors
    const header = "body > app > div.main > div.content.px-4 > h1";
    const counterNavBtn = "body > app > div.sidebar > div.collapse > ul > li:nth-child(2) > a";
    const currentCounterDisplay = "body > app > div.main > div.content.px-4 > p:nth-child(3)";
    const counterIncrementButton = "body > app > div.main > div.content.px-4 > button";
    const counterInitializedDisplay = "body > app > div.main > div.content.px-4 > p:nth-child(7)";

    const jsInteropNavBtn = "body > app > div.sidebar > div:nth-child(2) > ul > li:nth-child(3) > a";
    const jsInteropInput = "#jsInteropInput";
    const jsInteropButton = "#triggerjsfromdotnet";
    const dotnetInteropResult = "#dotnetinvocationcount";
    const dotnetInteropButton = "#triggerdotnetfromjs";
    const jsInteropTitle = "body > app > div.main > div.content.px-4 > h1";
    const jsInteropResult = "#welcome";


    // Start spectron
    before(function () {
        return app.start();
    });

    // Stop Electron
    after(function () {
        if (app && app.isRunning()) {
            return app.stop();
        }
    });

    describe("Home", function () {
        // wait for Electron window to open
        it('open window', async function () {
            const client = app.client;
            await client.waitUntilWindowLoaded();
            (await (await client.$(header)).getText()).should.equal("Hello, world!");
            (await client.getWindowCount()).should.equal(1);
        });

        // click on link in sidebar
        it("go to counter", async function () {
            const client = app.client;

            await (await client.$(counterNavBtn)).click();
            (await (await client.$(currentCounterDisplay)).getText()).should.equal('Current count: 0');
            (await (await client.$(counterInitializedDisplay)).getText()).should.equal("OnInitializedAsync");
            (await (await client.$(counterIncrementButton)).isEnabled()).should.equal(false);

            await client.waitUntilTextExists(counterInitializedDisplay, "OnInitializedAsync after delay");

            (await (await client.$(currentCounterDisplay)).getText()).should.equal('Current count: 5');
            (await (await client.$(counterIncrementButton)).isEnabled()).should.equal(true);
        });

        it("increment counter", async function () {
            const client = app.client;
            await (await client.$(counterIncrementButton)).click();
            await client.waitUntilTextExists(currentCounterDisplay, 'Current count: 6');
        });

        it ("go to jsinterop", async function () {
            const client = app.client;
            
            await (await client.$(jsInteropNavBtn)).click();
            await client.waitUntilTextExists(jsInteropTitle, "JavaScript Interop");

            (await (await client.$(jsInteropButton)).isEnabled()).should.equal(true);
        });

        it("execute dotnet-js interop", async function () {
            const client = app.client;
            await (await client.$(jsInteropInput)).setValue("testuser");
            await (await client.$(jsInteropButton)).click();
            await client.waitUntilTextExists(jsInteropResult, 'Hello testuser! Welcome to Blazor!');
        });

        it("execute js-dotnet interop", async function () {
            const client = app.client;
            await (await client.$(dotnetInteropButton)).click();
            await client.waitUntilTextExists(dotnetInteropResult, '2');
        });
    });
});