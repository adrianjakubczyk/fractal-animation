using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FractalAnimation
{
    class Server
    {
        static IPAddress ipAd = IPAddress.Parse("127.0.0.1");
        TcpListener serverListener = new TcpListener(ipAd, 8001);
        Thread serverThread;
        public List<ClientHandle> clients = new List<ClientHandle>();

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
            Console.WriteLine(" >> " + "Server Started");

            bool doWork = true;
            while (doWork)
            {
                counter += 1;
                try
                {
                    TcpClient clientSocket = serverListener.AcceptTcpClient();
                    Console.WriteLine(" >> " + "Client No:" + Convert.ToString(counter) + " started!");
                    ClientHandle client = new ClientHandle(clientSocket, counter);
                    clients.Add(client);
                } catch(SocketException e)
                {
                    Console.WriteLine("Socket closed; "+e.Message);
                    doWork = false;
                }
                
            }

        }

        public void Calculate(List<KeyframeControlElement> keyframes,int width,int height,int iterations, int framerate, ref CountdownEvent countdown)
        {
            
            //write some magic here

            //temp all clients calc the same

            Console.WriteLine("\n\nClient count: " + clients.Count);

            List<Frame> frames = new List<Frame>();

            //framerate = (int)Math.Ceiling(framerate*1.0/(keyframes.Count - 1));
            framerate = framerate / (keyframes.Count - 1);


            for (int i=0; i < keyframes.Count-1; i++)
            {
                Frame begin = new Frame(keyframes[i].bottomLeft, keyframes[i].topRight);


                Frame increment = new Frame(
                    (keyframes[i].bottomLeft.X - keyframes[i + 1].bottomLeft.X) / framerate,
                    (keyframes[i].bottomLeft.Y - keyframes[i + 1].bottomLeft.Y) / framerate,
                    (keyframes[i].topRight.X - keyframes[i + 1].topRight.X) / framerate,
                    (keyframes[i].topRight.Y - keyframes[i + 1].topRight.Y) / framerate
                    );
                //frames.Add(begin);
                for (int j = 0; j < framerate; ++j)
                {
                    Frame frame = new Frame();
                    frame.Add(begin);
                    for (int z = 0; z < j; z++)
                    {
                        frame.Subtract(increment);

                    }

                    frames.Add(frame);
                }
                //frames.Add(new Frame(keyframes[i+1].bottomLeft, keyframes[i+1].topRight));
            }

            Console.WriteLine("\n\nFRAMES: " +frames.Count);
            for (int i = 0; i < clients.Count; i++)
            {
                countdown.AddCount();
                if (!clients[i].clientSocket.Connected)
                {
                    Console.WriteLine("CLIENT GOT DCd, removing");
                    clients.RemoveAt(i);
                    i--;
                    countdown.Signal();
                    continue;
                }
                if((frames.Count & 1) == 0)
                {
                    clients[i].Calculate(ref countdown, width, height,iterations, frames.GetRange(frames.Count / clients.Count * i, frames.Count / clients.Count), (frames.Count / clients.Count * i));
                }
                else
                {
                    if (i == 0)
                    {
                        clients[i].Calculate(ref countdown, width, height, iterations, frames.GetRange(frames.Count / clients.Count * i, frames.Count / clients.Count), (frames.Count / clients.Count * i));

                    }
                    else
                    {
                        clients[i].Calculate(ref countdown, width, height, iterations, frames.GetRange((frames.Count / clients.Count * i)+1, frames.Count / clients.Count), (frames.Count / clients.Count * i));

                    }
                }

                //clients[i].Calculate(keyframes.GetRange(keyframes.Count / clients.Count * i, keyframes.Count / clients.Count), (keyframes.Count / clients.Count * i));



            }
            countdown.Signal();
            
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

        public ClientHandle(TcpClient socket, int clientNumber)
        {
            this.clientSocket = socket;
            this.clientNumber = clientNumber;
        }
        //public void Calculate(List<KeyframeControlElement> frames, int startingFrameNumber)
        //{
        //    this.frames = frames;
        //    this.startingFrameNumber = startingFrameNumber;
        //    Thread clientHandlerThread = new Thread(TalkToClient);
        //    clientHandlerThread.Start();
        //}
        
        public void Calculate(ref CountdownEvent countdown, int width, int height, int iterations, List<Frame> clientFrames, int startingFrameNumber)
        {
            this.clientFrames = clientFrames;
            this.startingFrameNumber = startingFrameNumber;
            this.countdown = countdown;
            this.width = width;
            this.height = height;
            this.iterations = iterations;
            Thread clientHandlerThread = new Thread(TalkToClient);
            clientHandlerThread.Start();
        }
        private void TalkToClient()
        {
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


                    networkStream.Write(bytesTo, 0, bytesTo.Length);
                    networkStream.Flush();
                    Console.WriteLine("Sent Keyframe data");


                    byte[] pixels = new byte[height * width * 3 + sizeof(int) + sizeof(long)];


                    int msgLength = networkStream.Read(pixels, 0, pixels.Length);


                    Console.WriteLine("Recieved..." + msgLength + " bytes");

                    Mandelbrot mandelbrot = new Mandelbrot(width, height);

                    mandelbrot.writeToBitmap(pixels, width * 3, sizeof(int) + sizeof(long));
                    mandelbrot.SaveToFile("testing" + frameNumber + ".png");
                    Console.WriteLine("Image saved");
                    Console.WriteLine("Press any key to exit");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(" >> " + ex.ToString());
                }
            }
            countdown.Signal();
        }
    }
}
