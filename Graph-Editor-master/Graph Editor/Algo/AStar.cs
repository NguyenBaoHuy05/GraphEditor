using Guna.UI2.WinForms;
using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Graph_Editor
{
    class AStar
    {
        public static async Task Algorithm(int n, int start, int end, List<List<int>> adjList, List<Guna2CircleButton> nodes, Dictionary<(int, int, Color), int> edges, Color defaultColor, Color visColor, Color bestNodeColor, Color completedColor, int delayMilliseconds, RichTextBox Log, Guna2PictureBox Board)
        {
            Log.Clear();
            int[] g = new int[n];
            double[] f = new double[n];
            double[] h = new double[n];
            bool[] vis = new bool[n];
            int[] save = new int[n];
            Array.Fill(g, int.MaxValue);
            Array.Fill(f, int.MaxValue);
            g[start] = 0;
            f[start] = 0;
            nodes[start].FillColor = Color.Red; 
            nodes[end].FillColor = Color.Green;

            
            save[start] = -1;

            var pq = new PriorityQueue<int, double>();
            pq.Enqueue(start, f[start]);

            while (pq.Count > 0)
            {
                int node = pq.Dequeue();
                if (node == end)
                {
                    await Reconstruct(n, start, end, save, nodes, defaultColor, completedColor, delayMilliseconds, Log, g, edges, Board);
                    return;
                }

                if (node != start && node != end)
                {
                    nodes[node].FillColor = bestNodeColor;
                }

                foreach(int neighbor in adjList[node])
                {
                    if (vis[neighbor]) continue;
                    if(neighbor != end)
                    {
                        nodes[neighbor].FillColor = visColor;
                    }
                    int max = Math.Max(neighbor, node);
                    int min = Math.Min(neighbor, node);
                    edges[(min, max, visColor)] = edges[(min, max, Color.Black)];
                    Board.Invalidate();
                    await Task.Delay(delayMilliseconds);
                    int weight = edges[(min, max, Color.Black)];
                    int dist = weight + g[node];
                    if (dist < g[neighbor])
                    {
                        save[neighbor] = node;
                        g[neighbor] = dist;
                        f[neighbor] = dist + h[neighbor];

                        pq.Enqueue(neighbor, f[neighbor]);
                    }
                    if(neighbor != end)
                    {
                        nodes[neighbor].FillColor = defaultColor;
                    }
                    edges.Remove((min, max, visColor));
                    Board.Invalidate();
                    await Task.Delay(delayMilliseconds);
                }

                vis[node] = true;
            }
            Log.AppendText($"Không có đường đi từ {start} đến {end}\n");
        }
        private static async Task Reconstruct(int n, int start, int end, int[] save, List<Guna2CircleButton> nodes, Color defaultColor, Color completedColor, int delayMilliseconds, RichTextBox Log, int[] g,Dictionary<(int, int, Color), int> edges, Guna2PictureBox Board)
        {            
            Log.AppendText($"Khoảng cách ngắn nhất từ {start} đến {end}: {g[end]}\n");
            Log.AppendText("Đường đi: ");
            for (int i = 0; i < n; ++i)
            {
                nodes[i].FillColor = defaultColor;
            }
            nodes[start].FillColor = Color.Red;
            nodes[end].FillColor = Color.Green;
            int j = end;
            Stack<int> S = new Stack<int>();
            while (j != -1)
            {
                S.Push(j);
                j = save[j];
            }

            Log.AppendText(string.Join(" -> ", S));
            while (S.Count > 0)
            {
                int node = S.Pop();
                if (node != end)
                {
                    if (node != start)
                    {
                        nodes[node].FillColor = completedColor;
                    }
                    await Task.Delay(delayMilliseconds);
                    int min = Math.Min(node, S.First());
                    int max = Math.Max(node, S.First());
                    edges[(min, max, Color.FromArgb(0, 255, 136))] = edges[(min, max, Color.Black)];
                    Board.Invalidate();
                    await Task.Delay(delayMilliseconds);
                }
            }
        }
        private static double Dist(Guna2CircleButton node1, Guna2CircleButton node2)
        {
            double x1 = node1.Location.X;
            double y1 = node1.Location.Y;
            double x2 = node2.Location.X;
            double y2 = node2.Location.Y;
            return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
        }
    }
}
