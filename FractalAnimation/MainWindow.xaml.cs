using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FractalAnimation
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private int MaxIterations = 80;
        Point zoom1;
        Point zoom2;
        private Mandelbrot mandelbrot;
        private List<KeyframeControlElement> keyframeControlElements = new List<KeyframeControlElement>();
        private Server server = new Server();
        private LogsController logger;
        
        private void generate(object sender, RoutedEventArgs e)
        {
            //mandelbrot.iterations = int.Parse(inputIterations.Text);

            //Matrix matrix = new Matrix(1280 / (double)1280, 0, 0, 720 / (double)1280, 0, 0);
            //zoom1 = new Point(-0.800338731554, -0.207153842533);
            //zoom2 = new Point(zoom1.X + 0.10, zoom1.Y + 0.10);

            //zoom1 = matrix.Transform(zoom1);
            //zoom2 = matrix.Transform(zoom2);


            //mandelbrot.setPoints(zoom1, zoom2);
            //mandelbrot.setRecommendedIterations();
            //mandelbrot.calculate();

            if (keyframeControlElements.Count > 1)
            {

                //int animationLength = int.Parse(Regex.Match(inputAnimationLength.Text, @"\d+").Value);
                int animationLength = 1;
                int fps = 30;
                int iterations = 0;
                try
                {
                    animationLength = Convert.ToInt32(inputAnimationLength.Text);
                    fps = Convert.ToInt32(inputFPS.Text);
                    iterations = Convert.ToInt32(inputIterations.Text);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                btnGenerate.Content = "Calculating";
                btnGenerate.IsEnabled = false;
                CountdownEvent countdown = new CountdownEvent(1);
                
                //Console.WriteLine("Animation length: {0}, FPS: {1}",animationLength,fps);
                int width = 1280;
                int height = 720;
                switch (inputResolution.SelectedIndex)
                {
                    case 0:
                        width = 640;
                        height = 480;
                        break;
                    case 1:
                        width = 1280;
                        height = 720;
                        break;
                    case 2:
                        width = 1920;
                        height = 1080;
                        break;
                    case 3:
                        width = 3840;
                        height = 2160;
                        break;
                    default:
                        break;
                }

                //30 * 1 -> fps * seconds
                Stopwatch stopwatch = Stopwatch.StartNew();
                server.Calculate(keyframeControlElements,width,height,iterations,fps*animationLength,ref countdown);
                countdown.Wait();
                stopwatch.Stop();
                Console.WriteLine("Stopwatch: " + stopwatch.ElapsedMilliseconds + "ms");
                
                string strCmdText;
                strCmdText = "/C ffmpeg -y -framerate " + fps+" -i testing%d.png output.mp4";
                Process ffmpeg = Process.Start("CMD.exe", strCmdText);
                ffmpeg.WaitForExit();
                logger.AddMessage("Finished");
                btnGenerate.IsEnabled = true;
                btnGenerate.Content = "Generate";

            }
        }
        private void addKeyFrame(object sender, RoutedEventArgs e)
        {
            KeyframeControlElement kce = new KeyframeControlElement(mandelbrot.bottomLeft, mandelbrot.topRight, ref keyframes, keyframeControlElements.Count);
            kce.addToParent();
            kce.arrowUp.Click += moveKeyFrameUp;
            kce.arrowDown.Click += moveKeyFrameDown;
            kce.btnRemove.Click += removeKeyframe;
            keyframeControlElements.Add(kce);


        }

        private void moveKeyFrameUp(object sender, RoutedEventArgs e)
        {
            
            int index = (int)((Button)sender).Tag;
            if (index >= 1)
            {
                KeyframeControlElement tmp = keyframeControlElements[index-1];
                
                keyframeControlElements[index-1] = keyframeControlElements[index];
                keyframeControlElements[index] = tmp;

                keyframeControlElements[index - 1].moveUp();
                keyframeControlElements[index].moveDown();


            }
        }

        private void moveKeyFrameDown(object sender, RoutedEventArgs e)
        {
            int index = (int)((Button)sender).Tag;
            if (index < keyframeControlElements.Count-1)
            {
                KeyframeControlElement tmp = keyframeControlElements[index + 1];
                keyframeControlElements[index + 1] = keyframeControlElements[index];
                keyframeControlElements[index] = tmp;

                keyframeControlElements[index + 1].moveDown();
                keyframeControlElements[index].moveUp();

            }
        }

        private void removeKeyframe(object sender, RoutedEventArgs e)
        {
            int index = (int)((Button)sender).Tag;

            keyframeControlElements.Remove(keyframeControlElements[index]);
            for(int i = index; i < keyframeControlElements.Count; i++)
            {
                keyframeControlElements[i].moveUp();
            }
        }
        private void zoomOut(object sender, RoutedEventArgs e)
        {
            
            Matrix matrix = new Matrix(1280 / (double)1280, 0, 0, 720 / (double)1280, 0, 0);
            zoom1 = new Point(-2, -2);
            zoom2 = new Point(zoom1.X + 4, zoom1.Y + 4);

            zoom1 = matrix.Transform(zoom1);
            zoom2 = matrix.Transform(zoom2);


            mandelbrot.setPoints(zoom1, zoom2);
            mandelbrot.setRecommendedIterations();
            mandelbrot.calculate();
        }
        private void zoomIn(object sender, RoutedEventArgs e)
        {
            Matrix matrix = new Matrix(1280 / (double)1280, 0, 0, 720 / (double)1280, 0, 0);
            zoom1 = new Point(-0.800338731554, -0.207153842533);
            zoom2 = new Point(zoom1.X + 0.10, zoom1.Y + 0.10);

            zoom1 = matrix.Transform(zoom1);
            zoom2 = matrix.Transform(zoom2);


            mandelbrot.setPoints(zoom1, zoom2);
            mandelbrot.setRecommendedIterations();
            mandelbrot.calculate();
        }
        public void fuck(String message)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.Inlines.Add(new Bold(new Run(DateTime.Now.ToString("HH:mm:ss"))));
            textBlock.Inlines.Add(new Run(" > " + message));
            logs.Children.Add(textBlock);

        }

        public MainWindow()
        {
            InitializeComponent();

            logger = LogsController.GetInstance();
            LogsController.logsPanel = logs;

            logger.AddMessage("Hello world");

            Closing += MainWindow_Closing;
            btnGenerate.Click += generate;
            btnAddKeyframe.Click += addKeyFrame;
            btnZoomOut.Click += zoomOut;
            previewImage.MouseLeftButtonDown += zoomIn;

            mandelbrot = new Mandelbrot(1280, 720);

            previewImage.Source = mandelbrot.GetBitmap();
            previewImage.Stretch = Stretch.Uniform;

            mandelbrot.calculate();




            Matrix matrix = new Matrix(1280 / (double)1280, 0, 0, 720 / (double)1280, 0, 0);
            zoom1 = new Point(-0.800338731554, -0.207153842533);
            zoom2 = new Point(zoom1.X + 0.10, zoom1.Y + 0.10);

            zoom1 = matrix.Transform(zoom1);
            zoom2 = matrix.Transform(zoom2);

            //Console.WriteLine("\n\nZOOM2: "+zoom2+"\n\n");

            zoom1.X = (mandelbrot.bottomLeft.X - zoom1.X) / 30;
            zoom1.Y = (mandelbrot.bottomLeft.Y - zoom1.Y) / 30;
            zoom2.X = (mandelbrot.topRight.X - zoom2.X) / 30;
            zoom2.Y = (mandelbrot.topRight.Y - zoom2.Y) / 30;


            server.Start();

        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            server.Stop();
        }
    }
}
