using Guna.UI2.WinForms;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Graph_Editor
{
    internal class Kruskal
    {

        public static async Task Algorithm(int n, Dictionary<(int, int, Color), int> edges,Color visitedEdges, Color edgeColor, Color mstEdgeColor, int delayMilliseconds, RichTextBox Log, Guna2PictureBox Board)
        {
            Log.Clear();
            int sum = 0;
            List<(int, int, int)> edgeList = new List<(int, int, int)>();
            foreach (var edge in edges)
                
            {
                int u = edge.Key.Item1;
                int v = edge.Key.Item2;
                int weight = edge.Value;

                edgeList.Add((u, v, weight));
            }

            edgeList.Sort((x, y) => x.Item3.CompareTo(y.Item3));
            DisjointSet ds = new DisjointSet(n);
            List<(int, int)> MST = new List<(int, int)>();
            foreach (var edge in edgeList)
            {
                int u = edge.Item1;
                int v = edge.Item2;
                int weight = edge.Item3;
                edges[(u, v, visitedEdges)] = edges[(u, v, Color.Black)];
                Board.Invalidate();
                await Task.Delay(delayMilliseconds);
                edges.Remove((u, v, visitedEdges));
                Board.Invalidate();
                await Task.Delay(delayMilliseconds);
                if (ds.FindUParent(u) != ds.FindUParent(v))
                {
                    ds.UnionByRank(u, v);
                    MST.Add((u, v));
                    edges[(u, v, mstEdgeColor)] = edges[(u, v, Color.Black)];
                    Board.Invalidate();
                    sum += edges[(u, v, Color.Black)];
                    Board.Invalidate();
                    await Task.Delay(delayMilliseconds);
                    Log.AppendText($"Cạnh {u} -> {v} với trọng số {weight} đã được thêm vào MST\n");
                }
            }
            if(MST.Count < n-1)
            {
                Log.AppendText("Đồ thị không có cây khung vì không liên thông:\n");
                return;
            }
            Log.AppendText($"Cây khung nhỏ nhất (MST) đã được tạo với trọng số là {sum}:\n");
            foreach (var edge in MST)
            {
                Log.AppendText($"{edge.Item1} -> {edge.Item2}\n");
            }
        }
    }
}
