using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChinaTelecom.OperateBrace.Lib
{
    public class Point
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class Edge
    {
        public double MinLon { get; set; }
        public double MaxLon { get; set; }
        public double MinLat { get; set; }
        public double MaxLat { get; set; }
    }

    public static class Circle
    {
        public static Edge GetEdge(Point[] src)
        {
            if (src.Count() == 1)
            {
                return new Edge {
                    MinLon = src.First().X,
                    MaxLon = src.First().X,
                    MinLat = src.First().Y,
                    MaxLat = src.First().Y
                };
            }
            else if (src.Count() > 1)
            {
                var ret = new Edge
                {
                    MinLon = src.First().X,
                    MaxLon = src.First().X,
                    MinLat = src.First().Y,
                    MaxLat = src.First().Y
                };
                for (var i =1; i < src.Count(); i++)
                {
                    ret.MinLon = Math.Min(ret.MinLon, src[i].X);
                    ret.MaxLon = Math.Max(ret.MaxLon, src[i].X);
                    ret.MinLat = Math.Min(ret.MinLat, src[i].Y);
                    ret.MaxLat = Math.Max(ret.MaxLat, src[i].Y);
                }
                return ret;
            }
            else
            {
                return null;
            }
        }

        public static Point[] StringToPoints(string src)
        {
            var ret = new List<Point>();
            foreach(var x in src.Trim(',').Split(','))
            {
                ret.Add(new Point
                {
                    X = Convert.ToDouble(x.Split('|')[0]),
                    Y = Convert.ToDouble(x.Split('|')[1])
                });
            }
            return ret.ToArray();
        }

        private static double isLeft(Point P0, Point P1, Point P2)
        {
            var ret = ((P1.X - P0.X) * (P2.Y - P0.Y) - (P2.X - P0.X) * (P1.Y - P0.Y));
            return ret;
        }

        public static bool PointInFences(Point pnt1, Point[] fencePnts)
        {

            int wn = 0, j = 0; 
            for (int i = 0; i < fencePnts.Length; i++)
            {  
                if (i == fencePnts.Length - 1)
                    j = 0;
                else
                    j = j + 1; 


                if (fencePnts[i].Y <= pnt1.Y) 
                {
                    if (fencePnts[j].Y > pnt1.Y)
                    {
                        if (isLeft(fencePnts[i], fencePnts[j], pnt1) > 0)
                        {
                            wn++;
                        }
                    }
                }
                else
                {
                    if (fencePnts[j].Y <= pnt1.Y)
                    {
                        if (isLeft(fencePnts[i], fencePnts[j], pnt1) < 0)
                        {
                            wn--;
                        }
                    }
                }
            }
            if (wn == 0)
                return false;
            else
                return true;
        }
    }
}
