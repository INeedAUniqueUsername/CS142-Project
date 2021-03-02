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
        public static Vector2 Rotate(Vector2 v, double delta) {
            var angle = (Angle(v) + delta);
            var length = v.Length();
            return Polar(angle, length);
        }
        public static Vector2 Polar(double angle, double length) {
            return new Vector2((float)(length * Math.Cos(angle)), (float)(length * Math.Sin(angle)));
        }
        public static Point Point(Vector2 v) {
            return new Point((int)v.X, (int)v.Y);
        }
        public static double Angle(Vector2 v) {
            var angle = Math.Atan2(v.Y, v.X);
            while(angle < 0) {
                angle += Math.PI * 2;
            }
            return angle;
        }

        public static bool RightOf(Vector2 a, Vector2 b, Vector2 p) {
            Vector2 ab = b - a;
            Vector2 right = Rotate(ab, -Math.PI / 2);
            return Vector2.Dot(right, p - a) > 0;
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

                for (int i = hull.Count - 1; i > 0; i--) {
                    if (RightOf(hull[i - 1], hull[i], nextPoint)) {
                        hull.RemoveAt(i);
                    }
                }

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

            using (Bitmap frame = new Bitmap(size, size)) {
                using (Graphics g = Graphics.FromImage(frame)) {
                    g.FillRectangle(Brushes.White, 0, 0, size, size);

                    int iterations = 4;
                    int interval = 255 / iterations;
                    for(int i = 0; i < iterations; i++) {
                        g.FillRectangle(new SolidBrush(Color.FromArgb(102, Color.White)), 0, 0, size, size);


                        Color c = Color.FromArgb(i * interval, 0, 255 - i * interval);
                        int radius = size/2 - ((size/2 - size/8) / iterations) * i;

                        int pointCount = 500;
                        List<Vector2> points = new List<Vector2>(
                            Enumerable.Range(0, pointCount).Select(
                                //p => new Vector2(r.Next(size), r.Next(size))
                                //p => center + new Vector2(r.Next(size/3), r.Next(size/3))
                                p => center + Polar(r.NextDouble() * (Math.PI * 2), r.Next(radius * (1 - p / pointCount)))
                                )
                            );


                        
                        DrawPoints(g, c, points);
                        //points.ForEach(p => g.DrawLine(new Pen(c, 1), p.X - 2, p.Y, p.X + 2, p.Y));

                        //Vector2 center = new Vector2(points.Sum(p => p.X), points.Sum(p => p.Y));
                        //points.Sort((p1, p2) => Angle(p2 - center).CompareTo(Angle(p1 - center)));
                        //DrawHull(g, c, points);

                        var hull = GetHull(points);
                        //DrawHull(g, c, points);
                        DrawHull(g, c, hull);

                    }

                }
                frame.Save($"Full.png", ImageFormat.Png);
            }
        }
    }
}
