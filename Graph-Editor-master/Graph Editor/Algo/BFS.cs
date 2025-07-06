using Guna.UI2.WinForms;
using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Graph_Editor
{
    class BFS
    {
        public static async Task Algorithm(int n, int start, int end, List<List<int>> adjList, List<Guna2CircleButton> nodes, Color defaultColor, Color visColor, Color bestNodeColor, Color completedColor, int delayMilliseconds, RichTextBox Log, Guna2PictureBox Board, Dictionary<(int, int, Color), int> edges)
        {
            Log.Clear();
            bool[] vis = new bool[n];
            int[] save = new int[n];
            Queue<int> queue = new Queue<int>();
            Array.Fill(save, -1);
            Array.Fill(vis, false);

            queue.Enqueue(start);
            vis[start] = true;
            nodes[start].FillColor = Color.Red;
            nodes[end].FillColor = Color.Green;
            for (int i = 0; i < n; ++i)
            {
                if (i != start && i != end)
                {
                    nodes[i].FillColor = defaultColor;
                }
            }

            while (queue.Count > 0)
            {
                int node = queue.Dequeue();
                if (node == end)
                {
                    await Reconstruct(n, start, end, save, nodes, defaultColor, completedColor, delayMilliseconds, Log, edges, Board);
                    return;
                }

                if (node != start && node != end)
                {
                    nodes[node].FillColor = bestNodeColor;
                }
                await Task.Delay(delayMilliseconds);
                foreach (int neighbor in adjList[node])
                {
                    if (vis[neighbor]) continue;

                    vis[neighbor] = true;
                    save[neighbor] = node;
                    queue.Enqueue(neighbor);
                    if (neighbor != end)
                    {
                        nodes[neighbor].FillColor = visColor;
                    }
                    int min = Math.Min(node, neighbor);
                    int max = Math.Max(node, neighbor);
                    edges[(min, max, visColor)] = edges[(min, max, Color.Black)];
                    Board.Invalidate();
                    await Task.Delay(delayMilliseconds);

                    if (neighbor != end)
                    {
                        nodes[neighbor].FillColor = defaultColor;
                    }
                    edges.Remove((min, max, visColor));
                    Board.Invalidate();
                    await Task.Delay(delayMilliseconds);
                }
            }

            Log.AppendText($"Không có đường đi từ {start} đến {end}\n");
        }

        private static async Task Reconstruct(int n, int start, int end, int[] save, List<Guna2CircleButton> nodes, Color defaultColor, Color completedColor, int delayMilliseconds, RichTextBox Log, Dictionary<(int, int, Color), int> edges, Guna2PictureBox Board)
        {
            Log.AppendText($"Đường đi từ {start} đến {end} đã được tìm thấy.\n");
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
    }
}

