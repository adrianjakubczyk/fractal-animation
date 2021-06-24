using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace FractalAnimation
{
    class Server
    {
        //static IPAddress ipAd = IPAddress.Parse("127.0.0.1");
        TcpListener serverListener = new TcpListener(IPAddress.Any, 8001);

        Thread serverThread;
        public List<ClientHandle> clients = new List<ClientHandle>();
        private LogsController logger = LogsController.GetInstance();

        private static Mutex mutex = new Mutex();

        public Server()
        {
            serverThread = new Thread(DoServerWork);
        }

        public void Start()
        {
            serverThread.Start();
        }
        
        public void DoServerWork()
        {
            int counter = 0;

            serverListener.Start();
            logger.AddMessage("Server started on " +IPAddress.Parse(((IPEndPoint)serverListener.LocalEndpoint).Address.ToString()) +
            ":" + ((IPEndPoint)serverListener.LocalEndpoint).Port.ToString());

            bool doWork = true;
            while (doWork)
            {
                counter += 1;
                try
                {
                    TcpClient clientSocket = serverListener.AcceptTcpClient();
                    logger.AddMessage("Client No: " + Convert.ToString(counter) + " connected");
                    ClientHandle client = new ClientHandle(clientSocket, counter);
                    clients.Add(client);
                } catch(SocketException e)
                {
                    Console.WriteLine("Socket closed; " + e.Message);
                    doWork = false;
                }
                
            }

        }

        public void Calculate(int mode,List<KeyframeControlElement> keyframes,int width,int height,int iterations, int framerate, ref CountdownEvent countdown)
        {
            
            
            List<Frame> frames = new List<Frame>();

            //framerate = (int)Math.Ceiling(framerate*1.0/(keyframes.Count - 1));
            framerate = framerate / (keyframes.Count - 1);


            for (int i=0; i < keyframes.Count-1; i++)
            {
                //Frame begin = new Frame(keyframes[i].bottomLeft, keyframes[i].topRight);
                
                double ratio = (keyframes[i + 1].topRight.X - keyframes[i + 1].bottomLeft.X)/ (keyframes[i].topRight.X - keyframes[i].bottomLeft.X);
                for (int j = 0; j < framerate; ++j)
                {
                    double t = j*1.0 / framerate;
                    double weight = ((Math.Pow(ratio, t) - 1) / (ratio - 1));
                    Point bottomLeft = Lerp(keyframes[i].bottomLeft, keyframes[i + 1].bottomLeft, weight);
                    Point topRight = Lerp(keyframes[i].topRight, keyframes[i + 1].topRight, weight);
                    Frame frame = new Frame(frames.Count, bottomLeft,topRight);
                    frames.Add(frame);
                }
                
            }
            frames.Add(new Frame(frames.Count,keyframes[keyframes.Count - 1].bottomLeft, keyframes[keyframes.Count - 1].topRight));

            for (int i = 0; i < clients.Count; i++)
            {
                countdown.AddCount();
                if (!clients[i].clientSocket.Connected)
                {
                    logger.AddMessage("Client "+i+" got disconnected");
                    clients.RemoveAt(i);
                    i--;
                    countdown.Signal();
                    continue;
                }
                if (mode == 0)
                {
                    logger.AddMessage("Frame per client");
                    clients[i].CalculateFrames(ref countdown, width, height, iterations, ref frames, ref mutex);
                }
                if (mode == 1)
                {
                    logger.AddMessage("Equal");
                    if ((frames.Count & 1) == 0)
                    {
                        clients[i].CalculateRange(ref countdown, width, height, iterations, frames.GetRange(frames.Count / clients.Count * i, frames.Count / clients.Count), (frames.Count / clients.Count * i));
                    }
                    else
                    {
                        if (i == 0)
                        {
                            clients[i].CalculateRange(ref countdown, width, height, iterations, frames.GetRange(frames.Count / clients.Count * i, frames.Count / clients.Count), (frames.Count / clients.Count * i));

                        }
                        else
                        {
                            clients[i].CalculateRange(ref countdown, width, height, iterations, frames.GetRange((frames.Count / clients.Count * i) + 1, frames.Count / clients.Count), (frames.Count / clients.Count * i));

                        }
                    }
                }
                
                



            }

            
            countdown.Signal();
            
        }
        private Point Lerp(Point start, Point end, double weight)
        {
            double x = (1 - weight) * start.X + weight * end.X;
            double y = (1 - weight) * start.Y + weight * end.Y;
            return new Point(x,y);
        }
        public void Stop()
        {
            foreach(ClientHandle client in clients)
            {
                client.clientSocket.Close();
            }
            serverListener.Stop();
        }
    }

    public class ClientHandle
    {
        public TcpClient clientSocket { get; set; }
        public int clientNumber { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int iterations { get; set; }

        public List<Frame> clientFrames { get; set; }

        public int startingFrameNumber { get; set; }

        public CountdownEvent countdown;

        public Mutex mutex;

        public ClientHandle(TcpClient socket, int clientNumber)
        {
            this.clientSocket = socket;
            this.clientNumber = clientNumber;
        }
        
        public void CalculateRange(ref CountdownEvent countdown, int width, int height, int iterations, List<Frame> clientFrames, int startingFrameNumber)
        {
            this.clientFrames = clientFrames;
            this.startingFrameNumber = startingFrameNumber;
            this.countdown = countdown;
            this.width = width;
            this.height = height;
            this.iterations = iterations;
            Thread clientHandlerThread = new Thread(TalkToClientRange);
            
            clientHandlerThread.Start();
        }

        public void CalculateFrames(ref CountdownEvent countdown, int width, int height, int iterations,ref List<Frame> frames, ref Mutex mutex)
        {
            this.countdown = countdown;
            this.width = width;
            this.height = height;
            this.iterations = iterations;
            this.mutex = mutex;
            //Thread clientHandlerThread = new Thread(TalkToClientFrames);

            //clientHandlerThread.Start();

            Thread clientHandlerThread = new Thread(new ParameterizedThreadStart(TalkToClientFrames));
            clientHandlerThread.Start(frames);
        }
        private void TalkToClientFrames(object framesList)
        {
            long calculationTime = 0;
            long totalCalculationTime = 0;
            bool doWork = true;
            LogsController logger = LogsController.GetInstance();
            while (doWork)
            {
                //logger.AddMessage(clientNumber + " count" + ((List<Frame>)framesList).Count);
                Frame frame = null;
                mutex.WaitOne();
                if (((List<Frame>)framesList).Count == 0)
                {
                    doWork = false;
                }
                else
                {
                    frame = ((List<Frame>)framesList)[((List<Frame>)framesList).Count - 1];
                    ((List<Frame>)framesList).RemoveAt(((List<Frame>)framesList).Count - 1);
                }
                mutex.ReleaseMutex();

                if (frame!=null)
                {
                    try
                    {
                        NetworkStream networkStream = clientSocket.GetStream();


                        byte[] bytesTo = new byte[52];

                        int numberOfFrames = 1;
                        int frameNumber = frame.number;
                        int byteCount = 0;
                        BitConverter.GetBytes(iterations).CopyTo(bytesTo, byteCount);
                        byteCount += 4;
                        BitConverter.GetBytes(width).CopyTo(bytesTo, byteCount);
                        byteCount += 4;
                        BitConverter.GetBytes(height).CopyTo(bytesTo, byteCount);
                        byteCount += 4;
                        BitConverter.GetBytes(numberOfFrames).CopyTo(bytesTo, byteCount);
                        byteCount += 4;
                        BitConverter.GetBytes(frameNumber).CopyTo(bytesTo, byteCount);
                        byteCount += 4;
                        BitConverter.GetBytes(frame.bottomLeft.X).CopyTo(bytesTo, byteCount);
                        byteCount += 8;
                        BitConverter.GetBytes(frame.bottomLeft.Y).CopyTo(bytesTo, byteCount);
                        byteCount += 8;
                        BitConverter.GetBytes(frame.topRight.X).CopyTo(bytesTo, byteCount);
                        byteCount += 8;
                        BitConverter.GetBytes(frame.topRight.Y).CopyTo(bytesTo, byteCount);
                        byteCount += 8;

                        Stopwatch stopwatch = Stopwatch.StartNew();
                        networkStream.Write(bytesTo, 0, bytesTo.Length);
                        networkStream.Flush();
                        //Console.WriteLine("Sent Keyframe data");


                        byte[] pixels = new byte[height * width * 3 + sizeof(int) + sizeof(long)];


                        int msgLength = networkStream.Read(pixels, 0, pixels.Length);
                        stopwatch.Stop();
                        totalCalculationTime += stopwatch.ElapsedMilliseconds;
                        calculationTime += BitConverter.ToInt64(pixels, 0);


                        //Console.WriteLine("Recieved..." + msgLength + " bytes");

                        Mandelbrot mandelbrot = new Mandelbrot(width, height);

                        mandelbrot.writeToBitmap(pixels, width * 3, sizeof(int) + sizeof(long));
                        mandelbrot.SaveToFile("testing" + frameNumber + ".png");
                        //Console.WriteLine("Image saved");
                        //Console.WriteLine("Press any key to exit");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(" >> " + ex.ToString());
                    }
                }

            }
            countdown.Signal();
            long communicationTime = totalCalculationTime - calculationTime;
            logger.AddMessage("C" + clientNumber + ": total calculation time = " + totalCalculationTime + "ms, calcutation time = " + calculationTime + "ms, communication time = " + communicationTime + "ms");

        }

        private void TalkToClientRange()
        {
            long calculationTime = 0;
            long totalCalculationTime = 0;
            for(int i = 0; i < clientFrames.Count; i++)
            {
                try
                {
                    NetworkStream networkStream = clientSocket.GetStream();


                    byte[] bytesTo = new byte[52];

                    int numberOfFrames = 1;
                    int frameNumber = startingFrameNumber + i;
                    int byteCount = 0;
                    BitConverter.GetBytes(iterations).CopyTo(bytesTo, byteCount);
                    byteCount += 4;
                    BitConverter.GetBytes(width).CopyTo(bytesTo, byteCount);
                    byteCount += 4;
                    BitConverter.GetBytes(height).CopyTo(bytesTo, byteCount);
                    byteCount += 4;
                    BitConverter.GetBytes(numberOfFrames).CopyTo(bytesTo, byteCount);
                    byteCount += 4;
                    BitConverter.GetBytes(frameNumber).CopyTo(bytesTo, byteCount);
                    byteCount += 4;
                    BitConverter.GetBytes(clientFrames[i].bottomLeft.X).CopyTo(bytesTo, byteCount);
                    byteCount += 8;
                    BitConverter.GetBytes(clientFrames[i].bottomLeft.Y).CopyTo(bytesTo, byteCount);
                    byteCount += 8;
                    BitConverter.GetBytes(clientFrames[i].topRight.X).CopyTo(bytesTo, byteCount);
                    byteCount += 8;
                    BitConverter.GetBytes(clientFrames[i].topRight.Y).CopyTo(bytesTo, byteCount);
                    byteCount += 8;

                    Stopwatch stopwatch = Stopwatch.StartNew();
                    networkStream.Write(bytesTo, 0, bytesTo.Length);
                    networkStream.Flush();
                    //Console.WriteLine("Sent Keyframe data");


                    byte[] pixels = new byte[height * width * 3 + sizeof(int) + sizeof(long)];


                    int msgLength = networkStream.Read(pixels, 0, pixels.Length);
                    stopwatch.Stop();
                    totalCalculationTime += stopwatch.ElapsedMilliseconds;
                    calculationTime += BitConverter.ToInt64(pixels, 0);


                    //Console.WriteLine("Recieved..." + msgLength + " bytes");

                    Mandelbrot mandelbrot = new Mandelbrot(width, height);

                    mandelbrot.writeToBitmap(pixels, width * 3, sizeof(int) + sizeof(long));
                    mandelbrot.SaveToFile("testing" + frameNumber + ".png");
                    //Console.WriteLine("Image saved");
                    //Console.WriteLine("Press any key to exit");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(" >> " + ex.ToString());
                }
            }
            countdown.Signal();
            long communicationTime = totalCalculationTime - calculationTime;
            LogsController logger = LogsController.GetInstance();
            logger.AddMessage("C" + clientNumber + ": total calculation time = " + totalCalculationTime + "ms, calcutation time = " + calculationTime + "ms, communication time = " + communicationTime + "ms");
            
        }
    }
}
