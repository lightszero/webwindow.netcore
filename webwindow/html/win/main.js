"use strict";
///<reference path="../../webhost/electron.d.ts"/>
var remote = require('electron').remote;
function closethis() {
    var win = remote.getCurrentWindow();
    win.close();
}
function settitle(title) {
    var win = remote.getCurrentWindow();
    win.setTitle(title);
}
function decodequery() {
    var url = document.location.toString();
    var arrUrl = url.split("?");
    var query = decodeURIComponent(arrUrl[1]);
    var lines = query.split("&");
    var _curl = "";
    var _winid = 0;
    for (var i = 0; i < lines.length; i++) {
        var line = lines[i];
        var words = line.split("=");
        if (words.length > 1 && words[0] == "curl") {
            _curl = words[1];
        }
        if (words.length > 1 && words[0] == "winid") {
            _winid = parseInt(words[1]);
        }
    }
    return { curl: _curl, winid: _winid };
}
var __pageoption;
window.onload = function () {
    __pageoption = decodequery();
    console.log("this winid =" + __pageoption.winid);
    console.log("curl = " + __pageoption.curl);
    var ws = new WebSocket(__pageoption.curl);
    ws.onopen = function () {
        //链接通，告之接收方
        // console.log("this open");
        var ret = { "cmd": "init", "vars": [__pageoption.winid] };
        ws.send(JSON.stringify(ret));
    };
    ws.onclose = function () {
        //利用websocket断开去关闭这个窗口
        console.log("this close.");
        closethis();
    };
    ws.onerror = function () {
        console.log("this error.");
        closethis();
    };
    ws.onmessage = function (ev) {
        //收到一些神马
        var json = JSON.parse(ev.data);
        var cmd = json["cmd"];
        var vars = json["vars"];
        if (cmd == "eval") {
            var got = eval(vars[0]);
            var ret = { "cmd": "eval_back", "vars": [got] };
            ws.send(JSON.stringify(ret));
        }
        if (cmd == "settitle") {
            var title = vars[0];
            settitle(title);
            var ret = { "cmd": "settitle_back" };
            ws.send(JSON.stringify(ret));
        }
    };
};
function testcode() {
}
//# sourceMappingURL=main.js.map