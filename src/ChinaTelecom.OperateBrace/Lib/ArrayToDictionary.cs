using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChinaTelecom.OperateBrace.Lib
{
    public static class ArrayToDictionary
    {
        public static Dictionary<int, int> Parse(int[] src)
        {
            var ret = new Dictionary<int, int>();
            for(var i = 1; i <= src.Count(); i++)
                ret.Add(i, src[i - 1]);
            return ret;
        }
    }
}
