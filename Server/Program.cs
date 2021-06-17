using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Server
{
    class Program
    {
        private static void SaveImage(string filename, BitmapSource image)
        {
            if (filename != string.Empty)
            {
                using (FileStream stream = new FileStream(filename, FileMode.Create))
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(image));
                    encoder.Save(stream);
                }
            }
        }
        public static void Main()
        {
            try
            {
                WriteableBitmap bitmap = new WriteableBitmap(1280, 720, 72, 72, PixelFormats.Rgb24, null);

                IPAddress ipAd = IPAddress.Parse("127.0.0.1");
                // use local m/c IP address, and 
                // use the same in the client

                /* Initializes the Listener */
                TcpListener server = new TcpListener(ipAd, 8001);

                /* Start Listeneting at the specified port */
                server.Start();

                Console.WriteLine("The server is running at port 8001...");
                Console.WriteLine("The local End point is  :" +
                                  server.LocalEndpoint);
                Console.WriteLine("Waiting for a connection.....");

                Socket socket = server.AcceptSocket();
                Console.WriteLine("Connection accepted from " + socket.RemoteEndPoint);

                Matrix matrix = new Matrix(bitmap.PixelWidth / (double)bitmap.PixelWidth, 0, 0, bitmap.PixelHeight / (double)bitmap.PixelWidth, 0, 0);

                //Point pointStart = new Point(-2, -2);
                //Point pointEnd = new Point(2, 2);

                Point pointStart = new Point(-0.800338731554, -0.207153842533);
                Point pointEnd = new Point(pointStart.X + 0.10, pointStart.Y + 0.10);


                pointStart = matrix.Transform(pointStart);
                pointEnd = matrix.Transform(pointEnd);



                byte[] keyframe = new byte[40];
                BitConverter.GetBytes(bitmap.PixelWidth).CopyTo(keyframe, 0);
                BitConverter.GetBytes(bitmap.PixelHeight).CopyTo(keyframe, 4);
                BitConverter.GetBytes(pointStart.X).CopyTo(keyframe,8);
                BitConverter.GetBytes(pointStart.Y).CopyTo(keyframe,16);
                BitConverter.GetBytes(pointEnd.X).CopyTo(keyframe,24);
                BitConverter.GetBytes(pointEnd.Y).CopyTo(keyframe,32);

                socket.Send(keyframe);
                Console.WriteLine("Sent Keyframe data");

                byte[] pixels = new byte[bitmap.PixelHeight * bitmap.PixelWidth * 3];
                int msgLength = socket.Receive(pixels);
                Console.WriteLine("Recieved..."+msgLength+" bytes");

                /* clean up */
                socket.Close();
                server.Stop();

                Int32Rect rect = new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight);
                bitmap.WritePixels(rect, pixels, 3 * bitmap.PixelWidth, 0);
                SaveImage("mandelbrot.png", bitmap);
                Console.WriteLine("Image saved");
                Console.WriteLine("Press any key to exit");

                Console.ReadKey();

            }
            catch (Exception e)
            {
                Console.WriteLine("Error..... " + e.StackTrace);
            }
        }
    }
}


