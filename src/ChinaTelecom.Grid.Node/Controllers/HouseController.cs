using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;

namespace ChinaTelecom.Grid.Node.Controllers
{
    public class HouseController : BaseController
    {
        [HttpGet]
        public long Count()
        {
            return DB.Houses
                .LongCount();
        }

        [HttpGet]
        [Route("house/get-no-relation-customer-count")]
        public long GetNoRelationCustomerCount()
        {
            var ha = DB.Houses
                .Select(x => x.Account)
                .Distinct();

            var noRelationCustomerCount = DB.Records
               .AsNoTracking()
               .Select(x => x.Account)
               .Distinct()
               .Where(x => !ha.Contains(x))
               .LongCount();

            return noRelationCustomerCount;
        }
    }
}
