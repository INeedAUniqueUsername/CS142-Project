using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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
            return Math.Atan2(v.Y, v.X);
        }

        public static bool LeftOf(Vector2 a, Vector2 b, Vector2 p) {
            Vector2 ab = b - a;
            Vector2 left = Rotate(ab, Math.PI / 2);
            return Vector2.Dot(left, p - a) > 0;
        }
        public static List<Vector2> GetHull(List<Vector2> points) {
            List<Vector2> hull = new List<Vector2>();

            Vector2 hullPoint = points[0];
            points.ForEach(p => {
                if (p.X < hullPoint.X) hullPoint = p;
            });

            Vector2 endpoint;
            do {
                hull.Add(hullPoint);
                endpoint = points[0];

                foreach (var p in points) {
                    if (endpoint == hullPoint || LeftOf(hullPoint, endpoint, p)) {
                        endpoint = p;
                    }
                }
                hullPoint = endpoint;
            } while (endpoint != hull.First());

            return hull;
        }
        public static void DrawHull(Graphics g, Color c, List<Vector2 > hull) {
            var p = new Pen(c, 1);
            for (int index = 0; index + 1 < hull.Count; index++) {

                var p1 = hull[index];
                var p2 = hull[index + 1];

                g.DrawLine(p, Point(p1), Point(p2));
            }
            g.DrawLine(p, Point(hull.First()), Point(hull.Last()));
        }
        public static void Main(string[] args) {
            Random r = new Random();
            int size = 1600;
            int radius = size / 2;

            Vector2 center = new Vector2(size / 2, size / 2);

            //points.Sort((a, b) => Angle(a).CompareTo(Angle(b)));
            
            using (Bitmap frame = new Bitmap(size, size)) {
                using (Graphics g = Graphics.FromImage(frame)) {


                    g.FillRectangle(Brushes.White, 0, 0, size, size);

                    Color[] colors = {
                        Color.Red, Color.Blue, Color.Green
                    };

                    foreach (Color c in colors) {
                        int pointCount = 1000;
                        List<Vector2> points = new List<Vector2>(
                            Enumerable.Range(0, pointCount).Select(
                                //p => new Vector2(r.Next(size), r.Next(size))
                                //p => center + new Vector2(r.Next(size/3), r.Next(size/3))
                                p => center + Polar(r.NextDouble() * (Math.PI * 2), r.Next(radius * p / pointCount))
                                )
                            );

                        points.ForEach(p => g.FillEllipse(new SolidBrush(c), p.X - 1, p.Y - 1, 3, 3));
                        points.ForEach(p => g.DrawLine(new Pen(c, 1), p.X - 2, p.Y, p.X + 2, p.Y));

                        var hull = GetHull(points);
                        DrawHull(g, c, hull);

                    }

                }
                frame.Save($"Hull.png", ImageFormat.Png);
            }
        }
    }
}
