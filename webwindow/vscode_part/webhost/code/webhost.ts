import http = require("http");
import electron = require('electron');
export class wshost
{
    server: http.Server | null;
    port: number;
    windows: { [id: number]: electron.BrowserWindow };
    urlcontrol: string | null;
    constructor()
    {
        this.server = null;
        this.urlcontrol = null;
        this.port = 0;
        this.windows = {};
    }
    begin(url: string): void
    {
        this.urlcontrol = url;
        this.server = new http.Server((req, res) =>
        {
            this.onRequest(req, res);
        });
        this.port = 8888;
        this.server.on("error", (err) =>
        {
            if (this.port > 10000)
            {
                //[TAG] is importent info.
                console.log("[TAG]listen fail");
                console.log("can't open http port:" + err);
                return;
            }
            this.port++;
            if (this.server)
                this.server.listen(this.port);
        })
        this.server.on("listening", () =>
        {
            //[TAG] is importent info.
            console.log("[TAG]listen at:" + this.port);
        });
        this.server.listen(this.port);

    }
    onRequest(req: http.IncomingMessage, res: http.ServerResponse)
    {
        var string = req.url;
        if (string == undefined) return;

        console.log("onRequest"+string);

        //query cmd
        var i = string.indexOf('?');
        if (i <= 0)
        {
            res.writeHead(200, { 'Content-Type': 'text/plain' });
            res.end(JSON.stringify({ "code": -1, "msg": 'Hello World.' }));
            return;
        }
        var query = decodeURIComponent(string.substr(i + 1));
        var lines = query.split("&");
        var method: string | null = null;
        var params: any;
        for (var i = 0; i < lines.length; i++)
        {
            var line = lines[i];
            var words = line.split("=");
            if (words.length > 1 && words[0] == "method")
            {
                method = words[1];
            }
            if (words.length > 1 && words[0] == "params")
            {
                params = JSON.parse(words[1]);
            }
        }
        if (method == null)
        {
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
    }
    onMethod(method: string, params: any, res: http.ServerResponse): boolean
    {
        switch (method)
        {
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
                    var outinfo: { [id: number]: boolean } = {};
                    for (var key in this.windows)
                    {
                        var b = this.windows[key].isVisible();
                        outinfo[key] = b;
                    }
                    res.writeHead(200, { 'Content-Type': 'text/plain' });
                    res.end(JSON.stringify({ "code": 0, "data": outinfo }));

                    electron.app.exit();
                }
            case "window.create":
                {
                    console.log("window.create"+JSON.stringify(params));
                    var winstyle: electron.BrowserWindowConstructorOptions = params[0];
                    var url: string = params[1];
                    var hide: boolean = params[2];
                    if (winstyle.autoHideMenuBar == undefined)
                        winstyle.autoHideMenuBar = true;
                    if (hide == undefined)
                        hide = false;
                    var mainWindow = new electron.BrowserWindow(winstyle);
                    var _id = this.regWindow(mainWindow);
                    console.log("try loadUrl=" + url + "?curl=" + this.urlcontrol + "&winid=" + _id);
                    mainWindow.loadURL(url);
                    if (hide)
                    {
                        this.showWindow(_id, false);
                    }
                    res.writeHead(200, { 'Content-Type': 'text/plain' });
                    res.end(JSON.stringify({ "code": 1, "winid": _id }));
                    return true;
                }
            case "window.show":
                {
                    var _id: number = params[0];
                    this.showWindow(_id, true);
                    res.writeHead(200, { 'Content-Type': 'text/plain' });
                    res.end(JSON.stringify({ "code": 1, "winid": _id }));
                    return true;
                }
            case "window.hide":
                {
                    var id: number = params[0];
                    this.showWindow(id, false);
                    res.writeHead(200, { 'Content-Type': 'text/plain' });
                    res.end(JSON.stringify({ "code": 1, "winid": id }));
                    return true;
                }
            case "window.close":
                {
                    var id: number = params[0];
                    this.closeWindow(id);
                    res.writeHead(200, { 'Content-Type': 'text/plain' });
                    res.end(JSON.stringify({ "code": 1, "winid": id }));
                }
        }
        return false;
    }

    regWindow(window: electron.BrowserWindow): number
    {
        this.windows[window.id] = window;
        window.on("close", (e) =>
        {
            delete this.windows[window.id];
        });
        return window.id;
    }
    closeWindow(id: number): boolean
    {
        if (this.windows[id] == undefined)
            return false;
        this.windows[id].close();
        return true;
    }
    showWindow(id: number, show: boolean, focus?: boolean): boolean
    {
        if (this.windows[id] == undefined)
            return false;
        var win = this.windows[id];

        if (show)
        {
            win.show();
            if (focus)
                win.focus();
        }
        else
        {
            win.hide();
        }
        return true;

    }
}
