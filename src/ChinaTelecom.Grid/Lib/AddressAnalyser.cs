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

        public static int? GetLayer(List<string> src)
        {
            if (src.Count < 2)
                return null;
            var tmp = src.Last();
            if (tmp.Length == 4)
                return Convert.ToInt32(tmp.Substring(0, 2));
            else if (tmp.Length == 3)
                return Convert.ToInt32(tmp.Substring(0, 1));
            else if (src[src.Count - 2].Length == 1 || src[src.Count - 2].Length == 2)
                return Convert.ToInt32(src[src.Count - 2]);
            else
                return null;
        }

        public static int? GetDoor(List<string> src)
        {
            if (src.Count < 2)
                return null;
            var tmp = src.Last();
            var ret = "";
            if (tmp.Length == 4)
                ret = tmp.Substring(2, 2);
            else if (tmp.Length == 3)
                ret = tmp.Substring(2, 1);
            else if (tmp.Length == 1)
                ret = tmp;
            else
                return null;
            ret = ret.Replace("A", "1")
                .Replace("B", "2")
                .Replace("C", "3")
                .Replace("D", "4")
                .Replace("E", "5");
            return Convert.ToInt32(ret);
        }
    }
}
