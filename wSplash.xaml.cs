using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Grudy
{
    /// <summary>
    /// Lógica interna para wSplash.xaml
    /// </summary>
    public partial class wSplash : Window
    {
        private readonly DispatcherTimer _timer;
        public wSplash()
        {
            InitializeComponent();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMicroseconds(800),   
                IsEnabled = true,
            };
            process.Value = 0;
            //_timer.Start();
            _timer.Tick += (a, b) =>
            {

                if (process.Value < process.Maximum)
                {
                    process.Value += 1; // avança 1 a cada tick (1s)
                }
                else
                {
                    process.Value = 10;
                }

            };
        }
        public async Task WaitSeg()
        {
            await Task.Delay(2000);
        }
        public void CloseMe()
        {
            this.Close();
        }
    }
}
