const fs = require("fs");
namespace mylog
{
    export function log(msg:string):void
    {
        var date =new Date();
        fs.appendFile("log.txt",date.toDateString()+":"+msg,()=>
        {

        });
    }
}