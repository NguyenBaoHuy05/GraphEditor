using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graph_Editor
{
    public class ForceMode
    {
        public static void ApplyForce(int k, double coolingFactor, double l, List<PointF> F, List<Guna2CircleButton> nodes, Dictionary<(int, int, Color), int> edges, int num, Guna2PictureBox Board, Guna2CircleButton draggingNode, List<List<int>> adjList)
        {
            double maxForce = double.MaxValue;
            while (k > 0 && maxForce > 0.00001)
            {

                maxForce = F.Max(f => Math.Sqrt(f.X * f.X + f.Y * f.Y));
                Parallel.For(0, num, i =>
                {
                    PointF repForce = RepulsiveForce(i, l, 12, nodes, num);
                    PointF attrForce = AttractiveForce(i, l, 1, repForce, nodes, adjList, edges);
                    F[i] = new PointF(repForce.X + attrForce.X, repForce.Y + attrForce.Y);
                });
                for (int i = 0; i < num; ++i)
                {
                    PointF newLocation = new PointF(
                        nodes[i].Location.X + (float)(coolingFactor * F[i].X),
                        nodes[i].Location.Y + (float)(coolingFactor * F[i].Y));
                    newLocation.X = Math.Max(0, Math.Min(Board.Width - nodes[i].Width, newLocation.X));
                    newLocation.Y = Math.Max(0, Math.Min(Board.Height - nodes[i].Height, newLocation.Y));
                    if (draggingNode == null || nodes[i].Location != draggingNode.Location)
                    {
                        nodes[i].Location = new Point((int)Math.Round(newLocation.X), (int)Math.Round(newLocation.Y));
                    }
                }
                coolingFactor *= 0.5;
                --k;
            }
        }
        public static PointF RepulsiveForce(int node, double l, double crep, List<Guna2CircleButton> nodes, int num)
        {
            PointF repForce = new PointF();
            repForce = new PointF(0, 0);
            Guna2CircleButton u = nodes[node];
            for (int i = 0; i < num; ++i)
            {
                if (i == node) continue;
                Guna2CircleButton v = nodes[i];
                PointF vu = new PointF(u.Location.X - v.Location.X, u.Location.Y - v.Location.Y);
                double distance = Math.Sqrt(vu.X * vu.X + vu.Y * vu.Y) / 148;
                double f = crep / (distance * distance);
                if (distance < 1.5)
                {
                    repForce.X += (float)(f * vu.X);
                    repForce.Y += (float)(f * vu.Y);
                }
            }
            return repForce;
        }

        public static PointF AttractiveForce(int node, double l, double cspring, PointF repForce, List<Guna2CircleButton> nodes, List<List<int>> adjList, Dictionary<(int, int, Color), int> edges)
        {
            PointF attrForce = new PointF();
            attrForce = new PointF(0, 0);
            Guna2CircleButton u = nodes[node];
            foreach (int adjNode in adjList[node])
            {
                Guna2CircleButton v = nodes[adjNode];
                PointF uv = new PointF(v.Location.X - u.Location.X, v.Location.Y - u.Location.Y);
                double distance = Math.Sqrt(uv.X * uv.X + uv.Y * uv.Y);
                int cs = edges[(node, adjNode, Color.Black)];
                double f = cspring * Math.Log(cs * cs * distance / l);
                attrForce.X += (float)(f * uv.X);
                attrForce.Y += (float)(f * uv.Y);
            }
            return attrForce;
        }
    }
}
