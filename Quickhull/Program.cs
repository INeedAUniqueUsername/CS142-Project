using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using static Quickhull.Program;
namespace Quickhull {
    public class Program {
        public static Vector2 Rotate(in Vector2 v, double delta) {
            var angle = (Angle(v) + delta);
            var length = v.Length();
            return Polar(angle, length);
        }
        public static Vector2 Polar(double angle, double length) {
            return new Vector2((float)(length * Math.Cos(angle)), (float)(length * Math.Sin(angle)));
        }
        public static Point Point(in Vector2 v) {
            return new Point((int)v.X, (int)v.Y);
        }
        public static double Angle(in Vector2 v) {
            var angle = Math.Atan2(v.Y, v.X);
            while(angle < 0) {
                angle += Math.PI * 2;
            }
            return angle;
        }
        public static double Cross(in Vector2 a, in Vector2 b) {
            return a.X * b.Y - a.Y * b.X;
        }

        public static bool RightOf(in Vector2 a, in Vector2 b, in Vector2 p) {
            Vector2 ab = b - a;
            Vector2 ap = p - a;
            return Cross(ap, ab) > 0;
        }
        public static List<Vector2> GetHull(IEnumerable<Vector2> points) {
            Vector2 firstPoint = points.First();
            foreach(var p in points) {
                if (p.X > firstPoint.X) {
                    firstPoint = p;
                }
            }


            List<Vector2> hull = new List<Vector2>();
            hull.Add(firstPoint);

            var sorted = points.AsParallel().OrderBy(p => Angle(p - firstPoint));

            foreach (var p in sorted) {
                if (hull.Count > 1) {
                    //Remove any old edges that no longer work
                    for (int i = hull.Count - 1; i > Math.Max(0, hull.Count - hull.Count); i--) {
                        if (RightOf(hull[i - 1], hull[i], p)) {
                            hull.RemoveAt(i);
                        } else {
                        }
                    }
                }

                hull.Add(p);
            };

            /*
            for (int i = hull.Count - 2; i > 0; i--) {
                if (RightOf(hull[i - 1], hull[i], hull[i + 1])) {
                    hull.RemoveAt(i);
                }
            }

            if (RightOf(hull[hull.Count - 2], hull.Last(), hull[0])) {
                hull.RemoveAt(hull.Count - 1);
            }
            if (RightOf(hull.Last(), hull.First(), hull[1])) {
                hull.RemoveAt(0);
            }
            */

            /*
            int j = hull.Count - 1;
            while(j > 0) {
                var p = hull[j];
                for (int i = j - 1; i > 0; i--) {
                    if (RightOf(hull[i - 1], hull[i], p)) {
                        hull.RemoveAt(i);
                        j--;
                    }
                }
                j--;
            }
            */


            return hull;
        }
        public static void DrawPoints(Graphics g, Color c, List<Vector2 > points) {
            points.ForEach(p => g.FillEllipse(new SolidBrush(c), p.X - 1, p.Y - 1, 3, 3));

            return;
            for(int i = 0; i < points.Count; i++) {
                g.DrawString($"{i}", new Font("Consolas", 8), Brushes.Black, points[i].X, points[i].Y);
            }

        }
        public static void DrawHull(Graphics g, Color c, List<Vector2 > hull) {
            if (hull.Any()) {

                var p = new Pen(c, 1);
                for (int index = 0; index + 1 < hull.Count; index++) {
                    var p1 = hull[index];
                    var p2 = hull[index + 1];
                    g.DrawLine(p, Point(p1), Point(p2));
                }
                g.DrawLine(p, Point(hull.First()), Point(hull.Last()));
            }
        }
        static int size = 1600;


        public static void Config(string name, IEnumerable<Vector2> points) {
            using (Bitmap frame = new Bitmap(size, size)) {
                using (Graphics g = Graphics.FromImage(frame)) {
                    g.FillRectangle(Brushes.White, 0, 0, size, size);


                    var c = Color.Black;
                    DrawPoints(g, c, points.ToList());

                    DateTime start = DateTime.Now;
                    var hull = GetHull(points.ToList());
                    DateTime end = DateTime.Now;
                    DrawHull(g, Color.Red, hull);
                    double seconds = (end - start).TotalSeconds;

                    name = $"Results/{name} - {seconds} seconds.png";

                }
                frame.Save(name, ImageFormat.Png);
            }
        }

        public static void Main(string[] args) {
            Random r = new Random();
            Vector2 center = new Vector2(size / 2, size / 2);
            //points.Sort((a, b) => Angle(a).CompareTo(Angle(b)));

            int pointCount = 1000000;
            Directory.CreateDirectory("Results");

            Config("DustRing", Enumerable.Range(0, pointCount).Select(p => p < 10 ? (center + Polar(p * Math.PI / 2, size / 2)) : center + Polar(r.NextDouble() * Math.PI * 2, Math.Pow(2 * r.NextDouble(), 2) * size / 8 - 50)));

            Config("UniformCircle", Enumerable.Range(0, pointCount).Select(p => p < 10 ? (center + Polar(p * Math.PI / 2, size / 2)) : center + Polar(r.NextDouble() * Math.PI * 2, Math.Sqrt(r.NextDouble() * 4) * size / 4)));



            Config("FullSquare", Enumerable.Range(0, pointCount).Select(
                            p => new Vector2(r.Next(size - 4) + 2, r.Next(size - 4) + 2)));
            Config("FullCircle", Enumerable.Range(0, pointCount).Select(p => center + Polar(r.NextDouble() * Math.PI * 2, r.NextDouble() * (size / 2) * (1 - p / pointCount))));
            Config("Ring", Enumerable.Range(0, pointCount).Select(p => center + Polar(r.NextDouble() * Math.PI * 2, size / 4)));
            Config("TwoRing", Enumerable.Range(0, pointCount).Select(p => p < 10 ? (center + Polar(r.NextDouble() * Math.PI * 2, size / 2)) : center + Polar(r.NextDouble() * Math.PI * 2, size / 8)));
            Config("SnowCone", Enumerable.Range(0, pointCount).Select(p => p < 10 ? (center + Polar(Math.PI / 2, size / 2)) : center + Polar(r.NextDouble() * Math.PI * 2, size / 8)));
            Config("DiamondRing", Enumerable.Range(0, pointCount).Select(p => p < 10 ? (center + Polar(p * Math.PI / 2, size / 2)) : center + Polar(r.NextDouble() * Math.PI * 2, size / 8)));


            //goto Start;
        }
    }
}
