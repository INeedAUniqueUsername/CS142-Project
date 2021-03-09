using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using static Quickhull.Program;

using static System.Math;
namespace Quickhull {
    public class Program {
        public static Vector2 Rotate(in Vector2 v, double delta) {
            var angle = (Angle(v) + delta);
            var length = v.Length();
            return Polar(angle, length);
        }
        public static Vector2 Polar(double angle, double length) {
            return new Vector2((float)(length * Cos(angle)), (float)(length * Sin(angle)));
        }
        public static Point Point(in Vector2 v) {
            return new Point((int)v.X, (int)v.Y);
        }
        public static double Slope(in Vector2 left, in Vector2 right) {

            double x = right.X - left.X, y = right.Y - left.Y;

            return x switch {
                0 => y > 0 ? double.MaxValue : double.MinValue,
                _ => y / x
            };
        }
        public static double Slope(in Vector2 v) {
            return v.X switch {
                0 => v.Y > 0 ? double.MaxValue : double.MinValue,
                _ => v.Y / v.X
            };
        }
        public static double Angle(in Vector2 v) {
            var angle = Atan2(v.Y, v.X);
            while(angle < 0) {
                angle += PI * 2;
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
        public static bool LeftOf(in Vector2 a, in Vector2 b, in Vector2 p) {
            Vector2 ab = b - a;
            Vector2 ap = p - a;
            return Cross(ap, ab) < 0;
        }
        public static bool Inside(in Vector2 east, in Vector2 north, in Vector2 west, in Vector2 south, in Vector2 point) {
            return LeftOf(east, north, point)
                && LeftOf(north, west, point)
                && LeftOf(west, south, point)
                && LeftOf(south, east, point);
        }
        public static bool Outside(in Vector2 east, in Vector2 north, in Vector2 west, in Vector2 south, in Vector2 point) {
            return RightOf(east, north, point)
                && RightOf(north, west, point)
                && RightOf(west, south, point)
                && RightOf(south, east, point);
        }
        public static List<Vector2> GetCorrectHull(List<Vector2> points) {
            Vector2 firstPoint = points.First();
            foreach (var p in points) {
                if (p.X > firstPoint.X) {
                    firstPoint = p;
                }
            }


            List<Vector2> hull = new List<Vector2>();

            hull.Add(firstPoint);

            var sorted = points.AsParallel().OrderBy(p => Angle(p - firstPoint)).Distinct();

            foreach (var p in sorted) {
                if (hull.Count > 1) {
                    //Remove any old edges that no longer work
                    for (int i = hull.Count - 1; i > 0; i--) {
                        if (RightOf(hull[i - 1], hull[i], p)) {
                            hull.RemoveAt(i);
                        }
                    }
                }
                hull.Add(p);
            };
            return hull;
        }
        public static List<Vector2> GetHullFast(List<Vector2> points) {
            Vector2 north = points.First(),
                    east = points.First(),
                    south = points.First(),
                    west = points.First();
            foreach(var p in points) {
                if (p.X > east.X) {
                    east = p;
                } else if(p.X < west.X) {
                    west = p;
                }

                if(p.Y > north.Y) {
                    north = p;
                } else if(p.Y < south.Y) {
                    south = p;
                }
            }
            List<Vector2> hull = new List<Vector2>();
            hull.Add(east);


            points.RemoveAll(p => Inside(east, north, west, south, p));
            var sorted = points.AsParallel().OrderBy(p => Slope(p, east)).Distinct();
            
            //Vector2 center = new Vector2(points.Average(p => p.X), points.Average(p => p.Y));
            //var sorted = points.AsParallel().OrderBy(p => Angle(p - center)).Distinct();

            foreach (var p in sorted) {
                if (hull.Count > 1) {
                    //Remove any old edges that no longer work
                    for (int i = hull.Count - 1; i > Max(0, hull.Count - 100); i--) {
                        if (RightOf(hull[i - 1], hull[i], p)) {
                            hull.RemoveAt(i);
                        }
                    }
                }
                hull.Add(p);
            };

            Check();
            Check();
            Check();
            void Check() {
                while (RightOf(hull.Last(), hull.First(), hull[1])) {
                    hull.RemoveAt(0);
                }
                while (RightOf(hull[hull.Count - 2], hull.Last(), hull[0])) {
                    hull.RemoveAt(hull.Count - 1);
                }

                for (int i = hull.Count - 2; i > 0;) {
                    if (RightOf(hull[i - 1], hull[i], hull[i + 1])) {
                        hull.RemoveAt(i);
                        i = Min(hull.Count - 2, i + 2);
                    } else if (LeftOf(hull[i + 1], hull[i], hull[i - 1])) {
                        hull.RemoveAt(i);
                        i = Min(hull.Count - 2, i + 2);
                    } else {
                        i--;
                    }
                }
                for (int i = 1; i < hull.Count - 1;) {
                    if (RightOf(hull[i - 1], hull[i], hull[i + 1])) {
                        hull.RemoveAt(i);
                        i = Max(0, i - 2);
                    } else if (LeftOf(hull[i + 1], hull[i], hull[i - 1])) {
                        hull.RemoveAt(i);
                        i = Max(0, i - 2);
                    } else {
                        i++;
                    }
                }
                
            }
            return hull;
        }
        public static void DrawPoints(Graphics g, Color c, List<Vector2 > points) {
            int size = 12;
            points.ForEach(p => g.FillEllipse(new SolidBrush(c), (int)p.X - size/2, (int)p.Y - size/2, size, size));

            return;
            for(int i = 0; i < points.Count; i++) {
                g.DrawString($"{i}", new Font("Consolas", 8), Brushes.Black, points[i].X, points[i].Y);
            }

        }
        public static void DrawHull(Graphics g, Color c, List<Vector2 > hull) {
            if (hull.Any()) {

                var p = new Pen(c, 3);
                for (int index = 0; index + 1 < hull.Count; index++) {
                    var p1 = hull[index];
                    var p2 = hull[index + 1];
                    g.DrawLine(p, Point(p1), Point(p2));
                }
                g.DrawLine(p, Point(hull.First()), Point(hull.Last()));

                int size = 6;
                hull.ForEach(p => g.FillEllipse(new SolidBrush(c), (int)p.X - size / 2, (int)p.Y - size / 2, size, size));
            }
        }
        static int size = 1600;


        public static void Config(string name, IEnumerable<Vector2> points) {
            using (Bitmap frame = new Bitmap(size, size)) {
                using (Graphics g = Graphics.FromImage(frame)) {
                    g.FillRectangle(Brushes.White, 0, 0, size, size);

                    //MAKE SURE we put all the points in a list so that it doesn't change behind our back
                    //Since IEnumerable gives us new points every time
                    List<Vector2> pointsList = new List<Vector2>(points);

                    var c = Color.Black;
                    DrawPoints(g, c, pointsList);

                    DateTime start = DateTime.Now;
                    var hull = GetHullFast(pointsList);

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
            Func<double, double> randf = max => r.NextDouble() * max;
            Func<int, int> randi = max => r.Next(max);

            Func<double, double, double> rangef = (min, max) => r.NextDouble() * (max - min) + min;
            Func<int, int, int> rangei = (min, max) => (int) (r.NextDouble() * (max - min) + min);


            Vector2 center = new Vector2(size / 2, size / 2);
            //points.Sort((a, b) => Angle(a).CompareTo(Angle(b)));

            int pointCount = 1000000;
            Directory.CreateDirectory("Results");
            foreach (var f in Directory.GetFiles("Results")) {
                File.Delete(f);
            }

            Config("Eye", Enumerable.Range(0, pointCount).Select(p => p switch {
                int i when i < 10 => center + Polar(randf(PI * 2), size / 2),
                _ => center + new Vector2((float)(randf(100) - 50), (float)(randf(100) - 50))
            }));
            Config("Atom", Enumerable.Range(0, pointCount).Select(p => p switch {
                int i when i < 10 => center + Polar(randf(PI * 2), size / 2),
                _ => center
            }));

            Config("DustRing", Enumerable.Range(0, pointCount).Select(p => p switch {
                int i when i < 10 => (center + Polar(randf(PI * 2), size / 2)),
                int i when i < 30 => (center + Polar(randf(PI * 2), size / 2 - 160)),
                int i when i < 60 => (center + Polar(randf(PI * 2), size / 2 - 320)),
                int i when i < 100 => (center + Polar(randf(PI * 2), size / 2 - 480)),
                int i when i < 150 => (center + Polar(randf(PI * 2), size / 2 - 640)),

                int i when i < 210 => (center + Polar(randf(PI * 2), size / 2 - 800)),

                _ => center + Polar(randf(PI * 2), size / 8)
            }));
            Config("MultiRing", Enumerable.Range(0, pointCount).Select(p => p switch {
                int i when i < Pow(10, 1) => (center + Polar(randf(PI * 2), size / 2)),
                int i when i < Pow(10, 2) => (center + Polar(randf(PI * 2), size / 2 - 160)),
                int i when i < Pow(10, 3) => (center + Polar(randf(PI * 2), size / 2 - 320)),
                int i when i < Pow(10, 4) => (center + Polar(randf(PI * 2), size / 2 - 480)),
                int i when i < Pow(10, 5) => (center + Polar(randf(PI * 2), size / 2 - 640)),

                _ => center + Polar(randf(PI * 2), size / 2 - 800)
            }));

            int s = 10;

            Config("BellTower", Enumerable.Range(0, pointCount).Select(p => p switch {
                int i when i < 10 => center + Polar(randf(PI * 2), size / 2),
                _ => center + new Vector2((float)(randf(80) - 40), (float)(randf(80) - 40)) + new Vector2((int)(randf(s) - s/2), (int)(randf(s) - s/2)) * 160
            }));


            Config("TwoRing", Enumerable.Range(0, pointCount).Select(p => p switch {
                int i when i < 10 => (center + Polar(randf(PI * 2), size / 2)),
                _ => center + Polar(randf(PI * 2), size / 8)
            }));



            Config("FullSquare", Enumerable.Range(0, pointCount).Select(
                            p => new Vector2(r.Next(size - 4) + 2, r.Next(size - 4) + 2)));
            Config("FullCircle", Enumerable.Range(0, pointCount).Select(p => center + Polar(randf(PI * 2), randf((size / 2) * (1 - p / pointCount)))));
            Config("SnowCone", Enumerable.Range(0, pointCount).Select(p => p < 10 ? (center + Polar(PI / 2, size / 2)) : center + Polar(randf(PI * 2), size / 8)));
            Config("BoxRing", Enumerable.Range(0, pointCount).Select(p => p < 10 ? (center + Polar(p * PI / 2, size / 2)) : center + Polar(randf(PI * 2), size / 8)));

            /*
            Config("DiamondCircle", Enumerable.Range(0, pointCount).Select(p => p < 10 ? (center + Polar(p * PI / 2, size / 2)) : center + Polar(randf(PI * 2), Sqrt(randf(4)) * size / 4)));
            */

            Config("Trapezoid", Enumerable.Range(0, pointCount).Select(p => p switch {
                int i when i < pointCount / 4 => center + new Vector2((float)-randf(500) - 100, 200),
                int i when i < pointCount / 2 => center + new Vector2((float)randf(500) + 100, -200),
                int i when i < pointCount * 3 / 4 => center + new Vector2((float)-randf(500) - 100, 400),
                int i when i < pointCount => center + new Vector2((float)randf(500) + 100, -400)
            }));
            Config("Parallelogram", Enumerable.Range(0, pointCount).Select(p => p switch {
                int i when i < pointCount/2 =>    center + new Vector2((float)-randf(500), 200),
                _ =>                    center + new Vector2((float)randf(500), -200)
            }));

            Config("Parallelogram", Enumerable.Range(0, pointCount).Select(p => p switch {
                int i when i < pointCount/4 => center + new Vector2((float)-randf(500), 200),
                int i when i < pointCount * 2/4 => center + new Vector2((float)randf(500), -200),
                int i when i < pointCount * 3/4 => center + new Vector2((float)-randf(500), 400),
                int i when i < pointCount => center + new Vector2((float)randf(500), -400)
            }));



            /*
            Config("Starburst", Enumerable.Range(0, pointCount).Select(p => p switch {

                int i when i < 10 => (center + Polar(p * PI / 2, size / 4)),
                _ => center + Polar(randf(PI * 2), Pow(randf(2), 2) * size / 8)
            }));
            */
            /*
            Config("DustRing", Enumerable.Range(0, pointCount).Select(p => p switch {
                int i when i < 10 => (center + Polar(p * PI / 2, size / 2)),
                _ => center + Polar(rand(PI * 2), Pow(rand(2), 2) * size / 8 - 50)
            }));
            */
            /*
            Config("UniformCircle", Enumerable.Range(0, pointCount).Select(
                p => center + Polar(rand(PI * 2), Sqrt(rand(4)) * size / 4)));
            */

            Config("Ring", Enumerable.Range(0, pointCount).Select(p => center + Polar(randf(PI * 2), size / 4)));

            //goto Start;
        }
    }
}
