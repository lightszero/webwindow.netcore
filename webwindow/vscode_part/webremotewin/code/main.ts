///<reference path="../../webhost/electron.d.ts"/>
const remote = require('electron').remote as Electron.Remote;
function closethis():void
{
    var win =remote.getCurrentWindow();
    win.close();
}
function settitle(title:string):void
{
    var win =remote.getCurrentWindow();
    win.setTitle(title);
}

function decodequery(): { curl: string, winid: number }
{
    var url = document.location.toString();
    var arrUrl = url.split("?");
    var query = decodeURIComponent(arrUrl[1]);
    var lines = query.split("&");
    var _curl: string = "";
    var _winid: number = 0;
    for (var i = 0; i < lines.length; i++)
    {
        var line = lines[i];
        var words = line.split("=");
        if (words.length > 1 && words[0] == "curl")
        {
            _curl = words[1];
        }
        if (words.length > 1 && words[0] == "winid")
        {
            _winid = parseInt(words[1]);
        }
    }
    return { curl: _curl, winid: _winid };
}

let __pageoption:{ curl: string, winid: number };
window.onload = () =>
{
    __pageoption = decodequery();
    console.log("this winid =" + __pageoption.winid);
    console.log("curl = " + __pageoption.curl);
    var ws:WebSocket = new WebSocket(__pageoption.curl);
        ws.onopen=()=>
        {
            //链接通，告之接收方
            // console.log("this open");
            let ret ={"cmd":"init","vars":[__pageoption.winid]};
            ws.send(JSON.stringify(ret));
        }
        ws.onclose=()=>
        {
            //利用websocket断开去关闭这个窗口
            console.log("this close.");
            closethis();
        }
        ws.onerror=()=>
        {
            console.log("this error.");
            closethis();

        }
        ws.onmessage=(ev:MessageEvent)=>
        {
            //收到一些神马
            var json = JSON.parse(ev.data as string);
            var cmd =json["cmd"];
            var vars =json["vars"];
            if(cmd=="eval")
            {
                let got = eval(vars[0]);
                let ret ={"cmd":"eval_back","vars":[got]};
                ws.send(JSON.stringify(ret));
            }
            if(cmd =="settitle")
            {
                let title =vars[0];
                settitle(title);
                let ret ={"cmd":"settitle_back"};
                ws.send(JSON.stringify(ret));
            }
        }
  
}