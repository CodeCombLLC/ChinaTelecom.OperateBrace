using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChinaTelecom.OperateBrace.ViewModels
{
    public class Area
    {
        public string Id { get; set; }
        public int Count { get; set; }
        public int CTUsers { get; set; }
        public int CTInUsingUsers { get; set; }
        public int NonCTUsers { get; set; }
        public int AddedUsers { get; set; }
        public int LeftUsers { get; set; }
        public double? Lon { get; set; }
        public double? Lat { get; set; }
    }
}
