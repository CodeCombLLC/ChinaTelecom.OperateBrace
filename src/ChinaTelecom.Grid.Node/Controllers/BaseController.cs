using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ChinaTelecom.Grid.Node.Models;

namespace ChinaTelecom.Grid.Node.Controllers
{
    public class BaseController : BaseController<GridContext>
    {
        public override void Prepare()
        {
            base.Prepare();
            var Config = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            if (!HttpContext.Request.Headers.ContainsKey("PrivateKey") || Config["System:PrivateKey"] != HttpContext.Request.Headers["PrivateKey"])
                throw new Exception("Access denied");
        }
    }
}
