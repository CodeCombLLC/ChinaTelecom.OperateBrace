using System.Linq;
using Microsoft.AspNet.Mvc;
using ChinaTelecom.Grid.Models;

namespace ChinaTelecom.Grid.Controllers
{
    public class BaseController : BaseController<GridContext, User, string>
    {
        public override void Prepare()
        {
            base.Prepare();
            ViewBag.HeaderRecordCount = DB.Records.Count();
            ViewBag.HeaderUserCount = DB.Users.Count();
            ViewBag.HeaderEstateCount = DB.Estates.Count();
        }
    }
}
