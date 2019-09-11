"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var http = require("http");
var electron = require("electron");
var wshost = /** @class */ (function () {
    function wshost() {
        this.server = null;
        this.urlcontrol = null;
        this.port = 0;
        this.windows = {};
    }
    wshost.prototype.begin = function (url) {
        var _this = this;
        this.urlcontrol = url;
        this.server = new http.Server(function (req, res) {
            _this.onRequest(req, res);
        });
        this.port = 8888;
        this.server.on("error", function (err) {
            if (_this.port > 10000) {
                //[TAG] is importent info.
                console.log("[TAG]listen fail");
                console.log("can't open http port:" + err);
                return;
            }
            _this.port++;
            if (_this.server)
                _this.server.listen(_this.port);
        });
        this.server.on("listening", function () {
            //[TAG] is importent info.
            console.log("[TAG]listen at:" + _this.port);
        });
        this.server.listen(this.port);
    };
    wshost.prototype.onRequest = function (req, res) {
        var string = req.url;
        if (string == undefined)
            return;
        console.log("onRequest" + string);
        //query cmd
        var i = string.indexOf('?');
        if (i <= 0) {
            res.writeHead(200, { 'Content-Type': 'text/plain' });
            res.end(JSON.stringify({ "code": -1, "msg": 'Hello World.' }));
            return;
        }
        var query = decodeURIComponent(string.substr(i + 1));
        var lines = query.split("&");
        var method = null;
        var params;
        for (var i = 0; i < lines.length; i++) {
            var line = lines[i];
            var words = line.split("=");
            if (words.length > 1 && words[0] == "method") {
                method = words[1];
            }
            if (words.length > 1 && words[0] == "params") {
                params = JSON.parse(words[1]);
            }
        }
        if (method == null) {
            res.writeHead(200, { 'Content-Type': 'text/plain' });
            res.end(JSON.stringify({ "code": -2, "msg": 'miss method.' }));
            return;
        }
        console.log("onreq:" + query);
        console.log("method:" + method);
        console.log("params:" + params);
        if (this.onMethod(method, params, res))
            return;
        res.writeHead(200, { 'Content-Type': 'text/plain' });
        res.end(JSON.stringify({ "code": -100, "msg": 'other error.' }));
        return;
    };
    wshost.prototype.onMethod = function (method, params, res) {
        switch (method) {
            case "hi":
                {
                    res.writeHead(200, { 'Content-Type': 'text/plain' });
                    res.end(JSON.stringify({ "code": 0, "msg": 'hi.' }));
                    return true;
                }
            case "quit":
                {
                    res.writeHead(200, { 'Content-Type': 'text/plain' });
                    res.end(JSON.stringify({ "code": 0, "msg": 'electron exit.' }));
                    electron.app.exit();
                    return true;
                }
            case "window.stats":
                {
                    var outinfo = {};
                    for (var key in this.windows) {
                        var b = this.windows[key].isVisible();
                        outinfo[key] = b;
                    }
                    res.writeHead(200, { 'Content-Type': 'text/plain' });
                    res.end(JSON.stringify({ "code": 0, "data": outinfo }));
                    electron.app.exit();
                }
            case "window.create":
                {
                    console.log("window.create" + JSON.stringify(params));
                    var winstyle = params[0];
                    var url = params[1];
                    var hide = params[2];
                    //这个机制保证菜单栏隐藏
                    if (winstyle.autoHideMenuBar == undefined)
                        winstyle.autoHideMenuBar = true;
                    if (hide == undefined)
                        hide = false;
                    //这个机制保证nodejs被打开，这样才能requrie
                    if (winstyle.webPreferences == null) {
                        winstyle.webPreferences = { nodeIntegration: true };
                    }
                    else {
                        winstyle.webPreferences.nodeIntegration = true;
                    }
                    var mainWindow = new electron.BrowserWindow(winstyle);
                    var _id = this.regWindow(mainWindow);
                    var loadurl = url + "?curl=" + this.urlcontrol + "&winid=" + _id;
                    console.log("try loadUrl=" + loadurl);
                    mainWindow.loadURL(loadurl);
                    if (hide) {
                        this.showWindow(_id, false);
                    }
                    res.writeHead(200, { 'Content-Type': 'text/plain' });
                    res.end(JSON.stringify({ "code": 1, "winid": _id }));
                    return true;
                }
            case "window.show":
                {
                    var _id = params[0];
                    this.showWindow(_id, true);
                    res.writeHead(200, { 'Content-Type': 'text/plain' });
                    res.end(JSON.stringify({ "code": 1, "winid": _id }));
                    return true;
                }
            case "window.hide":
                {
                    var id = params[0];
                    this.showWindow(id, false);
                    res.writeHead(200, { 'Content-Type': 'text/plain' });
                    res.end(JSON.stringify({ "code": 1, "winid": id }));
                    return true;
                }
            case "window.close":
                {
                    var id = params[0];
                    this.closeWindow(id);
                    res.writeHead(200, { 'Content-Type': 'text/plain' });
                    res.end(JSON.stringify({ "code": 1, "winid": id }));
                }
        }
        return false;
    };
    wshost.prototype.regWindow = function (window) {
        var _this = this;
        this.windows[window.id] = window;
        window.on("close", function (e) {
            delete _this.windows[window.id];
        });
        return window.id;
    };
    wshost.prototype.closeWindow = function (id) {
        if (this.windows[id] == undefined)
            return false;
        this.windows[id].close();
        return true;
    };
    wshost.prototype.showWindow = function (id, show, focus) {
        if (this.windows[id] == undefined)
            return false;
        var win = this.windows[id];
        if (show) {
            win.show();
            if (focus)
                win.focus();
        }
        else {
            win.hide();
        }
        return true;
    };
    return wshost;
}());
exports.wshost = wshost;
//# sourceMappingURL=webhost.js.map