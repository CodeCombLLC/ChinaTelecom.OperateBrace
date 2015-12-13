using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChinaTelecom.Grid.Models;
using ChinaTelecom.Grid.ViewModels;

namespace ChinaTelecom.Grid.Lib
{
    public static class Counting
    {
        public static UserStatistics Count(IEnumerable<Record>src1, IEnumerable<House> src2)
        {
            return new UserStatistics
            {
                CTUsers = src2.Where(x => x.HouseStatus == Models.HouseStatus.中国电信).Count(),
                CTUsingUsers = src2.Where(x => x.HouseStatus == Models.HouseStatus.中国电信 && x.ServiceStatus == Models.ServiceStatus.在用).Count(),
                NonCTUsers = src2.Where(x => x.HouseStatus != Models.HouseStatus.中国电信 && x.HouseStatus != Models.HouseStatus.未装机).Count(),
                Added = src2.Where(x => x.HouseStatus == Models.HouseStatus.中国电信 && x.ServiceStatus == Models.ServiceStatus.在用 && x.IsStatusChanged).Count(),
                Left = src2.Where(x => x.HouseStatus == Models.HouseStatus.中国电信 && x.ServiceStatus != Models.ServiceStatus.在用 && x.IsStatusChanged).Count(),
                NoRelation = src1.Where(x => !src2.Select(y => y.Account).Contains(x.Account)).Count()
            };
        }
    }
}
