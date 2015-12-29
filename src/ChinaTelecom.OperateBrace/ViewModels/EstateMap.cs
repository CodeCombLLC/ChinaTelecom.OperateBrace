using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChinaTelecom.OperateBrace.ViewModels
{
    public class EstateMap
    {
        public string Title { get; set; }
        
        public string Area { get; set; }

        public double UsingRate { get; set; }
        
        public long TotalCTUsers { get; set; }
        
        public long TotalNonCTUsers { get; set; }
        
        public long TotalInUsingUsers { get; set; }
        
        public long AddedUsers { get; set; }
        
        public long LeftUsers { get; set; }
    }
}
