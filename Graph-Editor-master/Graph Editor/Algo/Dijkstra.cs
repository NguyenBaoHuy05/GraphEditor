using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace Graph_Editor
{
    internal class Dijkstra
    {
        public static async Task Algorithm(int n, int start, int end, List<List<int>> adjList, List<Guna2CircleButton> nodes, Dictionary<(int, int, Color), int> edges, Color defaultColor, Color visColor, Color bestNodeColor, Color completedColor, int delayMilliseconds, RichTextBox Log, Guna2PictureBox Board)
        {
            int[] distances = new int[n];
            bool[] completed = new bool[n]; 
            int[] save = new int[n];

            for (int i = 0; i < n; i++)
            {
                distances[i] = int.MaxValue;
                completed[i] = false; 
                save[i] = -1;
            }
            distances[start] = 0;

            var pq = new PriorityQueue<int, int>();
            pq.Enqueue(start, 0);
            nodes[start].FillColor = Color.Red;
            nodes[end].FillColor = Color.Green;
            for (int i = 0; i < n; ++i)
            {
                if (i != start && i != end)
                {
                    nodes[i].FillColor = defaultColor;
                }
            }
            while (pq.Count > 0)
            {
                int current = pq.Dequeue(); 
                if(current == end)
                {
                    await Reconstruct(n, start, end, save, nodes, defaultColor, completedColor, delayMilliseconds, Log, distances, edges, Board);
                    return;
                }
                if (current != start && current != end) nodes[current].FillColor = bestNodeColor;
                
                foreach(int neighbor in adjList[current])
                {
                    if (completed[neighbor]) continue;
                    if(neighbor != end) nodes[neighbor].FillColor = visColor;
                    int min = Math.Min(neighbor, current);
                    int max = Math.Max(neighbor, current);
                    edges[(min, max, visColor)] = edges[(min, max, Color.Black)];
                    Board.Invalidate();
                    await Task.Delay(delayMilliseconds);
                    if (edges[(min, max, Color.Black)] > 0 &&
                       distances[current] + edges[(min, max, Color.Black)] < distances[neighbor])
                    {
                        distances[neighbor] = distances[current] + edges[(min, max, Color.Black)];
                        save[neighbor] = current; 
                        pq.Enqueue(neighbor, distances[neighbor]); 
                    }                    
                    if (neighbor != end) nodes[neighbor].FillColor = defaultColor;
                    edges.Remove((min, max, visColor));
                    Board.Invalidate();
                    await Task.Delay(delayMilliseconds);
                }
                completed[current] = true;
            }
            if (distances[end] == int.MaxValue)
            {
                Log.AppendText($"Không có đường đi từ {start} đến {end}\n");
            }
            else
            {
                Log.AppendText($"Khoảng cách ngắn nhất từ {start} đến {end}: {distances[end]}\n");
                Log.AppendText("Đường đi: ");

                Stack<int> path = new Stack<int>();
                int trace = end;
                while (trace != -1)
                {
                    path.Push(trace);
                    trace = save[trace];
                }
                Log.AppendText(string.Join(" -> ", path) + "\n");
            }
        }
        private static async Task Reconstruct(int n, int start, int end, int[] save, List<Guna2CircleButton> nodes, Color defaultColor, Color completedColor, int delayMilliseconds, RichTextBox Log, int[] g, Dictionary<(int, int, Color), int> edges, Guna2PictureBox Board)
        {
            Log.Clear();
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
    }
}

