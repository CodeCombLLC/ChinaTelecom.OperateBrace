using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChinaTelecom.Grid.Lib
{
    public static class AddressAnalyser
    {
        public static string FilterBrackets(string src)
        {
            var ret = "";
            var cnt = 0;
            foreach(var x in src)
            {
                if (x == '（' || x == '(')
                    cnt++;
                else if (x == '）' || x == ')')
                    cnt--;
                else if (cnt > 0)
                    continue;
                else if (cnt == 0)
                    ret += x;
            }
            return ret;
        }

        public static List<string> GetNumbers(string src)
        {
            var template = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM-1234567890";
            var ret = new List<string>();
            var tmp = "";
            foreach(var x in src)
            {
                if (template.Contains(x))
                {
                    tmp += x;
                }
                else
                {
                    if (!string.IsNullOrEmpty(tmp))
                        ret.Add(tmp);
                    tmp = "";
                }
            }
            if (!string.IsNullOrEmpty(tmp))
                ret.Add(tmp);
            return ret;
        }
    }
}
