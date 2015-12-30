using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChinaTelecom.OperateBrace.Lib
{
    public static class HouseCounter
    {
        public static long Caculate(Dictionary<int, int> src, int top, int bottom)
        {
            var layers = top - bottom;
            if (bottom > 0)
                layers++;
            if (top < 0)
                layers++;
            return src.Sum(x => x.Value) * layers;
        }
    }
}
