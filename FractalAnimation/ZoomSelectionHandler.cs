using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FractalAnimation
{
    class ZoomSelectionHandler
    {
        public Canvas referenceCanvas;
        public Rectangle zoomingRectangle { get; }

        private bool isZooming;

        public Point rectStart;

        public Point rectEnd;

        public double ratioXy { get; set; }

        public double ratioYx { get; set; }

        public ZoomSelectionHandler(Canvas referenceCanvas)
        {
            this.referenceCanvas = referenceCanvas;
            this.ratioXy = referenceCanvas.Width / referenceCanvas.Height;
            this.ratioYx = referenceCanvas.Height / referenceCanvas.Width;
            
            zoomingRectangle = new Rectangle
            {
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                Fill = new SolidColorBrush(Color.FromArgb(100, 170, 170, 255)),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Height = 0,
                Width = 0
            };
        }

        public void StartSelection(MouseEventArgs e)
        {
            isZooming = true;

            rectStart.X = e.GetPosition(referenceCanvas).X;
            rectStart.Y = e.GetPosition(referenceCanvas).Y;

            rectEnd.X = rectStart.X;
            rectEnd.Y = rectStart.Y;

            zoomingRectangle.Width = 0;
            zoomingRectangle.Height = 0;
            zoomingRectangle.Margin = new Thickness(rectStart.X, rectStart.Y, 0, 0);
        }

        public void MoveSelection(MouseEventArgs e)
        {
            if (isZooming)
            {
                rectEnd.X = e.GetPosition(referenceCanvas).X;
                rectEnd.Y = e.GetPosition(referenceCanvas).Y;

                var newWidth = (int)(rectEnd.X - rectStart.X);
                var newHeight = (int)(rectEnd.Y - rectStart.Y);

                if (newWidth > 0 && newHeight > 0)
                {
                    (zoomingRectangle.Width, zoomingRectangle.Height) = FixRatio(newWidth, newHeight);
                }
                else
                {
                    (zoomingRectangle.Width, zoomingRectangle.Height) = (0, 0);
                }
            }
        }


        public void AbortSelection()
        {
            isZooming = false;

            zoomingRectangle.Width = 0;
            zoomingRectangle.Height = 0;
        }

        private (int width, int height) FixRatio(int width, int height)
        {
            if (Math.Abs(width) > Math.Abs(height)) height = (int)(width * ratioYx);
            else width = (int)(height * ratioXy);

            return (width, height);
        }
    }
}
