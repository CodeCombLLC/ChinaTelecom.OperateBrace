using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using ChinaTelecom.Grid.Models;

namespace ChinaTelecom.Grid.Controllers
{
    public class RecordController : BaseController
    {
        public IActionResult Index(string ContractorName, ServiceStatus? Status, string Address, string Set, string raw)
        {
            IEnumerable<Record> ret = DB.Records.AsNoTracking();
            if (!string.IsNullOrEmpty(ContractorName))
                ret = ret.Where(x => x.ContractorName == ContractorName);
            if (Status.HasValue)
                ret = ret.Where(x => x.Status == Status);
            if (!string.IsNullOrEmpty(Set))
                ret = ret.Where(x => x.Set == Set);
            if (!string.IsNullOrEmpty(Address))
                ret = ret.Where(x => x.ImplementAddress.Contains(Address) || x.StandardAddress.Contains(Address));

            if (raw != "true")
            {
                ViewBag.Statuses = DB.Records.Select(x => x.Status.ToString()).Distinct().ToList();
                ViewBag.ContractorNames = DB.Records.Select(x => x.ContractorName).Distinct().ToList();
                ViewBag.Sets = DB.Records.Select(x => x.Set).Distinct().ToList();
                return PagedView(ret);
            }
            else
            {
                return XlsView(ret.ToList(), "chinatelecom.xls", "Xls");
            }
        }
    }
}
