using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace light.http.server
{
    public class JSONRPCController : IController
    {
        public JSONRPCController()
        {
            mapAction = new System.Collections.Concurrent.ConcurrentDictionary<string, ActionRPC>();
        }
        public delegate Task<JObject> ActionRPC(JObject request);
        public delegate Task<ErrorObject> ActionRPCFail(JObject request, string errorMessage);
        public class ErrorObject
        {
            public int code;
            public string message;
            public JObject data;
            public override string ToString()
            {
                return ToJObject().ToString();
            }
            public JObject ToJObject()
            {
                JObject obj = new JObject();
                obj["code"] = code;
                obj["message"] = message;
                obj["data"] = data;
                return obj;
            }
        }
        System.Collections.Concurrent.ConcurrentDictionary<string, ActionRPC> mapAction;
        ActionRPCFail failAction;
        public void AddAction(string method, ActionRPC action)
        {
            mapAction[method] = action;
        }
        public void SetFailAction(ActionRPCFail action)
        {
            failAction = action;
        }
        public async Task ProcessAsync(HttpContext context)
        {
            JObject jsonParam = null;

            try
            {
                context.Response.ContentType = "application/json;charset=UTF-8";

                context.Response.Headers["Access-Control-Allow-Origin"] = "*";
                context.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST";
                context.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type";
                context.Response.Headers["Access-Control-Max-Age"] = "31536000";

                if (context.Request.Method == "GET")
                {
                    string jsonrpc = context.Request.Query["jsonrpc"];
                    string id = context.Request.Query["id"];
                    string _method = context.Request.Query["method"];
                    string _params = context.Request.Query["params"];
                    if (jsonrpc == null)
                    {
                        throw new Exception("do not have element:jsonrpc");
                    }
                    if (id == null)
                    {
                        throw new Exception("do not have element:id");
                    }
                    if (_method == null)
                    {
                        throw new Exception("do not have element:method");
                    }
                    if (_params == null)
                    {
                        throw new Exception("do not have element:params");
                    }
                    jsonParam = new JObject();
                    jsonParam["jsonrpc"] = jsonrpc;
                    jsonParam["id"] = id;
                    jsonParam["method"] = _method;
                    jsonParam["params"] = JArray.Parse(_params);
                }
                else if (context.Request.Method == "POST")
                {
                    var ctype = context.Request.ContentType;
                    if (ctype == "application/x-www-form-urlencoded" || (ctype.IndexOf("multipart/form-data;") == 0))
                    {
                        var form = await FormData.FromRequest(context.Request);
                        var _jsonrpc = form.mapParams["jsonrpc"];
                        var _id = form.mapParams["id"];
                        var _method = form.mapParams["method"];
                        var _strparams = form.mapParams["params"];
                        if (_jsonrpc == null)
                        {
                            throw new Exception("do not have element:jsonrpc");
                        }
                        if (_id == null)
                        {
                            throw new Exception("do not have element:id");
                        }
                        if (_method == null)
                        {
                            throw new Exception("do not have element:method");
                        }
                        if (_strparams == null)
                        {
                            throw new Exception("do not have element:params");
                        }
                        jsonParam = new JObject();
                        jsonParam["jsonrpc"] = _jsonrpc;
                        jsonParam["id"] = long.Parse(_id);
                        jsonParam["method"] = _method;
                        jsonParam["params"] = JArray.Parse(_strparams);
                    }
                    else
                    {
                        var text = await FormData.GetStringFromRequest(context.Request);
                        jsonParam = JObject.Parse(text);
                        if (jsonParam["jsonrpc"] == null)
                        {
                            throw new Exception("do not have element:jsonrpc");
                        }
                        if (jsonParam["id"] == null)
                        {
                            throw new Exception("do not have element:id");
                        }
                        if (jsonParam["method"] == null)
                        {
                            throw new Exception("do not have element:method");
                        }
                        if (jsonParam["params"] == null)
                        {
                            throw new Exception("do not have element:params");
                        }
                    }
                }
                else
                {
                    throw new Exception("not implement request method.");
                }

                if (mapAction.TryGetValue(jsonParam["method"].Value<string>(), out ActionRPC method))
                {
                    var json = method(jsonParam).Result;
                    JObject result = new JObject();
                    result["result"] = json;
                    result["id"] = jsonParam["id"].Value<int>();
                    result["jsonrpc"] = "2.0";
                    await context.Response.WriteAsync(result.ToString());
                }
                else
                {
                    throw new Exception("Do not have this method.");
                }
            }
            catch (Exception err)
            {

                try
                {

                    if (failAction != null)
                    {
                        var errorobj = await failAction(jsonParam, err.Message);
                        if (jsonParam == null)
                        {
                            jsonParam = new JObject();
                            jsonParam["jsonrpc"] = "2.0";
                            jsonParam["id"] = null;
                        }
                        jsonParam["error"] = errorobj.ToJObject();
                        await context.Response.WriteAsync(jsonParam.ToString());

                    }
                    else
                    {
                        var errorobj = new ErrorObject();
                        errorobj.data = jsonParam;
                        errorobj.message = err.Message;
                        errorobj.code = -32000;
                        if (jsonParam == null)
                        {
                            jsonParam = new JObject();
                            jsonParam["jsonrpc"] = "2.0";
                            jsonParam["id"] = null;
                        }
                        jsonParam["error"] = errorobj.ToJObject();
                        await context.Response.WriteAsync(jsonParam.ToString());
                    }
                }
                catch
                {

                }
            }
        }
    }
}
