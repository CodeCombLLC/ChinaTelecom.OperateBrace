using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChinaTelecom.OperateBrace.Models;
using ChinaTelecom.OperateBrace.ViewModels;

namespace ChinaTelecom.OperateBrace.Lib
{
    public static class Counting
    {
        public static UserStatistics Count(IQueryable<Record> src1, IQueryable<House> src2)
        {
            var ha = src2
                .Select(y => y.Account)
                .Distinct();
            var cnt = src1.Select(x => x.Account)
                    .Distinct()
                    .Where(x => !ha.Contains(x))
                    .Count();
            return new UserStatistics
            {
                CTUsers = src2.Where(x => x.HouseStatus == Models.HouseStatus.中国电信).Count(),
                CTUsingUsers = src2.Where(x => x.HouseStatus == Models.HouseStatus.中国电信 && (x.HardlinkStatus == Models.ServiceStatus.在用 || x.MobileStatus == Models.ServiceStatus.在用)).Count(),
                NonCTUsers = src2.Where(x => x.HouseStatus != Models.HouseStatus.中国电信 && x.HouseStatus != Models.HouseStatus.未装机).Count(),
                Added = src2.Where(x => x.HouseStatus == Models.HouseStatus.中国电信 && (x.HardlinkStatus == Models.ServiceStatus.在用 || x.MobileStatus == Models.ServiceStatus.在用) && x.IsStatusChanged).Count(),
                Left = src2.Where(x => x.HouseStatus == Models.HouseStatus.中国电信 && (x.HardlinkStatus != Models.ServiceStatus.在用 || x.MobileStatus != Models.ServiceStatus.在用) && x.IsStatusChanged).Count(),
                NoRelation = cnt
            };
        }

        public static UserStatistics Count(IEnumerable<Record> src1, IEnumerable<House> src2)
        {
            var ha = src2
                .Select(y => y.Account)
                .Distinct();
            var cnt = src1.Select(x => x.Account)
                    .Distinct()
                    .Where(x => !ha.Contains(x))
                    .Count();
            return new UserStatistics
            {
                CTUsers = src2.Where(x => x.HouseStatus == Models.HouseStatus.中国电信).Count(),
                CTUsingUsers = src2.Where(x => x.HouseStatus == Models.HouseStatus.中国电信 && (x.HardlinkStatus == Models.ServiceStatus.在用 || x.MobileStatus == Models.ServiceStatus.在用)).Count(),
                NonCTUsers = src2.Where(x => x.HouseStatus != Models.HouseStatus.中国电信 && x.HouseStatus != Models.HouseStatus.未装机).Count(),
                Added = src2.Where(x => x.HouseStatus == Models.HouseStatus.中国电信 && (x.HardlinkStatus == Models.ServiceStatus.在用 || x.MobileStatus == Models.ServiceStatus.在用) && x.IsStatusChanged).Count(),
                Left = src2.Where(x => x.HouseStatus == Models.HouseStatus.中国电信 && (x.HardlinkStatus != Models.ServiceStatus.在用 || x.MobileStatus != Models.ServiceStatus.在用) && x.IsStatusChanged).Count(),
                NoRelation = cnt
            };
        }
    }
}
