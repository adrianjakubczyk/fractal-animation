using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FractalAnimation
{
    public class KeyframeControlElement
    {
        public Point bottomLeft { get; set; }
        public Point topRight { get; set; }

        private Grid parent;
        public int rowNumber { get; set; }

        private StackPanel stackPanel;
        public Button arrowUp { get; set; }
        public Button arrowDown { get; set; }
        private Label x1;
        private Label y1;
        private Label x2;
        private Label y2;
        public Button btnRemove { get; set; }



        public KeyframeControlElement(Point bottomLeft, Point topRight,ref Grid parent, int rowNumber)
        {
            this.bottomLeft = bottomLeft;
            this.topRight = topRight;

            this.parent = parent;
            this.rowNumber = rowNumber;
            
        }

        public void addToParent()
        {
            parent.RowDefinitions.Add(new RowDefinition());

            stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Vertical;

            Brush backgroundBrush = (Brush)(new BrushConverter().ConvertFrom("#333"));

            arrowUp = new Button();
            arrowDown = new Button();

            arrowUp.Tag = rowNumber;
            arrowDown.Tag = rowNumber;

            arrowUp.Background = backgroundBrush;
            arrowDown.Background = backgroundBrush;

            arrowUp.Foreground = Brushes.White;
            arrowDown.Foreground = Brushes.White;

            arrowUp.Content = "▲";
            arrowDown.Content = "▼";

            stackPanel.Children.Add(arrowUp);
            stackPanel.Children.Add(arrowDown);

            x1 = new Label();
            y1 = new Label();
            x2 = new Label();
            y2 = new Label();

            x1.Foreground = Brushes.White;
            y1.Foreground = Brushes.White;
            x2.Foreground = Brushes.White;
            y2.Foreground = Brushes.White;

            x1.VerticalAlignment = VerticalAlignment.Center;
            y1.VerticalAlignment = VerticalAlignment.Center;
            x2.VerticalAlignment = VerticalAlignment.Center;
            y2.VerticalAlignment = VerticalAlignment.Center;



            x1.Content = bottomLeft.X;
            y1.Content = bottomLeft.Y;
            x2.Content = topRight.X;
            y2.Content = topRight.Y;

            //x1.Content = rowNumber;
            //y1.Content = rowNumber;
            //x2.Content = rowNumber;
            //y2.Content = rowNumber;

            btnRemove = new Button();
            btnRemove.Tag = rowNumber;
            btnRemove.Background = backgroundBrush;
            btnRemove.Foreground = Brushes.DeepPink;
            btnRemove.Content = "-";
            btnRemove.Click += removeFromParent;

            Grid.SetRow(stackPanel, rowNumber);
            Grid.SetRow(x1, rowNumber);
            Grid.SetRow(y1, rowNumber);
            Grid.SetRow(x2, rowNumber);
            Grid.SetRow(y2, rowNumber);
            Grid.SetRow(btnRemove, rowNumber);

            Grid.SetColumn(stackPanel, 0);
            Grid.SetColumn(x1, 1);
            Grid.SetColumn(y1, 2);
            Grid.SetColumn(x2, 3);
            Grid.SetColumn(y2, 4);
            Grid.SetColumn(btnRemove, 5);


            parent.Children.Add(stackPanel);
            parent.Children.Add(x1);
            parent.Children.Add(y1);
            parent.Children.Add(x2);
            parent.Children.Add(y2);
            parent.Children.Add(btnRemove);
        }

        private void removeFromParent(object sender, RoutedEventArgs e)
        {
            stackPanel.Children.Clear();
            parent.Children.Remove(stackPanel);
            parent.Children.Remove(x1);
            parent.Children.Remove(y1);
            parent.Children.Remove(x2);
            parent.Children.Remove(y2);
            parent.Children.Remove(btnRemove);
        }

        public void moveUp()
        {
            rowNumber -= 1;
            Grid.SetRow(stackPanel, rowNumber);
            Grid.SetRow(x1, rowNumber);
            Grid.SetRow(y1, rowNumber);
            Grid.SetRow(x2, rowNumber);
            Grid.SetRow(y2, rowNumber);
            Grid.SetRow(btnRemove, rowNumber);

            arrowUp.Tag = rowNumber;
            arrowDown.Tag = rowNumber;
            btnRemove.Tag = rowNumber;

        }

        public void moveDown()
        {
            rowNumber += 1;
            Grid.SetRow(stackPanel, rowNumber);
            Grid.SetRow(x1, rowNumber);
            Grid.SetRow(y1, rowNumber);
            Grid.SetRow(x2, rowNumber);
            Grid.SetRow(y2, rowNumber);
            Grid.SetRow(btnRemove, rowNumber);

            arrowUp.Tag = rowNumber;
            arrowDown.Tag = rowNumber;
            btnRemove.Tag = rowNumber;
        }


    }
}
