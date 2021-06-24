using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FractalAnimation
{
    public class Frame
    {
        public Point bottomLeft { get; set; }
        public Point topRight { get; set; }

        public int number { get; set; }

        public Frame()
        {
            this.number = 0;
            this.bottomLeft = new Point();
            this.topRight = new Point();
        }
        public Frame(int number,Point bottomLeft, Point topRight)
        {
            this.number = number;
            this.bottomLeft = bottomLeft;
            this.topRight = topRight;

        }
        public Frame(double x1,double y1, double x2, double y2)
        {
            this.bottomLeft = new Point(x1, y1);
            this.topRight = new Point(x2, y2);
        }

        public void Add(Frame frame)
        {
            this.bottomLeft = new Point(this.bottomLeft.X + frame.bottomLeft.X, this.bottomLeft.Y + frame.bottomLeft.Y);
            this.topRight = new Point(this.topRight.X + frame.topRight.X, this.topRight.Y + frame.topRight.Y);
           
        }
        public void Subtract(Frame frame)
        {
            this.bottomLeft = new Point(this.bottomLeft.X - frame.bottomLeft.X, this.bottomLeft.Y - frame.bottomLeft.Y);
            this.topRight = new Point(this.topRight.X - frame.topRight.X, this.topRight.Y - frame.topRight.Y);
        }

        
    }
}
