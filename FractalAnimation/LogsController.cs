using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;

namespace FractalAnimation
{
    class LogsController
    {
        private static LogsController instance;

        private static Object lockingObject = new Object();
        public static StackPanel logsPanel { get; set; }
        private LogsController() { }

        public static LogsController GetInstance()
        {
            if (instance == null)
            {
                lock (lockingObject)
                {
                    if (instance == null)
                    {
                        instance = new LogsController();
                    }
                }
            }
            return instance;
        }
        
        public void AddMessage(String message)
        {
            logsPanel.Dispatcher.Invoke(() => {
                TextBlock textBlock = new TextBlock();
                textBlock.TextWrapping = System.Windows.TextWrapping.Wrap;
                textBlock.Inlines.Add(new Bold(new Run(DateTime.Now.ToString("HH:mm:ss"))));
                textBlock.Inlines.Add(new Run(" > " + message));
                logsPanel.Children.Add(textBlock);
            });
        }

    }
}
