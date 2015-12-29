using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Configuration;

namespace ChinaTelecom.OperateBrace.Controllers
{
    public class SystemController : BaseController
    {
        [HttpGet]
        [AnyRoles("系统管理员")]
        public IActionResult Threshold()
        {
            return View();
        }

        [HttpPost]
        [AnyRoles("系统管理员")]
        [ValidateAntiForgeryTokenAttribute]
        public IActionResult Threshold(int by, int bc, int cy, int cc, [FromServices] IConfiguration Config)
        {
            Config["Settings:Threshold:Customer:Yellow"] = cy.ToString();
            Config["Settings:Threshold:Customer:Cyan"] = cy.ToString();
            Config["Settings:Threshold:BusinessHall:Yellow"] = cy.ToString();
            Config["Settings:Threshold:BusinessHall:Cyan"] = cy.ToString();

            return Prompt(x => 
            {
                x.Title = "修改成功";
                x.Details = "阈值已成功修改！";
            });
        }
    }
}
