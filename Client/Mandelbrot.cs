using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Client
{
    class Mandelbrot
    {
        public Point bottomLeft { get; set; }
        public Point topRight { get; set; }

        private WriteableBitmap bitmap;
        public byte[] pixels { get; set; }
        public int iterations { get; set; } = 100;

        public Mandelbrot(int width, int height)
        {
            bitmap = new WriteableBitmap(width, height, 72, 72, PixelFormats.Rgb24, null);
            Matrix matrix = new Matrix(width / (double)width, 0, 0, height/ (double)width, 0, 0);
            bottomLeft = new Point(-2, -2);
            topRight = new Point(2, 2);

            bottomLeft = matrix.Transform(bottomLeft);
            topRight = matrix.Transform(topRight);
            

        }

        public void setPoints(Point p1, Point p2)
        {
            bottomLeft = p1;
            topRight = p2;

        }

        public void resizeBitmap(int width, int height)
        {
            bitmap = new WriteableBitmap(width, height, 72, 72, PixelFormats.Rgb24, null);
            Matrix matrix = new Matrix(width / (double)width, 0, 0, height / (double)width, 0, 0);
            bottomLeft = new Point(-2, -2);
            topRight = new Point(2, 2);

            bottomLeft = matrix.Transform(bottomLeft);
            topRight = matrix.Transform(topRight);
        }

        public void calculate()
        {
            Int32Rect rect = new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight);
            pixels = new byte[bitmap.PixelHeight * bitmap.PixelWidth * 3];


            for (int y = 0; y < bitmap.PixelHeight; ++y)
            {
                for (int x = 0; x < bitmap.PixelWidth; ++x)
                {
                    double real = bottomLeft.X + ((x * 1.0d) / bitmap.PixelWidth) * (topRight.X - bottomLeft.X);
                    double imaginary = bottomLeft.Y + ((y * 1.0d) / bitmap.PixelHeight) * (topRight.Y - bottomLeft.Y);
                    Complex c = new Complex(real, imaginary);

                    Complex z = 0;
                    int m = 0;
                    while (Complex.Abs(z) <= 2 && m < iterations)
                    {
                        z = z * z + c;
                        m++;
                    }

                    //int color = 255 * x / bitmap.PixelWidth;
                    int color = (int)((255) * (m * 1.0f) / iterations) % 255;


                    //RED
                    pixels[0 + 3 * (x + bitmap.PixelWidth * y)] = (byte)(color);
                    //GREEN
                    pixels[1 + 3 * (x + bitmap.PixelWidth * y)] = (byte)(color);
                    //BLUE
                    pixels[2 + 3 * (x + bitmap.PixelWidth * y)] = (byte)(color);

                    //if (x == bitmap.PixelWidth / 2)
                    //{
                    //    pixels[0 + 3 * (x + bitmap.PixelWidth * y)] = (byte)(255);
                    //}
                    //if (y == bitmap.PixelHeight / 2)
                    //{
                    //    pixels[2 + 3 * (x + bitmap.PixelWidth * y)] = (byte)(255);
                    //}
                    //if (y == 100)
                    //{
                    //    pixels[1 + 3 * (x + bitmap.PixelWidth * y)] = (byte)(255);
                    //}


                }
            }

            bitmap.WritePixels(rect, pixels, 3 * bitmap.PixelWidth, 0);
        }

        public WriteableBitmap GetBitmap()
        {
            return bitmap;
        }

        public void SaveToFile(string filename)
        {
            if (filename != string.Empty)
            {
                using (FileStream stream = new FileStream(filename, FileMode.Create))
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    //encoder.Frames.Add(BitmapFrame.Create(bitmap.Clone()));
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    encoder.Save(stream);
                }
            }
        }

        public void setRecommendedIterations()
        {
            double min = Math.Min(Math.Abs(topRight.X-bottomLeft.X),Math.Abs(topRight.Y - bottomLeft.Y));
            iterations = (int) (Math.Log(1 / min) * 40 + 100);
            Console.WriteLine(iterations);
        }

    }
}
