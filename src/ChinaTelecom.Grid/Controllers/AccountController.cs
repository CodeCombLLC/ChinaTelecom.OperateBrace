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

        [HttpGet]
        [AnyRoles("系统管理员")]
        public async Task<IActionResult> Modify(string id)
        {
            var user = await UserManager.FindByIdAsync(id);
            var claims = (await UserManager.GetClaimsAsync(user)).Select(x => x.Value);
            var areastr = "";
            foreach(var x in claims)
            {
                areastr += x + ", ";
            }
            ViewBag.Area = areastr.TrimEnd(' ').TrimEnd(',');
            return View(user);
        }

        [HttpPost]
        [AnyRoles("系统管理员")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Modify(string id, string FullName, string NewPwd, string Area, string Role)
        {
            var user = await UserManager.FindByIdAsync(id);
            var claims = (await UserManager.GetClaimsAsync(user)).Select(x => x.Value);
            foreach (var x in claims)
                await UserManager.RemoveClaimAsync(user, new System.Security.Claims.Claim("管辖片区", x));
            foreach (var x in Area.TrimEnd(' ').TrimEnd(',').Split(','))
                await UserManager.AddClaimAsync(user, new System.Security.Claims.Claim("管辖片区", x));
            if (!string.IsNullOrEmpty(NewPwd))
            {
                var token = await UserManager.GeneratePasswordResetTokenAsync(user);
                await UserManager.ResetPasswordAsync(user, token, NewPwd);
            }
            var role = await UserManager.GetRolesAsync(user);
            await UserManager.RemoveFromRoleAsync(user, role.First());
            await UserManager.AddToRoleAsync(user, Role);
            return RedirectToAction("Profile", "Account", new { id = id });
        }
    }
}
