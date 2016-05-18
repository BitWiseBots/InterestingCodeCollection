using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Sandbox.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const double BaseLength = 1;
        private const double ScaleFactor = 1.102;

        private readonly double _centerX;
        private readonly double _centerY;

        private readonly List<Polygon> _previousTriangles;

        public MainWindow()
        {
            _previousTriangles = new List<Polygon>();
            InitializeComponent();

            _centerX = drawingCanvas.Width / 2;
            _centerY = drawingCanvas.Height / 2;

            for (var i = 0; i < 100; i++)
            {
                AddTriangle();
            }
        }

        private void AddTriangle()
        {
            var newTriangle = new Polygon
            {
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                Fill = Brushes.Black,
                Opacity = 0.5
            };

            if (_previousTriangles.Count == 0)
            {
                newTriangle.Points.Add(new Point(_centerX, _centerY));
                newTriangle.Points.Add(new Point(_centerX, _centerY + BaseLength));
                newTriangle.Points.Add(new Point(_centerX + BaseLength, _centerY));
            }
            else
            {
                var prev = _previousTriangles.Last();
                var prevAngle = Math.Atan2(prev.Points[0].Y - prev.Points[1].Y, prev.Points[0].X - prev.Points[1].X) * 180 / Math.PI;
                var newAngle = prevAngle + 45;

                var newLength = BaseLength * Math.Pow(ScaleFactor, _previousTriangles.Count);

                var newBX = Math.Cos(newAngle * Math.PI / 180) * newLength + prev.Points[0].X;
                var newBY = Math.Sin(newAngle * Math.PI / 180) * newLength + prev.Points[0].Y;
                var newCX = Math.Cos((newAngle + 90) * Math.PI / 180) * newLength + newBX;
                var newCY = Math.Sin((newAngle + 90) * Math.PI / 180) * newLength + newBY;

                newTriangle.Points.Add(new Point(newBX, newBY));
                newTriangle.Points.Add(new Point(prev.Points[0].X, prev.Points[0].Y));

                newTriangle.Points.Add(new Point(newCX, newCY));
            }

            drawingCanvas.Children.Add(newTriangle);
            _previousTriangles.Add(newTriangle);
        }
    }
}
