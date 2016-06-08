namespace ChaosGeneStructure.Main
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Win32;
    using Color = System.Drawing.Color;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string fileName = string.Empty;
        private readonly BackgroundWorker worker = new BackgroundWorker();

        public MainWindow()
        {
            InitializeComponent();

            this.ProcessingText.Visibility = Visibility.Hidden;
            this.worker.DoWork += Worker_DoWork;
            this.worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
        }

        private void Process_Click(object sender, RoutedEventArgs e)
        {
            this.ProcessingText.Visibility = Visibility.Visible;
            this.Process.IsEnabled = false;
            this.LoadData.IsEnabled = false;

            this.worker.RunWorkerAsync();
        }

        public BitmapSource ConvertToBitmapSource(Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height, 96, 96, PixelFormats.Bgr24, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);

            return bitmapSource;
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.ImgBitmap.Source = e.Result as BitmapSource;

            this.worker.Dispose();

            this.ProcessingText.Visibility = Visibility.Hidden;
            this.Process.IsEnabled = true;
            this.LoadData.IsEnabled = true;
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BitmapSource bs = ComputeWithBitmap();

            bs.Freeze();

            e.Result = bs;
        }

        private BitmapSource ComputeWithBitmap()
        {
            int maxBounds = this.ImageStackPanel.ActualHeight > this.ImageStackPanel.ActualWidth ? (int)this.ImageStackPanel.ActualWidth : (int)this.ImageStackPanel.ActualHeight;
            var initialPoint = new Point(maxBounds / 2, maxBounds / 2);
            var data = ReadFile().ToArray();
            var verticesDictionary = InitializeBoundaryPoints(maxBounds);

            Bitmap image = new Bitmap((int)maxBounds, (int)maxBounds);
            ClearWithColor(image);
            
            DrawControlPoints(verticesDictionary, image);
            DrawGenePoints(initialPoint, data, image, verticesDictionary);

            return ConvertToBitmapSource(image);
        }

        private static Dictionary<char, Point> InitializeBoundaryPoints(int maxBounds)
        {
            var verticesDictionary = new Dictionary<char, Point>();

            var pointA = new Point(0, maxBounds - 2);
            var pointC = new Point(0, 0);
            var pointG = new Point(maxBounds - 2, 0);
            var pointT = new Point(maxBounds - 2, maxBounds - 2);

            verticesDictionary.Add('A', pointA);
            verticesDictionary.Add('C', pointC);
            verticesDictionary.Add('G', pointG);
            verticesDictionary.Add('T', pointT);

            return verticesDictionary;
        }

        private static void ClearWithColor(Bitmap image)
        {
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    image.SetPixel(x, y, Color.White);
                }
            }
        }

        private void LoadData_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                this.fileName = openFileDialog.FileName;
            }
        }

        private void DrawControlPoints(Dictionary<char, Point> verticesDictionary, Bitmap image)
        {
            Point[] points = { verticesDictionary['A'], verticesDictionary['C'], verticesDictionary['T'], verticesDictionary['G'] };

            for (int i = 0; i < points.Length; i++)
            {
                DrawPoint(points[i].X, points[i].Y, image);
            }
        }

        private void DrawGenePoints(Point initialPoint, char[] data, Bitmap image, Dictionary<char, Point> verts)
        {
            var point = initialPoint;

            DrawPoint(initialPoint.X, initialPoint.Y, image);

            for (int i = 0; i < data.Length; i++)
            {
                point = Midpoint(point, verts[data[i]]);
                DrawPoint(point.X, point.Y, image);
            }
        }

        private void DrawPoint(int x, int y, Bitmap image)
        {
            image.SetPixel(x, y, Color.Black);
        }

        public List<char> ReadFile()
        {
            if (string.IsNullOrEmpty(this.fileName))
            {
                MessageBox.Show("Please select a file to analyze first!", "No Data...", MessageBoxButton.OK);

                return new List<char>();
            }

            var reader = new StreamReader(this.fileName);
            var sequence = new List<char>();

            do
            {
                var ch = (char)reader.Read();
                var charValue = Convert.ToInt32(ch);
                if (charValue == 65 || charValue == 67 || charValue == 71 || charValue == 84)
                {
                    sequence.Add(ch);
                }
            } while (!reader.EndOfStream);

            reader.Close();
            reader.Dispose();

            return sequence;
        }

        public double Distance(Point a1, Point a2) => Math.Sqrt(Math.Pow((a2.X - a1.X), 2) + Math.Pow((a2.Y - a1.Y), 2));
        public Point Midpoint(Point a1, Point a2) => new Point(((a1.X + a2.X) / 2), ((a1.Y + a2.Y) / 2));
    }
}
