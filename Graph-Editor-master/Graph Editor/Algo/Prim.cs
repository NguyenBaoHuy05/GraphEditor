using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graph_Editor
{
    internal class Prim
    {

        public static async Task Algorithm(int n, List<List<int>> adjList, List<Guna2CircleButton> nodes, Dictionary<(int, int, Color), int> edges, Color edgeColor, Color mstEdgeColor, Color completedColor, int delayMilliseconds, RichTextBox Log, Guna2PictureBox Board)
        {
            Log.Clear();
            var pq = new PriorityQueue<(int,int), int>();
            bool[] vis = new bool[n];
            Array.Fill(vis, false);
            pq.Enqueue((-1,0),0);
            int sum = 0;
            int cnt = 0;
            List<(int, int)> MST = new List<(int, int)>();
            while (pq.Count > 0)
            {
                var node = pq.Dequeue();
                if (vis[node.Item2]) continue;
                vis[node.Item2] = true;
                nodes[node.Item2].FillColor = completedColor;
                ++cnt;
                MST.Add((node.Item1, node.Item2));
                if (node.Item1 != -1)
                {
                    int min = Math.Min(node.Item1, node.Item2);
                    int max = Math.Max(node.Item1, node.Item2);
                    edges[(min, max, mstEdgeColor)] = edges[(min, max, Color.Black)];
                    int wt = edges[(min, max, Color.Black)];
                    sum += wt;
                    Board.Invalidate();
                    Log.AppendText($"Cạnh {node.Item1} -> {node.Item2} với trọng số {wt} đã được thêm vào MST\n");
                    await Task.Delay(delayMilliseconds);
                }
                foreach(int neighbor in adjList[node.Item2])
                {
                    if (!vis[neighbor])
                    {
                        int min = Math.Min(node.Item2, neighbor);
                        int max = Math.Max(node.Item2, neighbor);
                        int wt = edges[(min, max, Color.Black)];
                        pq.Enqueue((node.Item2, neighbor), wt);
                        edges[(min, max, edgeColor)] = edges[(min, max, Color.Black)];
                        nodes[neighbor].FillColor = edgeColor;
                        Board.Invalidate();
                        await Task.Delay(delayMilliseconds);
                        edges.Remove((min, max, edgeColor));
                        nodes[neighbor].FillColor = Color.FromArgb(94,148,255);
                        Board.Invalidate();
                    }
                }
            }
            if (cnt < n)
            {
                Log.AppendText("Đồ thị không có cây khung vì không liên thông:\n");
                return;
            }
            Log.AppendText($"Cây khung nhỏ nhất (MST) đã được tạo với trọng số là {sum}:\n");
            foreach (var edge in MST)
            {

                if(edge.Item1 == -1)
                {
                    continue;
                }
                Log.AppendText($"{edge.Item1} -> {edge.Item2}\n");
            }

        }


    }
}
