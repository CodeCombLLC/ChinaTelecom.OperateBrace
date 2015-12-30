using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChinaTelecom.OperateBrace.ViewModels
{
    public class Area
    {
        public string Id { get; set; }
        public long Count { get; set; }
        public long CTUsers { get; set; }
        public long CTInUsingUsers { get; set; }
        public long NonCTUsers { get; set; }
        public long AddedUsers { get; set; }
        public long LeftUsers { get; set; }
        public double? Lon { get; set; }
        public double? Lat { get; set; }
        public long Port { get; set; }
        public double PortRate { get { return Port == 0 ? 1 : CTUsers * 1d / Port; } }
        public double Penetrance { get; set; }
    }
}
