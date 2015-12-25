using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using ChinaTelecom.Grid.SharedModels;

namespace ChinaTelecom.Grid.Web.Controllers
{
    public class RecordController : BaseController
    {
        public IActionResult Index(string City, string BusinessHall, string ContractorName, string StaffName, ServiceStatus? Status, string Name, string Address, string Account, string Set, string Phone, bool? xls, bool? info, Guid? SeriesId, DateTime? BeginTime, DateTime? EndTime)
        {
            
        }
    }
}
