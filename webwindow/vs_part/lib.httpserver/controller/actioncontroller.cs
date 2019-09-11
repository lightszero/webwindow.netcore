using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace light.http.server
{
    public class ActionController : IController
    {
        public ActionController(httpserver.deleProcessHttp action)
        {
            this.action = action;
        }
        public async Task ProcessAsync(HttpContext context)
        {
            await action(context);
        }
        httpserver.deleProcessHttp action;
    }
}
