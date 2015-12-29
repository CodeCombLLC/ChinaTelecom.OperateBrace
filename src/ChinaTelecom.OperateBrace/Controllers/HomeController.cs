using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Authorization;
using Microsoft.Data.Entity;
using ChinaTelecom.OperateBrace.ViewModels;

namespace ChinaTelecom.OperateBrace.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        public IActionResult Index()
        {
            ViewBag.UserStatistics = Lib.Counting.Count(DB.Records.AsNoTracking(), DB.Houses);
            ViewBag.SetStatistics = DB.Records
                .AsNoTracking()
                .Where(x => x.Status == Models.ServiceStatus.在用)
                .OrderByDescending(x => x.ImportedTime)
                .DistinctBy(x => x.Account)
                .GroupBy(x => x.Set)
                .Select(x => new BarChartItem
                {
                    Key = x.Key,
                    Count = x.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(30)
                .ToList();

            var area = DB.Estates
                .AsNoTracking()
                .DistinctBy(x => x.Area)
                .Select(x => x.Area).ToList();
            var areaStatistics = new List<BarChartItem>();
            foreach(var x in area)
            {
                areaStatistics.Add(new BarChartItem
                {
                    Key = x,
                    Count = DB.Houses
                        .Include(a => a.Building)
                        .ThenInclude(a => a.Estate)
                        .Where(a => a.Building.Estate.Area == x && a.HouseStatus == Models.HouseStatus.中国电信 && (a.HardlinkStatus == Models.ServiceStatus.在用 || a.MobileStatus == Models.ServiceStatus.在用))
                        .Count()
                });
            }
            ViewBag.AreaStatistics = areaStatistics;

            ViewBag.ContractorStatistics = DB.Records
                .AsNoTracking()
                .Where(x => x.Status == Models.ServiceStatus.在用)
                .OrderByDescending(x => x.ImportedTime)
                .DistinctBy(x => x.Account)
                .GroupBy(x => x.ContractorName)
                .Select(x => new BarChartItem
                {
                    Key = x.Key,
                    Count = x.Count()
                })
                .ToList();
            return View();
        }

        public IActionResult Skin(string style)
        {
            Cookies["styles"] = style;
            return Content("ok");
        }
    }
}
