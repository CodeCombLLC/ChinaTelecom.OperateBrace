using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChinaTelecom.OperateBrace.Lib
{
    public static class AddressAnalyser
    {
        private const string template = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM-1234567890";
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

        public static int? GetLayer(string src)
        {
            return GetLayer(GetNumbers(FilterBrackets(src)), src);
        }

        private static int? GetLayer(List<string> src, string src2)
        {
            if (src.Count < 2)
                return null;
            var tmp = src.Last();
            int? ret = null;
            if (tmp.Length == 4)
                return Convert.ToInt32(tmp.Substring(0, 2));
            else if (tmp.Length == 3)
            {
                if (tmp[1] == '0')
                    ret = Convert.ToInt32(tmp.Substring(0, 1));
                else
                    ret = Convert.ToInt32(tmp.Substring(0, 2));
            }
            else if (src[src.Count - 2].Length == 1 || src[src.Count - 2].Length == 2)
                ret = Convert.ToInt32(src[src.Count - 2]);
            else
                ret = null;
            if (ret.HasValue)
            {
                if (src2.IndexOf("B" + ret) >= 0 || src2.IndexOf("负" + ret) >= 0)
                    return -ret.Value;
                else
                    return ret.Value;
            }
            else
            {
                return null;
            }
        }

        public static int? GetDoor(string src)
        {
            return GetDoor(GetNumbers(FilterBrackets(src)));
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

        public static int? GetUnit(string src)
        {
            src = FilterBrackets(src);
            if (string.IsNullOrEmpty(src))
                return null;
            var tmp = src.Split("单元".ToArray());
            if (tmp.Count() == 1)
                return 1;
            var tmp2 = tmp[0];
            var result = GetNumbers(tmp2);
            return Convert.ToInt32(result.LastOrDefault() ?? "1");
        }

        public static string GetBuildingNumber(string src)
        {
            src = FilterBrackets(src);
            var splitChar = "";
            if (src.Contains("号楼"))
            {
                splitChar = "号楼";
            }
            else if (src.Contains("栋"))
            {
                splitChar = "栋";
            }
            else if (src.Contains("幢"))
            {
                splitChar = "幢";
            }
            else if (src.Contains("座"))
            {
                splitChar = "座";
            }
            if (string.IsNullOrEmpty(splitChar))
                return "1";
            var tmp = src.Split(splitChar.ToArray());
            if (tmp.Count() >= 2)
            {
                var tmp2 = GetNumbers(tmp[0]);
                return tmp2.Last();
            }
            else
            {
                return "1";
            }
        }

        public static string GetCity(string src)
        {
            if (src.IndexOf("市") < 0)
                return null;
            var ret = "";
            foreach(var x in src)
            {
                if (x != '市')
                    ret += x;
                else
                    break;
            }
            return ret + "市";
        }

        public static string GetDistrict(string src)
        {
            var city = GetCity(src);
            if (city != null)
                src = src.Replace(city, "");
            if (src.IndexOf("区") < 0)
                return null;
            var ret = "";
            foreach (var x in src)
            {
                if (x != '区')
                    ret += x;
                else
                    break;
            }
            return ret + "区";
        }

        public static string GetEstate(string src)
        {
            var city = GetCity(src);
            if (city != null)
                src = src.Replace(city, "");
            var district = GetDistrict(src);
            if (district != null)
                src = src.Replace(district, "");
            var ret = "";
            src = src.Replace("小区", "@")
                .Replace("号楼", "$")
                .Replace("号", "%");
            if (src.IndexOf("%") >= 0 && src.IndexOf("@") < 0)
                return src.Substring(0, src.IndexOf("%")) + "号";

            foreach (var x in src)
            {
                if (template.Contains(x))
                    break;
                else if (x == '@')
                    break;
                else
                    ret += x;
            }
            if (!string.IsNullOrEmpty(ret))
            {
                if (src.IndexOf("@") >= 0)
                    return ret + "小区";
                else
                    return ret;
            }
            return null;
        }
    }
}
