///<reference path="../../webhost/electron.d.ts"/>

const remote =require("electron").remote;

//给js调用的API
class API
{
    sendback:((vars: any[]) => void) | undefined;
}
var __api:API =new API();
window.onload = () =>
{
    let __pageoption: { curl: string, winid: number };

    //把函数藏起来，尽量不污染空间
    let closethis:()=>void =()=>
    {
        let win = remote.getCurrentWindow();
        win.close();    
    }
    let settitle:(title: string)=> void=(title:string)=>
    {
        let win = remote.getCurrentWindow();
        win.setTitle(title);
    }
    let decodequery: () => { curl: string, winid: number } =() =>
        {
            let url = document.location.toString();
            let arrUrl = url.split("?");
            let query = decodeURIComponent(arrUrl[1]);
            let lines = query.split("&");
            let _curl: string = "";
            let _winid: number = 0;
            for (let i = 0; i < lines.length; i++)
            {
                let line = lines[i];
                let words = line.split("=");
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
    __pageoption = decodequery();
    console.log("this winid =" + __pageoption.winid);
    console.log("curl = " + __pageoption.curl);
    let ws: WebSocket = new WebSocket(__pageoption.curl);


    ws.onopen = () =>
    {
        //链接通，告之接收方
        // console.log("this open");
        let ret = { "cmd": "init", "vars": [__pageoption.winid] };
        ws.send(JSON.stringify(ret));
    }
    ws.onclose = () =>
    {
        //利用websocket断开去关闭这个窗口
        console.log("this close.");
        closethis();
    }
    ws.onerror = () =>
    {
        console.log("this error.");
        closethis();

    }
    ws.onmessage = (ev: MessageEvent) =>
    {
        //收到一些神马
        let json = JSON.parse(ev.data as string);
        let cmd = json["cmd"];
        let vars = json["vars"];
        if (cmd == "eval")
        {
            let got = eval(vars[0]);
            let ret = { "cmd": "eval_back", "vars": [got] };
            ws.send(JSON.stringify(ret));
        }
        if (cmd == "settitle")
        {
            let title = vars[0];
            settitle(title);
            let ret = { "cmd": "settitle_back" };
            ws.send(JSON.stringify(ret));
        }
    }


    //赋予公开的API功能
    __api.sendback= (vars:any[])=>
    {
        let ret = { "cmd": "sendback", "vars":vars};
        ws.send(JSON.stringify(ret));
    }
}
