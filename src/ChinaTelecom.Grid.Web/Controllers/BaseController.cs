using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using ChinaTelecom.Grid.SharedModels;
using ChinaTelecom.Grid.Web.Models;
using ChinaTelecom.Grid.Caller;

namespace ChinaTelecom.Grid.Web.Controllers
{
    public class BaseController : BaseController<GridContext, User, string>
    {
        public override void Prepare()
        {
            base.Prepare();
            var recordCount = 0UL;
            DB.NodeServers
                .GetPrimaryNodes()
                .ToClients(1000)
                .GetAsync("/record/count", x => 
                {
                    recordCount += x.Result;
                })
                .Wait();
            var estateCount = 0UL;
            DB.NodeServers
                .GetPrimaryNodes()
                .ToClients(1000)
                .GetAsync("/estate/count", x =>
                {
                    estateCount += x.Result;
                })
                .Wait();
            var houseCount = 0UL;
            DB.NodeServers
                .GetPrimaryNodes()
                .ToClients(1000)
                .GetAsync("/house/count", x =>
                {
                    houseCount += x.Result;
                })
                .Wait();
            var noRelationCustomerCount = 0UL;
            DB.NodeServers
                .GetPrimaryNodes()
                .ToClients(1000)
                .GetAsync("/house/get-no-relation-customer-count", x =>
                {
                    noRelationCustomerCount += x.Result;
                })
                .Wait();
            ViewBag.HeaderRecordCount = recordCount;
            ViewBag.HeaderEstateCount = estateCount;
            ViewBag.NoRelationCustomerCount = noRelationCustomerCount;
            ViewBag.HeaderUserCount = DB.Users.Count();
        }
    }
}
