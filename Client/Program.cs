using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Client
{
    class Program
    {
        public static void Main()
        {

            try
            {
                Console.WriteLine("Server's IP address");
                String IP = Console.ReadLine();
                Console.WriteLine("Server's port");
                Int32 portNumber = Convert.ToInt32(Console.ReadLine());

                //Console.ReadKey();

                TcpClient client = new TcpClient();
                Console.WriteLine("Connecting.....");

                //client.Connect("127.0.0.1", 8001);
                try
                {
                    client.Connect(IP, portNumber);

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
                // use the ipaddress as in the server program

                Console.WriteLine("Connected");

                Stream stm = client.GetStream();


                bool doWork = true;
                while (doWork)
                {
                    try
                    {
                        byte[] buffer = new byte[52];
                        int k = stm.Read(buffer, 0, buffer.Length);
                        
                        Console.WriteLine("Read " + k + " bytes");
                        int byteCount = 0;
                        int iterations = BitConverter.ToInt32(buffer, byteCount);
                        byteCount += 4;
                        int width = BitConverter.ToInt32(buffer, byteCount);
                        byteCount += 4;
                        int height = BitConverter.ToInt32(buffer, byteCount);
                        byteCount += 4;
                        int numberOfFrames = BitConverter.ToInt32(buffer, byteCount);
                        byteCount += 4;
                        int frameNumber = BitConverter.ToInt32(buffer, byteCount);
                        byteCount += 4;
                        Point pointStart = new Point(BitConverter.ToDouble(buffer, byteCount), BitConverter.ToDouble(buffer, byteCount + 8));
                        byteCount += 16;
                        Point pointEnd = new Point(BitConverter.ToDouble(buffer, byteCount), BitConverter.ToDouble(buffer, byteCount + 8));
                        byteCount += 16;


                        //Console.WriteLine("Got data:\niterations: " + iterations + " width: " + width + "  height: " + height);
                        //Console.WriteLine("number of frames: " + numberOfFrames + " frame number: " + frameNumber);

                        //Console.WriteLine("Got two points:\n" + pointStart + " , " + pointEnd);

                        byte[] pixels = new byte[height * width * 3 + sizeof(int) + sizeof(long)];
                        

                        Mandelbrot mandelbrot = new Mandelbrot(width, height);
                        mandelbrot.iterations = iterations;
                        mandelbrot.setPoints(pointStart, pointEnd);
                        if (iterations == 0)
                        {
                            mandelbrot.setRecommendedIterations();
                        }
                        Stopwatch stopwatch = Stopwatch.StartNew();
                        mandelbrot.calculate();
                        stopwatch.Stop();
                        long frameCalculationTime = stopwatch.ElapsedMilliseconds;
                        BitConverter.GetBytes(frameCalculationTime).CopyTo(pixels, 0);
                        BitConverter.GetBytes(frameNumber).CopyTo(pixels, sizeof(long));

                        mandelbrot.pixels.CopyTo(pixels, sizeof(int) + sizeof(long));


                        //Console.WriteLine("Transmitting frame "+frameNumber+".....");
                        stm.Write(pixels, 0, pixels.Length);
                        Console.WriteLine("Frame "+frameNumber+" sent!");
                    }
                    catch(Exception e)
                    {
                        doWork = false;
                        Console.WriteLine("Connection closed: " + e.Message);
                    }
                }
                


                client.Close();
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
