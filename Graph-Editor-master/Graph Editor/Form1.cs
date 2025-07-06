using Guna.UI2.WinForms;
using Microsoft.Win32;
using System.Collections.Immutable;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net;
using static Guna.UI2.Native.WinApi;


namespace Graph_Editor
{
    public partial class Form1 : Form
    {
        int num = 0;
        bool isDragging = false;
        Point startPos = new Point();
        Color defaultColor = Color.Black;
        List<Guna2CircleButton> nodes = new List<Guna2CircleButton>();
        List<PointF> F = new List<PointF>();
        Guna2CircleButton firstSelectedNode = null;
        Guna2CircleButton chosenNode = null;
        Guna2CircleButton draggingNode = null;
        string filePath;
        Dictionary<(int, int, Color), int> edges = new Dictionary<(int, int, Color), int>();
        List<List<int>> adjList = new List<List<int>>();
        Stack<(List<Guna2CircleButton>, Dictionary<(int, int, Color), int>)> undo = new Stack<(List<Guna2CircleButton>, Dictionary<(int, int, Color), int>)>();
        Stack<(List<Guna2CircleButton>, Dictionary<(int, int, Color), int>)> redo = new Stack<(List<Guna2CircleButton>, Dictionary<(int, int, Color), int>)>();
        public Form1()
        {
            InitializeComponent();
            Board.Paint += new PaintEventHandler(this.Board_Paint);
            this.DoubleBuffered = true;
            forceModeRadioBtn.Checked = false;
        }

        private void CreateNode(Point point)
        {
            if (addNodes.Checked)
            {
                Guna2CircleButton btn = new Guna2CircleButton();
                btn.BackColor = Color.Transparent;
                btn.Size = new Size(60, 60);
                btn.Location = point;
                btn.Click += Choosebtn;
                btn.Text = num++.ToString();
                btn.MouseDown += btn_MouseDown;
                btn.MouseMove += btn_MouseMove;
                btn.MouseUp += btn_MouseUp;
                btn.DisabledState.FillColor = Color.Gray;
                btn.DisabledState.BorderColor = Color.White;
                btn.DisabledState.ForeColor = Color.White;

                // Thiết lập vùng hiển thị hình tròn
                GraphicsPath path = new GraphicsPath();
                path.AddEllipse(0, 0, btn.Width, btn.Height);
                btn.Region = new Region(path);

                Undo_Redo();

                Board.Controls.Add(btn);
                nodes.Add(btn);

                F.Add(new PointF(0, 0));

                StartNode.Maximum = num - 1;
                EndNode.Maximum = num - 1;

                CreateAdjMatrix();
                CreateWeiMatrix();
                ChangeText();
            }
        }
        private void CreateNodeRandom()
        {
            Random rd = new Random();
            Guna2CircleButton btn = new Guna2CircleButton();
            btn.Size = new Size(60, 60);

            btn.Location = new Point(rd.Next(50, 600), rd.Next(100, 600));
            btn.Text = num++.ToString();
            btn.Click += Choosebtn;
            btn.MouseDown += btn_MouseDown;
            btn.MouseMove += btn_MouseMove;
            btn.MouseUp += btn_MouseUp;

            btn.DisabledState.FillColor = Color.Gray;
            btn.DisabledState.BorderColor = Color.DarkGray;
            btn.DisabledState.ForeColor = Color.White;

            // Thiết lập vùng hiển thị hình tròn
            GraphicsPath path = new GraphicsPath();
            path.AddEllipse(0, 0, btn.Width, btn.Height);
            btn.Region = new Region(path);


            Board.Controls.Add(btn);
            nodes.Add(btn);
            F.Add(new PointF(0, 0));
        }

        private void CreateNodeGph(Point point)
        {
            Guna2CircleButton btn = new Guna2CircleButton();
            btn.Size = new Size(60, 60);
            btn.Location = point;
            btn.Text = num++.ToString();
            btn.Click += Choosebtn;
            btn.MouseDown += btn_MouseDown;
            btn.MouseMove += btn_MouseMove;
            btn.MouseUp += btn_MouseUp;

            btn.DisabledState.FillColor = Color.Gray;
            btn.DisabledState.BorderColor = Color.DarkGray;
            btn.DisabledState.ForeColor = Color.White;

            // Thiết lập vùng hiển thị hình tròn
            GraphicsPath path = new GraphicsPath();
            path.AddEllipse(0, 0, btn.Width, btn.Height);
            btn.Region = new Region(path);

            Board.Controls.Add(btn);
            nodes.Add(btn);

            F.Add(new PointF(0, 0));

        }
        private void ResetColor()
        {
            foreach (var node in nodes)
            {
                node.FillColor = Color.FromArgb(94, 148, 255);
            }
            foreach (Guna2Button btn in adjMatrixPanel.Controls)
            {
                btn.FillColor = Color.Turquoise;
            }
            foreach (Guna2Button btn in weiMatrixPanel.Controls)
            {
                btn.FillColor = Color.Turquoise;
            }
        }
        private void ChangeColor()
        {
            if (chosenNode == null) return;
            chosenNode.FillColor = Color.Gold;
            foreach (int i in adjList[int.Parse(chosenNode.Text)])
            {
                nodes[i].FillColor = Color.GreenYellow;
            }
            foreach (Guna2Button btn in adjMatrixPanel.Controls)
            {
                var indices = (ValueTuple<int, int>)btn.Tag;
                if ((indices.Item1.ToString() == chosenNode.Text || (indices.Item2.ToString() == chosenNode.Text && undirect.Checked)) && btn.Text != "0" && btn.Text != "\u221E") btn.FillColor = Color.GreenYellow;
                else btn.FillColor = Color.Turquoise;
            }
            foreach (Guna2Button btn in weiMatrixPanel.Controls)
            {
                var indices = (ValueTuple<int, int>)btn.Tag;
                if ((indices.Item1.ToString() == chosenNode.Text || (indices.Item2.ToString() == chosenNode.Text && undirect.Checked)) && btn.Text != "\u221E") btn.FillColor = Color.GreenYellow;
                else btn.FillColor = Color.Turquoise;
            }
        }
        private void ChangeChoosebtn(object sender, EventArgs e)
        {
            if (ChoseBtn.Checked)
            {
                return;
            }
            chosenNode = null;
            ResetColor();
        }
        private void Choosebtn(object sender, EventArgs e)
        {
            Guna2CircleButton button = (Guna2CircleButton)sender;
            if (ChoseBtn.Checked)
            {
                chosenNode = button;
                button.FillColor = Color.Gold;
                Chosen();
            }
            else if (DeleteNodes.Checked && num > 0)
            {
                Undo_Redo();
                num -= 1;
                int number = int.Parse(button.Text);
                nodes.Remove(button);
                Board.Controls.Remove(button);
                foreach (var node in nodes)
                {
                    int num3 = int.Parse(node.Text);
                    if (num3 > number)
                    {
                        node.Text = (num3 - 1).ToString();
                    }
                }
                edges = edges.OrderBy(kvp => kvp.Key.Item1)
                      .ThenBy(kvp => kvp.Key.Item2)
                      .ThenBy(kvp => kvp.Key.Item3.ToArgb())
                      .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                foreach (var edge in edges.Keys.ToList())
                {
                    if (edge.Item1 == number || edge.Item2 == number)
                    {
                        edges.Remove((edge.Item1, edge.Item2, defaultColor));
                    }
                    else if (edge.Item1 > number || edge.Item2 > number)
                    {
                        int num1 = (edge.Item1 > number) ? edge.Item1 - 1 : edge.Item1;
                        int num2 = (edge.Item2 > number) ? edge.Item2 - 1 : edge.Item2;
                        edges[(num1, num2, defaultColor)] = edges[(edge.Item1, edge.Item2, defaultColor)];
                        edges.Remove((edge.Item1, edge.Item2, defaultColor));

                    }
                }
                Board.Invalidate();
                CreateAdjMatrix();
                CreateWeiMatrix();
                ChangeText();
            }
        }
        private void Chosen()
        {
            ResetColor();
            ChangeColor();
        }
        private void CreateAdjMatrix()
        {
            adjMatrixPanel.Controls.Clear();
            for (int i = 0; i < num; ++i)
            {
                for (int j = 0; j < num; ++j)
                {
                    Guna2Button btn = new Guna2Button();
                    if (num < 8)
                    {
                        btn.Size = new Size(50, 50);
                        btn.Location = new Point(j * 50, i * 50);
                    }
                    else
                    {
                        btn.Size = new Size(adjMatrixPanel.Width / num, adjMatrixPanel.Height / num);
                        btn.Location = new Point(j * (adjMatrixPanel.Width / num), i * (adjMatrixPanel.Width / num));
                    }
                    btn.Tag = (i, j);
                    btn.FillColor = Color.Turquoise;
                    btn.ForeColor = Color.White;
                    btn.BorderColor = Color.White;
                    btn.BorderThickness = 1;
                    btn.MouseClick += btn_ClickAdj;
                    btn.DisabledState.FillColor = Color.Gray;
                    btn.DisabledState.BorderColor = Color.White;
                    btn.DisabledState.ForeColor = Color.White;
                    if (i == j)
                    {
                        btn.Enabled = false;
                    }
                    adjMatrixPanel.Controls.Add(btn);
                }
            }
        }

        private void CreateWeiMatrix()
        {
            weiMatrixPanel.Controls.Clear();
            for (int i = 0; i < num; ++i)
            {
                for (int j = 0; j < num; ++j)
                {
                    Guna2Button btn = new Guna2Button();
                    if (num < 8)
                    {
                        btn.Size = new Size(50, 50);
                        btn.Location = new Point(j * 50, i * 50);
                    }
                    else
                    {
                        btn.Size = new Size(weiMatrixPanel.Width / num, weiMatrixPanel.Height / num);
                        btn.Location = new Point(j * (weiMatrixPanel.Width / num), i * (weiMatrixPanel.Width / num));
                    }
                    btn.Tag = (i, j);
                    btn.FillColor = Color.Turquoise;
                    btn.ForeColor = Color.White;
                    btn.BorderColor = Color.White;
                    btn.BorderThickness = 1;
                    btn.MouseClick += btn_ClickWei;
                    btn.DisabledState.FillColor = Color.Gray;
                    btn.DisabledState.BorderColor = Color.White;
                    btn.DisabledState.ForeColor = Color.White;
                    if (i == j)
                    {
                        btn.Enabled = false;
                    }
                    weiMatrixPanel.Controls.Add(btn);
                }
            }
        }

        private void btn_ClickAdj(object sender, EventArgs e)
        {
            Undo_Redo();
            Guna2Button btn = (Guna2Button)sender;
            var indices = (ValueTuple<int, int>)btn.Tag;
            int row = indices.Item1;
            int column = indices.Item2;
            int max = undirect.Checked ? Math.Max(row, column) : column;
            int min = undirect.Checked ? Math.Min(row, column) : row;

            if (btn.Text == "1")
            {
                edges.Remove((min, max, defaultColor));
            }
            else
            {
                edges[(min, max, defaultColor)] = 1;
            }

            Board.Invalidate();
            ChangeText();
        }

        private void btn_ClickWei(object sender, MouseEventArgs e)
        {
            Undo_Redo();
            Guna2Button btn = (Guna2Button)sender;
            var indices = (ValueTuple<int, int>)btn.Tag;
            int row = indices.Item1;
            int column = indices.Item2;
            int max = undirect.Checked ? Math.Max(row, column) : column;
            int min = undirect.Checked ? Math.Min(row, column) : row;
            if (e.Button == MouseButtons.Left)
            {

                if (btn.Text == "\u221E")
                {

                    edges[(min, max, defaultColor)] = 1;
                }
                else
                {
                    ++edges[(min, max, defaultColor)];

                }

            }
            else
            {
                if (btn.Text == "\u221E")
                {
                    return;
                }
                else if (btn.Text == "1")
                {
                    edges.Remove((min, max, defaultColor));
                }
                else
                {
                    --edges[(min, max, defaultColor)];
                }
            }
            Board.Invalidate();
            ChangeText();
        }

        private void ChangeText()
        {
            foreach (Guna2Button btn in adjMatrixPanel.Controls)
            {
                var indices = (ValueTuple<int, int>)btn.Tag;
                int row = indices.Item1;
                int column = indices.Item2;
                int max = undirect.Checked ? Math.Max(row, column) : column;
                int min = undirect.Checked ? Math.Min(row, column) : row;
                btn.Text = edges.ContainsKey((min, max, defaultColor)) ? "1" : "0";
            }
            foreach (Guna2Button btn in weiMatrixPanel.Controls)
            {
                var indices = (ValueTuple<int, int>)btn.Tag;
                int row = indices.Item1;
                int column = indices.Item2;
                int max = undirect.Checked ? Math.Max(row, column) : column;
                int min = undirect.Checked ? Math.Min(row, column) : row;
                if (row == column)
                {
                    btn.Text = "0";
                }
                else
                {
                    btn.Text = edges.ContainsKey((min, max, defaultColor)) ? edges[(min, max, defaultColor)].ToString() : "\u221E";
                }

            }
            if (ChoseBtn.Checked)
            {
                LoadAdjList();
                Chosen();
            }
        }

        #region Chức năng SelectNode
        private void btn_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && selectNode.Checked)
            {
                isDragging = false;
                draggingNode = null;
            }
        }

        private void btn_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && selectNode.Checked)
            {
                Guna2CircleButton btn = (Guna2CircleButton)sender;
                draggingNode = btn;
                var newLocation = new Point(
                    btn.Left + e.X - startPos.X,
                    btn.Top + e.Y - startPos.Y
                );
                btn.Location = newLocation;
                Board.Invalidate(); ;
            }
        }
        private void btn_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && selectNode.Checked)
            {
                isDragging = true;
                startPos = e.Location;
            }
            else if (addEdges.Checked)
            {
                var clickedNode = sender as Guna2CircleButton;

                if (firstSelectedNode == null)
                {
                    firstSelectedNode = clickedNode;
                }
                else
                {
                    if (firstSelectedNode != clickedNode)
                    {
                        int i = int.Parse(firstSelectedNode.Text);
                        int j = int.Parse(clickedNode.Text);
                        int max = undirect.Checked ? Math.Max(i, j) : j;
                        int min = undirect.Checked ? Math.Min(i, j) : i;
                        Undo_Redo();
                        edges[(min, max, defaultColor)] = 1;
                        firstSelectedNode = null;
                        Board.Invalidate();
                    }
                }
            }
            ChangeText();
        }
        #endregion
        ///

        ///

        private void loadFile_Click(object sender, EventArgs e)
        {
            if (num != 0)
            {
                MessageBox.Show("Vui lòng reset!", "Dangerous", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Text Files|*.txt",
                Title = "Select a File"
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = openFileDialog.FileName;
                try
                {
                    var fileContent = File.ReadAllLines(filePath); // Đọc từng dòng trong file
                    if (fileContent.Length < 1)
                    {
                        MessageBox.Show("File trống hoặc không hợp lệ.", "Error");
                        return;
                    }

                    // Lấy chiều dài ma trận từ dòng đầu tiên
                    if (!int.TryParse(fileContent[0], out int length) || length < 1)
                    {
                        MessageBox.Show("Dữ liệu file không đúng định dạng. Chiều dài ma trận phải là số nguyên dương.", "Error");
                        return;
                    }

                    // Kiểm tra tổng số dòng file đủ để chứa ma trận
                    if (fileContent.Length < 1 + length)
                    {
                        MessageBox.Show("File không đủ dữ liệu để tạo graph.", "Error");
                        return;
                    }

                    // Tạo ma trận từ dữ liệu file
                    //int[,] edgesFile = new int[length, length];
                    for (int i = 0; i < length; i++)
                    {
                        var line = fileContent[i + 1].Split(' '); // Tách từng giá trị bằng dấu cách
                        if (line.Length != length)
                        {
                            MessageBox.Show($"Dòng {i + 2} không có đủ giá trị ({length} cột).", "Error");
                            return;
                        }

                        for (int j = 0; j < length; j++)
                        {
                            if (!int.TryParse(line[j], out int value))
                            {
                                MessageBox.Show($"Giá trị không hợp lệ tại dòng {i + 2}, cột {j + 1}.", "Error");
                                return;
                            }
                            int max = Math.Max(i, j);
                            int min = Math.Min(i, j);
                            if (value != 0) edges[(min, max, defaultColor)] = value;
                        }
                        CreateNodeRandom();
                    }

                    Board.Invalidate();
                    CreateAdjMatrix();
                    CreateWeiMatrix();

                    StartNode.Maximum = num - 1;
                    EndNode.Maximum = num - 1;

                    ChangeText();
                    MessageBox.Show("File đã được tải thành công và ma trận đã được xử lý!", "Success");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Đã xảy ra lỗi: {ex.Message}", "Error");
                }
            }

        }
        private void saveFiles(object sender, EventArgs e)
        {

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                FileName = "Graph" + num.ToString() + ".txt", // Đặt tên mặc định là tên file gốc
                Filter = "Text Files|*.txt",
                Title = "Save File To"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string savePath = saveFileDialog.FileName;

                try
                {
                    using (StreamWriter writer = new StreamWriter(savePath))
                    {
                        // Ghi kích thước ma trận
                        writer.WriteLine(num);

                        foreach (Guna2Button txt in weiMatrixPanel.Controls)
                        {
                            var indices = (ValueTuple<int, int>)txt.Tag;
                            string content = txt.Text == "\u221E" ? "0" : txt.Text;
                            writer.Write(content);
                            if (indices.Item2 == num - 1) writer.WriteLine();
                            if (indices.Item2 < num - 1) writer.Write(" ");
                        }
                    }
                    MessageBox.Show($"File đã được lưu tại: {savePath}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Đã xảy ra lỗi khi lưu file: {ex.Message}");
                }
            }
        }
        private void saveGph_Click(object ssender, EventArgs e)
        {

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                FileName = "Graph" + num.ToString() + ".gph", // Đặt tên mặc định là tên file gốc
                Filter = "Text Files|*.gph",
                Title = "Save File To"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string savePath = saveFileDialog.FileName;

                try
                {
                    using (StreamWriter writer = new StreamWriter(savePath))
                    {
                        // Ghi kích thước ma trận
                        writer.WriteLine(num);

                        for (int i = 0; i < nodes.Count; i++)
                        {
                            writer.Write(nodes[i].Location.X + " " + nodes[i].Location.Y);
                            writer.WriteLine();
                        }
                        writer.WriteLine(edges.Count);
                        foreach (var edge in edges)
                        {
                            writer.Write(edge.Key.Item1 + " " + edge.Key.Item2 + " " + edge.Value);
                            writer.WriteLine();
                        }
                    }
                    MessageBox.Show($"File đã được lưu tại: {savePath}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Đã xảy ra lỗi khi lưu file: {ex.Message}");
                }
            }
        }
        private void loadgph_Click(object sender, EventArgs e)
        {
            if (num != 0)
            {
                MessageBox.Show("Vui lòng reset!", "Dangerous", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Text Files|*.gph",
                Title = "Select a File"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = openFileDialog.FileName;

                try
                {
                    var fileContent = File.ReadAllLines(filePath); // Đọc từng dòng trong file
                    if (fileContent.Length < 1)
                    {
                        MessageBox.Show("File trống hoặc không hợp lệ.", "Error");
                        return;
                    }

                    // Lấy chiều dài ma trận từ dòng đầu tiên
                    if (!int.TryParse(fileContent[0], out int length) || length < 1)
                    {
                        MessageBox.Show($"Dữ liệu file không đúng định dạng. Chiều dài ma trận phải là số nguyên dương.s", "Error");
                        return;
                    }

                    // Kiểm tra tổng số dòng file đủ để chứa ma trận
                    if (fileContent.Length < 1 + length)
                    {
                        MessageBox.Show("File không đủ dữ liệu để tạo ma trận.", "Error");
                        return;
                    }

                    // Tạo ma trận từ dữ liệu files
                    for (int i = 0; i < length; i++)
                    {
                        var line = fileContent[i + 1].Split(' '); // Tách từng giá trị bằng dấu cách
                        int x = int.Parse(line[0]);
                        int y = int.Parse(line[1]);
                        CreateNodeGph(new Point(x, y));
                    }

                    int length1 = int.Parse(fileContent[1 + length]);

                    for (int i = 0; i < length1; i++)
                    {
                        var line = fileContent[i + 2 + length].Split(' '); // Tách từng giá trị bằng dấu cách
                        int x = int.Parse(line[0]);
                        int y = int.Parse(line[1]);
                        int z = int.Parse(line[2]);
                        edges[(x, y, defaultColor)] = z;
                    }

                    CreateAdjMatrix();
                    CreateWeiMatrix();
                    ChangeText();

                    StartNode.Maximum = num - 1;
                    EndNode.Maximum = num - 1;

                    Board.Invalidate();
                    MessageBox.Show("File đã được tải thành công và ma trận đã được xử lý!", "Success");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Đã xảy ra lỗi: {ex.Message}", "Error");
                }
            }
        }
        private void SavePng(object sender, EventArgs e)
        {
            Bitmap bitmap = new Bitmap(Board.Width, Board.Height);

            Board.DrawToBitmap(bitmap, new Rectangle(0, 0, Board.Width, Board.Height));

            // Lưu ảnh ra file (nếu cần)
            using (SaveFileDialog saveFileDialogs = new SaveFileDialog())
            {
                saveFileDialogs.Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp";
                saveFileDialogs.Title = "Save Captured Image";
                saveFileDialogs.FileName = "CapturedPanel.png"; // Tên mặc định

                if (saveFileDialogs.ShowDialog() == DialogResult.OK)
                {
                    // Lưu file tới đường dẫn mà người dùng chọn
                    bitmap.Save(saveFileDialogs.FileName);

                    MessageBox.Show($"File đã được lưu tại: {saveFileDialogs.FileName}");
                }
            }

            // Giải phóng tài nguyên bitmap
            bitmap.Dispose();
        }

        ///

        ///

        private void Board_Paint(object sender, PaintEventArgs e)
        {
            //Board.OnPaint(e);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            foreach (var edge in edges.Keys)
            {

                if (edge.Item1 > edge.Item2 && undirect.Checked) continue;
                var node1 = nodes[edge.Item1];
                var node2 = nodes[edge.Item2];
                PointF point1 = new PointF(node1.Left + node1.Width / 2, node1.Top + node1.Height / 2);
                PointF point2 = new PointF(node2.Left + node2.Width / 2, node2.Top + node2.Height / 2);


                using (Pen pen = new Pen(edge.Item3, 3))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;

                    // Tính toán vector hướng từ point1 đến point2
                    float deltaX = point2.X - point1.X;
                    float deltaY = point2.Y - point1.Y;

                    // Tính toán độ dài của vector hướng
                    float vectorLength = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

                    // Chuẩn hóa vector hướng
                    float normalizedDeltaX = deltaX / vectorLength;
                    float normalizedDeltaY = deltaY / vectorLength;

                    // Tính toán endPoint mới, thụt vào 30 pixel (bán kính hình tròn)
                    float radius = 30; // Bán kính hình tròn
                    PointF endPoint = new PointF(
                        point2.X - radius * normalizedDeltaX,
                        point2.Y - radius * normalizedDeltaY
                    );

                    // Tính toán góc của vector
                    float angle = (float)Math.Atan2(endPoint.Y - point1.Y, endPoint.X - point1.X);

                    if (direct.Checked)
                    {
                        // Tính toán tọa độ của hai điểm tạo mũi tên
                        float arrowSize = 10; // Kích thước mũi tên
                        PointF arrowPoint1 = new PointF(
                            endPoint.X - arrowSize * (float)Math.Cos(angle - Math.PI / 6),
                            endPoint.Y - arrowSize * (float)Math.Sin(angle - Math.PI / 6)
                        );
                        PointF arrowPoint2 = new PointF(
                            endPoint.X - arrowSize * (float)Math.Cos(angle + Math.PI / 6),
                            endPoint.Y - arrowSize * (float)Math.Sin(angle + Math.PI / 6)
                        );

                        // Vẽ hai đường thẳng tạo mũi tên
                        e.Graphics.DrawLine(pen, endPoint, arrowPoint1);
                        e.Graphics.DrawLine(pen, endPoint, arrowPoint2);
                    }

                    // Vẽ đường thẳng chính
                    e.Graphics.DrawLine(pen, point1, endPoint); // Vẽ đến endPoint mới
                }
                PointF midpoint = new PointF((point1.X + point2.X) / 2, (point1.Y + point2.Y) / 2);


                string edgeWeight = edges[(edge.Item1, edge.Item2, defaultColor)].ToString();
                using (System.Drawing.Font font = new System.Drawing.Font("Robot", 10, FontStyle.Bold))
                {
                    e.Graphics.DrawString(edgeWeight, font, Brushes.Chocolate, midpoint);
                }
            }
        }

        private void Board_MouseDown(object sender, MouseEventArgs e)
        {
            CreateNode(e.Location);
        }

        ///

        ///

        private void Reset_Click(object sender, EventArgs e)
        {
            Undo_Redo();
            Board.Controls.Clear();
            adjList.Clear();
            edges.Clear();
            nodes.Clear();
            Board.Invalidate();
            adjMatrixPanel.Controls.Clear();
            weiMatrixPanel.Controls.Clear();
            num = 0;
            chosenNode = null;
        }

        private void TrackBar_Value(object sender, EventArgs e)
        {
            timeRun.Text = (TrackBar.Value).ToString() + "s";
        }

        private void ChoseAlgorithm(object sender, EventArgs e)
        {
            ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;
            Algo.Text = toolStripMenuItem.Text.ToString();
        }

        private void btnColor(object sender, EventArgs e)
        {
            Guna2Button btn = (Guna2Button)sender;
            using (ColorDialog colorDialog = new ColorDialog())
            {
                // Hiển thị hộp thoại
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    // Sử dụng màu được chọn
                    btn.FillColor = colorDialog.Color; // Đổi màu nền của Form
                }
            }
        }
        private void LoadAdjList()
        {
            adjList = new List<List<int>>();
            for (int i = 0; i < num; i++)
            {
                List<int> list = new List<int>();
                adjList.Add(list);
            }
            foreach (var edge in edges.Keys)
            {
                if (undirect.Checked)
                {
                    adjList[edge.Item1].Add(edge.Item2);
                    adjList[edge.Item2].Add(edge.Item1);
                }
                else
                {
                    adjList[edge.Item1].Add(edge.Item2);
                }
            }
            foreach (var adj in adjList)
            {
                adj.Sort();
            }
        }
        async private void Run_Click(object sender, EventArgs e)
        {

            drawModeRadioBtn.Checked = true;
            if (Run.Text == "Close")
            {
                for (int i = 0; i < num; ++i)
                {
                    nodes[i].FillColor = Color.FromArgb(94, 148, 255);
                }
                Board.Invalidate();
                Reset.Enabled = Undo.Enabled = Redo.Enabled = true;
                foreach (Control control in Work.Controls)
                {
                    if (control is RadioButton radio)
                    {
                        radio.Enabled = false;
                    }
                }

                Run.Text = "Run";
                return;
            }
            LoadAdjList();
            int time = TrackBar.Value * 1000;
            Color nodeColor = Color.FromArgb(94, 148, 255);
            Color visNodeColor = Color1.FillColor;
            Color bestNodeColor = Color2.FillColor;
            Color completedColor = Color3.FillColor;
            int start = int.Parse(StartNode.Value.ToString());
            int end = int.Parse(EndNode.Value.ToString());
            if (start == end && Algo.Text != "Kruskal" && Algo.Text != "Prim" && Algo.Text != "None")
            {
                MessageBox.Show("Đỉnh xuất phát không được trùng với đỉnh kết thức");
                return;
            }
            forceModeRadioBtn.Checked = false;
            drawModeRadioBtn.Checked = true;
            Run.Enabled = Undo.Enabled = Redo.Enabled = false;
            foreach (Control control in Work.Controls)
            {
                if (control is RadioButton radio)
                {
                    radio.Enabled = false;
                }
            }

            Dictionary<(int, int, Color), int> edgesCopy = new Dictionary<(int, int, Color), int>(edges);
            switch (Algo.Text.ToString())
            {
                case "A*":
                    await AStar.Algorithm(num, start, end, adjList, nodes, edges, nodeColor, visNodeColor, bestNodeColor, completedColor, time, Log, Board);
                    edges = edgesCopy;
                    break;
                case "Dijkstra":
                    await Dijkstra.Algorithm(num, start, end, adjList, nodes, edges, nodeColor, visNodeColor, bestNodeColor, completedColor, time, Log, Board);
                    edges = edgesCopy;
                    break;
                case "DFS":
                    await DFS.Algorithm(num, start, end, adjList, nodes, nodeColor, visNodeColor, bestNodeColor, completedColor, time, Log, Board, edges);
                    edges = edgesCopy;
                    break;
                case "BFS":
                    await BFS.Algorithm(num, start, end, adjList, nodes, nodeColor, visNodeColor, bestNodeColor, completedColor, time, Log, Board, edges);
                    edges = edgesCopy;
                    break;
                case "Kruskal":
                    await Kruskal.Algorithm(num, edges, visNodeColor, bestNodeColor, completedColor, time, Log, Board);
                    edges = edgesCopy;
                    break;
                case "Prim":
                    await Prim.Algorithm(num, adjList, nodes, edges, visNodeColor, bestNodeColor, completedColor, time, Log, Board);
                    edges = edgesCopy;
                    break;
                default:
                    MessageBox.Show("Vui lòng chọn thuật toán");
                    Run.Enabled = Undo.Enabled = Redo.Enabled = true;
                    foreach (Control control in Work.Controls)
                    {
                        if (control is RadioButton radio)
                        {
                            radio.Enabled = true;
                        }
                    }

                    return;

            }
            MessageBox.Show("Algorithm is completed!", "Success");
            Run.Enabled = true;
            Run.Text = "Close";
        }
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = tabControl1.SelectedIndex;
            if (idx == 3)
            {
                LoadAdjList();
                adjListShow.Clear();
                for (int i = 0; i < adjList.Count; ++i)
                {
                    adjListShow.AppendText(i.ToString() + " -> ");
                    foreach (int j in adjList[i])
                    {
                        adjListShow.AppendText($"[{j} ]");
                    }
                    adjListShow.AppendText("\n");
                }
            }
        }

        ///

        ///
        private void ForceModeTimer_Tick(object sender, EventArgs e)
        {
            if (num > 1 && forceModeRadioBtn.Checked)
            {
                LoadAdjList();
                ForceMode.ApplyForce(10, 0.001, 1, F, nodes,edges, num, Board, draggingNode, adjList);
                Board.Invalidate();
            }
        }


        ///
     
        ///
        private void Undo_Click(object sender, EventArgs e)
        {
            if (undo.Count == 0) return;
            Undo_Redo(1);
            nodes = new List<Guna2CircleButton>(undo.Peek().Item1);
            edges = new Dictionary<(int, int, Color), int>(undo.Peek().Item2);
            Board.Controls.Clear();
            num = nodes.Count;

            foreach (var node in nodes)
            {
                Board.Controls.Add(node);
            }

            undo.Pop();
            Board.Invalidate();
            CreateAdjMatrix();
            CreateWeiMatrix();
            ChangeText();
        }
        private void Undo_Redo(int k = 0)
        {
            List<Guna2CircleButton> nodes_1 = new List<Guna2CircleButton>();
            foreach (var node in nodes)
            {
                Guna2CircleButton btn = new Guna2CircleButton
                {
                    BackColor = Color.Transparent,
                    Size = new Size(60, 60),
                    Location = node.Location,
                    Text = nodes_1.Count.ToString(),
                    DisabledState = { FillColor = Color.Gray, BorderColor = Color.White, ForeColor = Color.White }
                };

                btn.Click += Choosebtn;
                btn.MouseDown += btn_MouseDown;
                btn.MouseMove += btn_MouseMove;
                btn.MouseUp += btn_MouseUp;

                // Thiết lập vùng hiển thị hình tròn
                GraphicsPath path = new GraphicsPath();
                path.AddEllipse(0, 0, btn.Width, btn.Height);
                btn.Region = new Region(path);

                nodes_1.Add(btn);

                F.Add(new PointF(0, 0));

                StartNode.Maximum = num - 1;
                EndNode.Maximum = num - 1;
            }
            Dictionary<(int, int, Color), int> edgesCopy = new Dictionary<(int, int, Color), int>(edges);
            if (k == 1) redo.Push((nodes_1, edgesCopy));
            else
            {
                undo.Push((nodes_1, edgesCopy));
                redo.Clear();
            }
        }

        private void Redo_Click(object sender, EventArgs e)
        {
            if (redo.Count == 0) return;
            Undo_Redo();
            nodes = new List<Guna2CircleButton>(redo.Peek().Item1);
            edges = new Dictionary<(int, int, Color), int>(redo.Peek().Item2);
            Board.Controls.Clear();
            num = nodes.Count;

            foreach (var node in nodes)
            {
                Board.Controls.Add(node);
            }

            redo.Pop();
            Board.Invalidate();
            CreateAdjMatrix();
            CreateWeiMatrix();
            ChangeText();
        }
        /*
         
         */
        #region Vohuong Cohuong
        private void undirect_Click(object sender, EventArgs e)
        {
            if (num != 0)
            {
                MessageBox.Show("Vui lòng reset!", "Dangerous", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                undirect.Checked = false;
                direct.Checked = true;
                return;
            }
        }

        private void direct_Click(object sender, EventArgs e)
        {
            if (num != 0)
            {
                MessageBox.Show("Vui lòng reset!", "Dangerous", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                direct.Checked = false;
                undirect.Checked = true;
                return;
            }
        }
        #endregion
    }
}
