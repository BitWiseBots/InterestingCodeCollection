using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Sandbox.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TaskScheduler _scheduler;

        public MainWindow()
        {
            InitializeComponent();
            _scheduler = TaskScheduler.FromCurrentSynchronizationContext();
        }

        private void DrawingCanvas_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(drawingCanvas);


            Task.Factory.StartNew(() => { })
                .ContinueWith((task, o) => PatternRenderer.DrawPattern(position, drawingCanvas), null, _scheduler);

            //Task.Run(() => PatternRenderer.DrawPattern(position, drawingCanvas));
        }
    }

    public class PatternRenderer
    {
        private const double BaseLength = 1;
        private const double ScaleFactor = 1.102;

        private double _centerX;
        private double _centerY;

        private int _currIterations;

        private readonly List<Polygon> _previousTriangles;
        private readonly Canvas _drawingCanvas;

        private PatternRenderer(Canvas canvas)
        {
            _previousTriangles = new List<Polygon>();
            _drawingCanvas = canvas;
        }

        public static async void DrawPattern(Point origin, Canvas canvas)
        {
            var renderer = new PatternRenderer(canvas);
            renderer._centerX = origin.X;
            renderer._centerY = origin.Y;

            renderer.AddTriangle(true);

            for (var i = 0; i < 99; i++)
            {
                renderer.AddTriangle(false);
                await Task.Delay(100);
            }
        }

        private void AddTriangle(bool isNew)
        {
            var newTriangle = new Polygon
            {
                Stroke = Brushes.Black,
                StrokeThickness = 1,
            };

            if (isNew)
            {
                _currIterations = 0;
                newTriangle.Points.Add(new Point(_centerX, _centerY));
                newTriangle.Points.Add(new Point(_centerX, _centerY + BaseLength));
                newTriangle.Points.Add(new Point(_centerX + BaseLength, _centerY));
            }
            else
            {
                var prev = _previousTriangles.Last();
                var prevAngle = Math.Atan2(prev.Points[0].Y - prev.Points[1].Y, prev.Points[0].X - prev.Points[1].X) * 180 / Math.PI;
                var newAngle = prevAngle + 45;

                var newLength = BaseLength * Math.Pow(ScaleFactor, _currIterations);

                var newBX = Math.Cos(newAngle * Math.PI / 180) * newLength + prev.Points[0].X;
                var newBY = Math.Sin(newAngle * Math.PI / 180) * newLength + prev.Points[0].Y;
                var newCX = Math.Cos((newAngle + 90) * Math.PI / 180) * newLength + newBX;
                var newCY = Math.Sin((newAngle + 90) * Math.PI / 180) * newLength + newBY;

                newTriangle.Points.Add(new Point(newBX, newBY));
                newTriangle.Points.Add(new Point(prev.Points[0].X, prev.Points[0].Y));

                newTriangle.Points.Add(new Point(newCX, newCY));
            }

            Dispatcher.CurrentDispatcher.Invoke(() => _drawingCanvas.Children.Add(newTriangle));
            
            _previousTriangles.Add(newTriangle);
            _currIterations++;
        }
    }
}
