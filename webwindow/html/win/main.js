"use strict";
///<reference path="../../webhost/electron.d.ts"/>
var url = document.location.toString();
var arrUrl = url.split("?");
var para = arrUrl[1];
console.log("what?" + url);
console.log("para=" + para);
var app = require('electron').app;
var url = app.commandLine.getSwitchValue("controlurl");
console.log("controlurl=" + url);
//# sourceMappingURL=main.js.map