using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Authorization;
using ChinaTelecom.Grid.ViewModels;

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

        [Authorize]
        public async Task<IActionResult> Profile(string id)
        {
            var user = await UserManager.FindByIdAsync(id);
            var statistics = new List<BarChartItem>();
            statistics.Add(new BarChartItem
            {
                Key = "在用",
                Count = DB.Records
                    .Where(x => x.ContractorName == user.FullName || x.ServiceStaff == user.FullName && x.Status == Models.ServiceStatus.在用)
                    .OrderByDescending(x => x.ImportedTime)
                    .DistinctBy(x => x.Account)
                    .Count()
            });

            statistics.Add(new BarChartItem
            {
                Key = "非在用",
                Count = DB.Records
                    .Where(x => x.ContractorName == user.FullName || x.ServiceStaff == user.FullName && x.Status != Models.ServiceStatus.在用)
                    .OrderByDescending(x => x.ImportedTime)
                    .DistinctBy(x => x.Account)
                    .Count()
            });

            statistics.Add(new BarChartItem
            {
                Key = "未对应至楼宇",
                Count = DB.Records
                    .Where(x => x.ContractorName == user.FullName || x.ServiceStaff == user.FullName && !DB.Houses.Select(y => y.Account).Contains(x.Account))
                    .OrderByDescending(x => x.ImportedTime)
                    .DistinctBy(x => x.Account)
                    .Count()
            });
            
            ViewBag.UserStatistics = statistics;
            ViewBag.SetStatistics = DB.Records
                    .Where(x => x.ContractorName == user.FullName || x.ServiceStaff == user.FullName && x.Status == Models.ServiceStatus.在用)
                    .GroupBy(x => x.Set)
                    .Select(x => new BarChartItem
                    {
                        Key = x.Key,
                        Count = x.Count()
                    })
                    .ToList();
            return View(user);
        }
    }
}
