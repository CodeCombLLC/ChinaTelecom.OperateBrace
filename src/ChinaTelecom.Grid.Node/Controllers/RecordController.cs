using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;

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
    }
}
