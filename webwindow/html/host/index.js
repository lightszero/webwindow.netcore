"use strict";
///<reference path="../electron.d.ts"/>
Object.defineProperty(exports, "__esModule", { value: true });
var electron = require("electron");
var webhost = require("./webhost");
var host = null;
function main() {
    //入参方法，通过命令行可以传递数据给electron
    var url = electron.app.commandLine.getSwitchValue("controlurl");
    console.log("controlurl=" + url);
    host = new webhost.wshost();
    host.begin(url);
}
electron.app.on("ready", main);
electron.app.on("window-all-closed", function () {
    //[TAG] is importent info.
    //依然选择所有窗口都关闭则退出electron进程
    console.log("[TAG]all window closed.quit");
    electron.app.exit();
});
console.log("electron app start.");
//# sourceMappingURL=index.js.map