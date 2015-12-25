using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using ChinaTelecom.Grid.SharedModels;

namespace ChinaTelecom.Grid.Node.Controllers
{
    public class RecordController : BaseController
    {
        [HttpGet]
        public long Count()
        {
            return DB.Records
                .LongCount();
        }

        public IActionResult DetailFilter(string ContractorName, string StaffName, ServiceStatus? Status, string Name, string Address, string Account, string Set, string Phone, bool? xls, bool? info, Guid? SeriesId, DateTime? BeginTime, DateTime? EndTime, int p = 1)
        {
            IEnumerable<Record> ret = DB.Records
                .AsNoTracking();
            if (Status.HasValue)
                ret = ret.Where(x => x.Status == Status);
            if (!string.IsNullOrEmpty(Set))
                ret = ret.Where(x => x.Set == Set);
            if (!string.IsNullOrEmpty(Address))
                ret = ret.Where(x => x.ImplementAddress.Contains(Address) || x.StandardAddress.Contains(Address));
            if (!string.IsNullOrEmpty(ContractorName))
                ret = ret.Where(x => x.ContractorName == ContractorName);
            if (!string.IsNullOrEmpty(StaffName))
                ret = ret.Where(x => x.ServiceStaff == StaffName);
            if (!string.IsNullOrEmpty(Name))
                ret = ret.Where(x => x.CustomerName == Name);
            if (!string.IsNullOrEmpty(Phone))
                ret = ret.Where(x => x.Phone.Contains(Phone));
            if (!string.IsNullOrEmpty(Account))
                ret = ret.Where(x => x.Account == Account);
            if (SeriesId.HasValue)
                ret = ret.Where(x => x.SeriesId == SeriesId.Value);
            if (BeginTime.HasValue)
                ret = ret.Where(x => x.ImportedTime >= BeginTime.Value);
            if (EndTime.HasValue)
            {
                EndTime = EndTime.Value.AddDays(1);
                ret = ret.Where(x => x.ImportedTime < EndTime.Value);
            }
            ret = ret.OrderByDescending(x => x.ImportedTime);
            if (info.HasValue && info.Value)
                return Json(Pager.GetPagerInfo(ref ret, 50, p));
            if (xls.HasValue && xls.Value)
                return XlsView(ret.ToList(), "details");
            return PagedJson(ret);
        }
    }
}
