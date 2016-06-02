namespace ChaosGeneStructure.Main
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Shapes;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const double MinBounds = 10;
        public Dictionary<char, Point> VerticesDictionary = new Dictionary<char, Point>();
        private readonly BackgroundWorker worker = new BackgroundWorker();

        public MainWindow()
        {
            InitializeComponent();

            this.ProcessingText.Visibility = Visibility.Hidden;
        }

        private void Process_Click(object sender, RoutedEventArgs e)
        {
            this.ProcessingText.Visibility = Visibility.Visible;
            this.Process.IsEnabled = false;
            this.LoadData.IsEnabled = false;

            this.worker.DoWork += Worker_DoWork;
            this.worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

            var maxBounds = this.Board.ActualHeight > this.Board.ActualWidth ? this.Board.ActualWidth : this.Board.ActualHeight;

            var pointA = new Point(MinBounds, maxBounds - MinBounds);
            var pointC = new Point(MinBounds, MinBounds);
            var pointG = new Point(maxBounds - MinBounds, MinBounds);
            var pointT = new Point(maxBounds - MinBounds, maxBounds - MinBounds);
            var initialPoint = new Point(((maxBounds - MinBounds) - MinBounds) / 2, ((maxBounds - MinBounds) - MinBounds) / 2);

            this.VerticesDictionary.Add('A', pointA);
            this.VerticesDictionary.Add('C', pointC);
            this.VerticesDictionary.Add('G', pointG);
            this.VerticesDictionary.Add('T', pointT);

            Point[] vertices = { pointA, pointC, pointT, pointG };
            var data = ReadFile().ToArray();

            DrawPoints(vertices);
            DrawLines(initialPoint, data);
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.ProcessingText.Visibility = Visibility.Hidden;
            this.Process.IsEnabled = true;
            this.LoadData.IsEnabled = true;
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            //var maxBounds = this.Board.ActualHeight > this.Board.ActualWidth ? this.Board.ActualWidth : this.Board.ActualHeight;

            //var pointA = new Point(MinBounds, maxBounds - MinBounds);
            //var pointC = new Point(MinBounds, MinBounds);
            //var pointG = new Point(maxBounds - MinBounds, MinBounds);
            //var pointT = new Point(maxBounds - MinBounds, maxBounds - MinBounds);
            //var initialPoint = new Point(((maxBounds - MinBounds) - MinBounds) / 2, ((maxBounds - MinBounds) - MinBounds) / 2);

            //this.VerticesDictionary.Add('A', pointA);
            //this.VerticesDictionary.Add('C', pointC);
            //this.VerticesDictionary.Add('G', pointG);
            //this.VerticesDictionary.Add('T', pointT);

            //Point[] vertices = { pointA, pointC, pointT, pointG };
            //var data = ReadFile().ToArray();

            //DrawPoints(vertices);
            //DrawLines(initialPoint, data);
        }

        private void LoadData_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DrawPoints(Point[] points)
        {
            for (int i = 0; i < points.Length; i++)
            {
                DrawPoint(points[i].X, points[i].Y);
            }
        }

        private void DrawLines(Point initialPoint, char[] data)
        {
            var point = initialPoint;
            DrawPoint(initialPoint.X, initialPoint.Y);
            for (int i = 0; i < data.Length; i++)
            {
                point = Midpoint(point, this.VerticesDictionary[data[i]]);
                DrawPoint(point.X, point.Y);
            }
        }

        private void DrawPoint(double x, double y)
        {
            Ellipse ellipse = new Ellipse();

            SolidColorBrush color = new SolidColorBrush { Color = Color.FromArgb(255, 255, 0, 0) };

            ellipse.Fill = color;
            ellipse.StrokeThickness = 0;
            ellipse.Stroke = Brushes.White;
            ellipse.Width = 2;
            ellipse.Height = 2;

            Canvas.SetTop(ellipse, y);
            Canvas.SetLeft(ellipse, x);

            this.Board.Children.Add(ellipse);
        }

        public List<char> ReadFile()
        {
            var reader = new StreamReader(@"HUMHBB_TestData.txt");
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
