using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebWindow
{
    //这边的简单类型只要名字和 electron.BrowserWindowConstructorOptions 匹配 就可以自动生效 string number bool

    //复杂的类型有一些需要底层转换，比如parentwindow，这个随后会在做对话框的时候处理
    public class WindowCreateOption
    {
        public string title;
        public int? x;
        public int? y;
        public int? width;
        public int? height;
        public JObject ToJson()
        {
            var objwin =new JObject();
            objwin["title"] = title;
            if (x != null)objwin["x"]= x.Value;
            if (y != null) objwin["y"] = y.Value;
            if (width != null) objwin["x"] = width.Value;
            if (height != null) objwin["x"] = height.Value;

            return objwin;

        }
    }
}
