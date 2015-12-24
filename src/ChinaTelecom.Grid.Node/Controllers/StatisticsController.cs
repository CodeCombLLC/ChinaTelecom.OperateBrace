using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using ChinaTelecom.Grid.SharedModels;

namespace ChinaTelecom.Grid.Node.Controllers
{
    public class StatisticsController : BaseController
    {
        [HttpGet]
        [Route("statistics/profile-general")]
        public IActionResult ProfileGeneral(string fullname)
        {
            var statistics = new List<BarChartItem>();
            statistics.Add(new BarChartItem
            {
                Key = "在用",
                Count = DB.Records
                    .Where(x => x.ContractorName == fullname || x.ServiceStaff == fullname && x.Status == ServiceStatus.在用)
                    .OrderByDescending(x => x.ImportedTime)
                    .DistinctBy(x => x.Account)
                    .Count()
            });

            statistics.Add(new BarChartItem
            {
                Key = "非在用",
                Count = DB.Records
                    .Where(x => x.ContractorName == fullname || x.ServiceStaff == fullname && x.Status != ServiceStatus.在用)
                    .OrderByDescending(x => x.ImportedTime)
                    .DistinctBy(x => x.Account)
                    .Count()
            });

            statistics.Add(new BarChartItem
            {
                Key = "未对应至楼宇",
                Count = DB.Records
                    .Where(x => x.ContractorName == fullname 
                        || x.ServiceStaff == fullname 
                        && !DB.Houses
                            .Select(y => y.Account)
                            .Contains(x.Account))
                    .OrderByDescending(x => x.ImportedTime)
                    .DistinctBy(x => x.Account)
                    .Count()
            });

            return Json(statistics);
        }

        [HttpGet]
        [Route("statistics/profile-set")]
        public IActionResult ProfileSet(string fullname)
        {
            var ret = DB.Records
                    .Where(x => x.ContractorName == fullname || x.ServiceStaff == fullname && x.Status == ServiceStatus.在用)
                    .Select(x => x.Set)
                    .ToList()
                    .GroupBy(x => x)
                    .Select(x => new BarChartItem
                    {
                        Key = x.Key,
                        Count = x.Count()
                    })
                    .ToList();
            return Json(ret);
        }
    }
}
