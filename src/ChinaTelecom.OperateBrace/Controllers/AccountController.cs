using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Authorization;
using ChinaTelecom.OperateBrace.Models;
using ChinaTelecom.OperateBrace.ViewModels;

namespace ChinaTelecom.OperateBrace.Controllers
{
    [Authorize]
    public class AccountController : BaseController
    {
        [HttpGet]
        [AnyRoles("系统管理员")]
        public IActionResult Index(string name, string username)
        {
            var ret = UserManager.Users;
            if (!string.IsNullOrEmpty(name))
                ret = ret.Where(x => x.FullName.Contains(name) || name.Contains(x.FullName));
            if (!string.IsNullOrEmpty(username))
                ret = ret.Where(x => x.UserName.Contains(username) || username.Contains(x.UserName));
            return PagedView(ret);
        }

        [HttpPost]
        [AnyRoles("系统管理员")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(string id)
        {
            await UserManager.DeleteAsync(await UserManager.FindByIdAsync(id));
            return Prompt(x =>
            {
                x.Title = "删除成功";
                x.Details = "该用户已成功删除！";
            });
        }

        [HttpPost]
        [AnyRoles("系统管理员")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string name, string pwd, string role, string fullname, string area)
        {
            var user = new User { UserName = name, FullName = fullname };
            await UserManager.CreateAsync(user, pwd);
            await UserManager.AddToRoleAsync(user, role);
            await UserManager.AddClaimAsync(user, new System.Security.Claims.Claim("管辖片区", string.Empty));
            foreach (var x in area.TrimEnd(' ').TrimEnd(',').Split(','))
            {
                await UserManager.AddClaimAsync(user, new System.Security.Claims.Claim("管辖片区", x));
            }
            return RedirectToAction("Index", "Account");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
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
                    .Select(x => x.Set)
                    .ToList()
                    .GroupBy(x => x)
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
                if (!string.IsNullOrEmpty(x))
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
            user.FullName = FullName;
            var claims = (await UserManager.GetClaimsAsync(user)).Select(x => x.Value);
            foreach (var x in claims)
                await UserManager.RemoveClaimAsync(user, new System.Security.Claims.Claim("管辖片区", x));
            if (!string.IsNullOrEmpty(Area))
                foreach (var x in Area.TrimEnd(' ').TrimEnd(',').Split(','))
                    await UserManager.AddClaimAsync(user, new System.Security.Claims.Claim("管辖片区", x));
            if (!string.IsNullOrEmpty(NewPwd))
            {
                var token = await UserManager.GeneratePasswordResetTokenAsync(user);
                await UserManager.ResetPasswordAsync(user, token, NewPwd);
            }
            
            await UserManager.AddClaimAsync(user, new System.Security.Claims.Claim("管辖片区", string.Empty));
            var role = await UserManager.GetRolesAsync(user);
            await UserManager.RemoveFromRoleAsync(user, role.First());
            await UserManager.AddToRoleAsync(user, Role);
            DB.SaveChanges();
            return RedirectToAction("Profile", "Account", new { id = id });
        }

        [HttpGet]
        public IActionResult Setting()
        {
            return View(User.Current);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Setting(string oldpwd, string newpwd, string confirmpwd, IFormFile file)
        {
            var user = await UserManager.FindByIdAsync(User.Current.Id);
            if (confirmpwd != newpwd)
                return Prompt(x =>
                {
                    x.Title = "操作失败";
                    x.Details = "两次密码输入不一致，请返回重试！";
                });
            if (file != null)
                user.Avatar = await file.ReadAllBytesAsync();
            if (!string.IsNullOrEmpty(oldpwd) && await UserManager.CheckPasswordAsync(user, oldpwd))
                await UserManager.ChangePasswordAsync(user, oldpwd, newpwd);
            DB.SaveChanges();
            return RedirectToAction("Profile", "Account", new { id = User.Current.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Avatar(string id)
        {
            var user = await UserManager.FindByIdAsync(User.Current.Id);
            return File(user.Avatar, "image/x-png");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await SignInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
