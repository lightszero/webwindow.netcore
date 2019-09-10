///<reference path="../electron.d.ts"/>


import electron = require('electron');
import webhost = require("./webhost");
var host: webhost.wshost | null = null;
function main(): void {
  //入参方法，通过命令行可以传递数据给electron
  var url = electron.app.commandLine.getSwitchValue("controlurl");
  console.log("controlurl=" + url);

  host = new webhost.wshost();
  host.begin(url);
}

electron.app.on("ready", main);
electron.app.on("window-all-closed", () => {
  //[TAG] is importent info.
  console.log("[TAG]all window closed");
});
console.log("electron app start.");
