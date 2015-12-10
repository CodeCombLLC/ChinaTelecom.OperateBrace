using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Authorization;
using ChinaTelecom.Grid.Models;
using ChinaTelecom.Grid.ViewModels;

namespace ChinaTelecom.Grid.Controllers
{
    [Authorize]
    public class GridController : BaseController
    {
        public async Task<IActionResult> Index()
        {
            ViewBag.Areas = (await UserManager.GetClaimsAsync(User.Current)).Where(x => x.Type == "管辖片区").Select(x => x.Value).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateEstate(string title, double lon, double lat, string rules, string area)
        {
            var estate = new Estate
            {
                Area = area,
                Lat = lat,
                Lon = lon,
                Title = title
            };
            DB.Estates.Add(estate);
            if (!string.IsNullOrEmpty(rules))
                foreach (var x in rules.Split('\n'))
                    DB.EstateRules.Add(new EstateRule { Rule = x, EstateId = estate.Id });
            DB.SaveChanges();
            return Content("ok");
        }

        [HttpPost]
        public IActionResult ModifyPosition(Guid id, double lon, double lat)
        {
            var estate = DB.Estates.Where(x => x.Id == id).Single();
            estate.Lon = lon;
            estate.Lat = lat;
            DB.SaveChanges();
            return Content("ok");
        }

        [HttpGet]
        public IActionResult GetEstates(double left, double right, double top, double bottom)
        {
            var estates = DB.Estates.Where(x => x.Lon >= left && x.Lon <= right && x.Lat <= top && x.Lat >= bottom);
            foreach(var x in estates)
            {
                var tmp = DB.Houses
                    .Include(y => y.Building)
                    .Where(y => y.Building.EstateId == x.Id && y.HouseStatus == HouseStatus.中国电信);
                x.TotalCTUsers = tmp.Count();
                x.TotalInUsingUsers = tmp.Where(y => y.ServiceStatus == ServiceStatus.在用).Count();
                x.TotalNonCTUsers = DB.Houses
                    .Include(y => y.Building)
                    .Where(y => y.Building.EstateId == x.Id && y.HouseStatus != HouseStatus.中国电信)
                    .Count();
                if (x.TotalCTUsers == 0)
                    x.UsingRate = 0;
                else
                    x.UsingRate = (double)x.TotalInUsingUsers / (double)x.TotalCTUsers;
            }
            return Json(estates);
        }

        [HttpGet]
        public async Task<IActionResult> Area()
        {
            IEnumerable<Estate> tmp = DB.Estates
                .Include(x => x.Buildings)
                .ThenInclude(x => x.Houses);
            if (!User.IsInRole("系统管理员"))
            {
                var areas = (await UserManager.GetClaimsAsync(User.Current)).Where(x => x.Type == "管辖片区").Select(x => x.Value).ToList();
                tmp = tmp.Where(x => areas.Contains(x.Area));
            }
            var ret = tmp.OrderBy(x => x.Area)
                .GroupBy(x => x.Area)
                .Select(x => new Area
                {
                    Id = x.Key,
                    Buildings = x.Count(),
                    CTUsers = x.Sum(y => y.Buildings.Sum(z => z.Houses.Where(a => a.HouseStatus == HouseStatus.中国电信).Count())),
                    CTInUsingUsers = x.Sum(y => y.Buildings.Sum(z => z.Houses.Where(a => a.HouseStatus == HouseStatus.中国电信 && a.ServiceStatus == ServiceStatus.在用).Count())),
                    NonCTUsers = x.Sum(y => y.Buildings.Sum(z => z.Houses.Where(a => a.HouseStatus != HouseStatus.中国电信).Count())),
                    AddedUsers = x.Sum(y => y.Buildings.Sum(z => z.Houses.Where(a => a.HouseStatus == HouseStatus.中国电信 && a.IsStatusChanged && a.ServiceStatus == ServiceStatus.在用).Count())),
                    LeftUsers = x.Sum(y => y.Buildings.Sum(z => z.Houses.Where(a => a.HouseStatus == HouseStatus.中国电信 && a.IsStatusChanged && a.ServiceStatus != ServiceStatus.在用).Count())),
                    Lon = x.FirstOrDefault() == null ? null : (double?)x.FirstOrDefault().Lon,
                    Lat = x.FirstOrDefault() == null ? null : (double?)x.FirstOrDefault().Lat
                });
            return PagedView(ret);
        }
    }
}
