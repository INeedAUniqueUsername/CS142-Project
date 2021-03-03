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
        public static List<Vector2> GetHull(List<Vector2> points) {
            List<Vector2> hull = new List<Vector2>();

            int firstIndex = 0;
            Vector2 hullPoint = points[0];
            for (int i = 0; i < points.Count; i++) {
                var p = points[i];
                if (p.X > hullPoint.X) {
                    firstIndex = i;
                    hullPoint = p;
                }
            }

            points.Sort((p1, p2) => {
                if(p1 == hullPoint) {
                    return -1;
                } else if(p2 == hullPoint) {
                    return 1;
                }

                var o1 = p1 - hullPoint;
                var o2 = p2 - hullPoint;
                var result = Angle(o1).CompareTo(Angle(o2));
                if(result == 0) {
                    result = -o1.Length().CompareTo(o2.Length());
                }
                return result;
            });


            int index = points.IndexOf(hullPoint);

            Vector2 nextPoint = hullPoint;

            int frame = 0;
            Directory.CreateDirectory("Frames");
            do {
                /*
                frame++;
                using (Bitmap b = new Bitmap(size, size)) {
                    using (Graphics g = Graphics.FromImage(b)) {
                        g.FillRectangle(new SolidBrush(Color.White), 0, 0, size, size);
                        DrawPoints(g, Color.Black, points);
                        DrawHull(g, Color.Black, hull);
                    }
                    b.Save($"Frames/{frame}.png", ImageFormat.Png);
                }
                */
                hull.Add(hullPoint);

                index = (index + 1) % points.Count;
                nextPoint = points[index];

                //Remove any old edges that no longer work
                for (int i = hull.Count - 1; i > 0; i--) {
                    if (RightOf(hull[i - 1], hull[i], nextPoint)) {
                        hull.RemoveAt(i);
                    } else {
                    }
                }
                /*
                //Our initial edges might start at the center point
                //Make sure to remove them when we reach the end
                for (int i = 1; i < hull.Count; i++) {
                    if (RightOf(hull[i - 1], hull[i], nextPoint)) {
                        hull.RemoveAt(i);
                    } else {
                    }
                }
                */



                hullPoint = nextPoint;
            } while (nextPoint != hull.First());


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
        public static void Main(string[] args) {
            Random r = new Random();
            Vector2 center = new Vector2(size / 2, size / 2);
            //points.Sort((a, b) => Angle(a).CompareTo(Angle(b)));

            Start:
            using (Bitmap frame = new Bitmap(size, size)) {

                string name;
                using (Graphics g = Graphics.FromImage(frame)) {
                    g.FillRectangle(Brushes.White, 0, 0, size, size);


                    int pointCount = 1000000;
                    List<Vector2> points = new List<Vector2>(
                        Enumerable.Range(0, pointCount).Select(
                            p => new Vector2(r.Next(size - 4) + 2, r.Next(size - 4) + 2)
                            //p => center + Polar(r.NextDouble() * Math.PI*2, r.NextDouble() * (size / 2) * (1 - p / pointCount))
                            //p => center + Polar(r.NextDouble() * Math.PI * 2, size / 4)
                        )); ;

                    var c = Color.Black;
                    DrawPoints(g, c, points);

                    DateTime start = DateTime.Now;
                    var hull = GetHull(points);
                    DateTime end = DateTime.Now;
                    DrawHull(g, Color.Red, hull);
                    double seconds = (end - start).TotalSeconds;

                    name = $"Full{Directory.GetFiles(".").Length} - {seconds} seconds.png";
                }
                frame.Save(name, ImageFormat.Png);
            }
            //goto Start;
        }
    }
}
