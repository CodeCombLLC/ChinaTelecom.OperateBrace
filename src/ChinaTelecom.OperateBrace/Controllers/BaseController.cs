using System.Linq;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using ChinaTelecom.OperateBrace.Models;

namespace ChinaTelecom.OperateBrace.Controllers
{
    public class BaseController : BaseController<GridContext, User, string>
    {
        public override void Prepare()
        {
            base.Prepare();
            ViewBag.HeaderRecordCount = DB.Records.Count();
            ViewBag.HeaderUserCount = DB.Users.Count();
            ViewBag.HeaderEstateCount = DB.Estates.Count();
            var ha = DB.Houses
                .Select(y => y.Account)
                .Distinct();
            var noRelationCustomerCount = DB.Records
                .AsNoTracking()
                .Select(x => x.Account)
                .Distinct()
                .Where(x => !ha.Contains(x))
                .Count();
            ViewBag.NoRelationCustomerCount = noRelationCustomerCount;
        }
    }
}
