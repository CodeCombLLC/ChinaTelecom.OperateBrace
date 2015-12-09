using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.OleDb;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Http;
using Microsoft.Data.Entity;
using ChinaTelecom.Grid.Models;

namespace ChinaTelecom.Grid.Controllers
{
    public class RecordController : BaseController
    {
        public IActionResult Index(string ContractorName, ServiceStatus? Status, string Address, string Set, string Phone, string raw)
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
            if (!string.IsNullOrEmpty(Phone))
                ret = ret.Where(x => x.Phone.Contains(Phone));
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

        public IActionResult Import(IFormFile file)
        {
            var fname = Guid.NewGuid().ToString().Replace("-", "") + System.IO.Path.GetExtension(file.GetFileName());
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), fname);
            file.SaveAs(path);
            string connStr;
            if (System.IO.Path.GetExtension(path) == ".xls")
                connStr = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + path + ";" + ";Extended Properties=\"Excel 8.0;HDR=YES;IMEX=1\"";
            else
                connStr = "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + path + ";" + ";Extended Properties=\"Excel 12.0;HDR=YES;IMEX=1\"";
            using (var conn = new OleDbConnection(connStr))
            {
                conn.Open();

            }
            return View();
        }
    }
}
