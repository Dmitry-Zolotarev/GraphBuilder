using System;
using System.Drawing;
using System.Windows.Forms;
using Mathos.Parser;
using System.Text.RegularExpressions;

namespace SKV
{
    public partial class Form1 : Form
    {
        private MathParser parser;
        double scale = 20, wheelspeed = 1;
        int centerX = 0, centerY = 0, offsetX = 0, offsetY = 0; // Смещение графика
        Point lastMousePos; // Последняя позиция мыши при перетаскивании

        public Form1()
        {
            InitializeComponent();
            pictureBox1.BackColor = Color.White;

            // Инициализация парсера
            parser = new MathParser();

            // Добавляем кастомную функцию pow(x, y)
            parser.LocalFunctions["pow"] = args => Math.Pow(args[0], args[1]);
            buildGraph();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            offsetX = 0;
            offsetY = 0;
            buildGraph();
        }
        private double f(double x)
        {
            double y = 0;
            try
            {
                parser.LocalVariables["x"] = x;
                var function = Regex.Replace(textBox1.Text, @"([a-zA-Z0-9_\.]+)\s*\^\s*([0-9\.]+)", "pow($1,$2)");
                y = parser.Parse(function);
            }
            catch(Exception) {}
            return y;
        }

        

        private void buildGraph()
        {
            try
            {
                label3.Text = "y = " + textBox1.Text;
                var function = Regex.Replace(textBox1.Text, @"([a-zA-Z0-9_\.]+)\s*\^\s*([0-9\.]+)", "pow($1,$2)");
                int w = pictureBox1.Width / 2, h = pictureBox1.Height / 2;
                Bitmap bmp = new Bitmap(w, h);
                Graphics g = Graphics.FromImage(bmp);
                pictureBox1.Width = bmp.Width * 2;
                pictureBox1.Height = bmp.Height * 2;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.Clear(Color.White);

                centerX = w / 2 + offsetX;
                centerY = h / 2 + offsetY;

                // Рисуем оси
                g.DrawLine(new Pen(Color.Gray, 1), 0, centerY, w, centerY); // X-axis
                g.DrawLine(new Pen(Color.Gray, 1), centerX, 0, centerX, h); // Y-axis


                // Отрисовка делений по оси X
                double step = 60 / scale; // адаптивный шаг, можно сделать динамическим
                double xStart = -((centerX) / scale);
                double xEnd = (w - centerX) / scale;

                for (double x = Math.Floor(xStart / step) * step; x <= xEnd; x += step)
                {
                    int screenX = (int)(centerX + x * scale);
                    g.DrawLine(Pens.LightGray, screenX, centerY - 5, screenX, centerY + 5);

                    if (Math.Abs(x) > 1e-6)
                        g.DrawString(Math.Round(x / 3, 3).ToString(), this.Font, Brushes.Black, screenX + 2, centerY + 5);
                }

                // Отрисовка делений по оси Y
                double yStart = -((h - centerY) / scale);
                double yEnd = (centerY) / scale;

                for (double y = Math.Floor(yStart / step) * step; y <= yEnd; y += step)
                {
                    int screenY = (int)(centerY - y * scale);
                    g.DrawLine(Pens.LightGray, centerX - 5, screenY, centerX + 5, screenY);

                    if (Math.Abs(y) > 1e-6)
                        g.DrawString(Math.Round(y / 3, 2).ToString(), this.Font, Brushes.Black, centerX + 5, screenY - 10);
                }

                // Рисуем график функции
                for (int px = 0; px < w - 1; px++)
                {
                    double x1 = (px - centerX), x2 = (px + 1 - centerX);


                    parser.LocalVariables["x"] = x1 / scale;
                    double y1 = parser.Parse(function);

                    parser.LocalVariables["x"] = x2 / scale;
                    double y2 = parser.Parse(function);

                    int py1 = centerY - (int)(y1 * scale), py2 = centerY - (int)(y2 * scale);

                    if (py1 >= 0 && py1 < h && py2 >= 0 && py2 < h)
                        g.DrawLine(new Pen(Color.Blue, 2), px, py1, px + 1, py2);
                    
                }
                // Отображаем полученное изображение на PictureBox
                pictureBox1.Image = bmp;
            }
            catch (Exception)
            {
                MessageBox.Show("Ошибка: неправильно введена функция!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Form1_Resize(object sender, EventArgs e) => buildGraph();

        private void textBox1_Leave(object sender, EventArgs e) => buildGraph();

        private void textBox1_KeyDown(object sender, KeyEventArgs e) { 
            if(e.KeyCode == Keys.Enter)buildGraph();
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                toolTip1.Hide(pictureBox1);            
                offsetX += (e.X - lastMousePos.X) / 2;
                offsetY += (e.Y - lastMousePos.Y) / 2;
                buildGraph();

            }
            else
            {
                double x = (e.X / 2 - centerX + offsetX) / scale / 3;
                toolTip1.Show($"f({Math.Round(x, 3)}) = {Math.Round(f(x), 3)}", pictureBox1, lastMousePos.X + 15, lastMousePos.Y + 15, 1000);
            }
            lastMousePos = e.Location;
        }
        private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (scale > 1 && e.Delta < 0 || scale < 1000 && e.Delta >= 0) scale += e.Delta * wheelspeed / 100;
            label2.Text = "Масштаб: " + Math.Round(scale / 20, 2) + "x";
            buildGraph();
        }

    }
}
