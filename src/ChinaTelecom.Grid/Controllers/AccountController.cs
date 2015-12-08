using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;

namespace ChinaTelecom.Grid.Controllers
{
    public class AccountController : BaseController
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, bool remember, [FromHeader] string Referer)
        {
            var result = await SignInManager.PasswordSignInAsync(username, password, remember, false);
            if (result.Succeeded)
                return Redirect(Referer == null || Referer.IndexOf("Account") > 0 ? Url.Action("Index", "Home") : Referer);
            else
                return Prompt(x =>
                {
                    x.Title = "登录失败";
                    x.Details = "请检查用户名密码是否正确后返回上一页重试！";
                    x.RedirectText = "忘记密码";
                    x.RedirectUrl = Url.Action("Index", "Home");
                    x.StatusCode = 403;
                });
        }
    }
}
