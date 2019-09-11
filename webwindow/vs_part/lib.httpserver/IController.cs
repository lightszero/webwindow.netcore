using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace light.http.server
{
    public interface IController
    {
        Task ProcessAsync(HttpContext context);
    }
}
