﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChinaTelecom.Grid.Lib
{
    public class Point
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public static class Circle
    {
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
