using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using static Quickhull.Program;
namespace Quickhull {
    public class Program {
        public static Vector2 Rotate(Vector2 v, double delta) {
            var angle = (Angle(v) + delta);
            var length = v.Length();
            return new Vector2((float)(length * Math.Cos(angle)), (float)(length * Math.Sin(angle)));
        }
        public static double Angle(Vector2 v) {
            return Math.Atan2(v.Y, v.X);
        }
        
    }
    class ConvexHull {
        List<Vector2> hull = new List<Vector2>();
        public void FindHull() {

            List<Vector2> points = new List<Vector2>();
            points.Sort((a, b) => {
                var result = a.X.CompareTo(b.X);
                if (result == 0) {
                    result = a.Y.CompareTo(b.Y);
                }
                return result;
            });

            var left = points.First();
            var right = points.Last();
            var midpoint = (left + right) / 2;

            var vector = (right - left);
            var up = Rotate(vector, Math.PI / 2);

            List<Vector2> s1 = new List<Vector2>();
            List<Vector2> s2 = new List<Vector2>();
            foreach (var p in points.Skip(1).SkipLast(1)) {
                if (Vector2.Dot(up, p - midpoint) > 0) {
                    s1.Add(p);
                } else {
                    s2.Add(p);
                }
            }
            FindHull(s1, left, right);
            FindHull(s2, right, left);

            Vector2 center = new Vector2(0, 0);
            hull.ForEach(v => center += v / hull.Count);
            hull.Sort((a, b) => Angle(a - center).CompareTo(b - center));
        }
        static void FindHull(List<Vector2> points, Vector2 left, Vector2 right) {

        }
    }
}
