using CefSharp;
using CefSharp.Wpf;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Path = System.IO.Path;

namespace Grudy
{
    /// <summary>
    /// Interação lógica para MiniBrowser.xam
    /// </summary>
    public partial class MiniBrowser : UserControl
    {
        public event EventHandler Exit;
        public ChromiumWebBrowser webb;
        static Boolean? ChromiumWebBrowserInitilize = null;
        public MiniBrowser()
        {
            InitializeComponent();
            string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (exeDir != null)
            {
                Directory.SetCurrentDirectory(exeDir);
            }

            if (ChromiumWebBrowserInitilize == null)
            {
                ChromiumWebBrowserInitilize = true;
                if (!Cef.Initialize(new CefSettings()
                {
                    //BrowserSubprocessPath = $"{exeDir}\\CefSharp.BrowserSubprocess.exe",
                }, performDependencyCheck: false, browserProcessHandler: null))
                {
                    throw new Exception("Unable to Initialize Cef");
                }
            }

            webb = new ChromiumWebBrowser("https://www.google.com");

            gWeb.Children.Add(webb);

            this.MouseMove += MainWindow_MouseMove;
            this.MouseDown += MainWindow_MouseDown;
            txt_url.MouseDown += MainWindow_MouseDown;

            gTools.Visibility = Visibility.Hidden;
            gRow1.Height = new GridLength(0);
            gRow3.Height = new GridLength(0);


            lbStatus.Content = "Iniciando....";

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                txt_url.Text = addUrl(args[1]);
            }
            else
            {
                txt_url.Text = "https://google.com.br";
            }

            webb.Load(txt_url.Text);

            this.KeyDown += (a, b) =>
            {
                //if (b.Key == System.Windows.Input.Key.F5) webb.Refresh();
            };

            webb.LoadingStateChanged += (a, b) =>
            {
                //lbStatus.Content = b.Browser.GetH
            };


            webb.AddressChanged += (a, e) => {
                txt_url.Text = e.NewValue.ToString();
            };

            webb.LifeSpanHandler = new LifeSpanHandler();
        }

        public void Navegate(string Url, string currentDir)
        {

        }
        private void MainWindow_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //if (e.GetPosition(this).Y <= 34 || e.GetPosition(this).Y >= (Height - 20))
            //{
            //    this.DragMove();
            //}
        }

        private void MainWindow_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //gTools.Visibility = Visibility.Hidden;
            //gRow1.Height = new GridLength(0);
            //gRow3.Height = new GridLength(0);
            //if (e.GetPosition(this).Y <= 34)
            //{
            //    gRow1.Height = new GridLength(32);
            //    gTools.Visibility = Visibility.Visible;
            //    gRow3.Height = new GridLength(20);
            //}
        }

        string addUrl(string g)
        {
            if (g.Length > "https://".Length && (g.Substring(0, "https://".Length) != "https://"))
            {
                if (g.Length > "http://".Length && (g.Substring(0, "http://".Length) != "http://"))
                    return $"https://{g}";
            }
            return g;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            webb.Back();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            webb.Forward();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Exit != null)
                Exit(null, e);
        }

        private void txt_url_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                e.Handled = false;
                webb.Load(addUrl(txt_url.Text));
            }
        }


        private void Label_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //this.DragMove();
        }
    }

    public class LifeSpanHandler : ILifeSpanHandler
    {
        public bool DoClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
            MessageBox.Show("Ivocando DoClose");
            return false;
        }

        public void OnAfterCreated(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {

        }

        public void OnBeforeClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
            MessageBox.Show("Ivocando OnBeforeClose");
        }

        public bool OnBeforePopup(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl, string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
        {
            noJavascriptAccess = false;
            newBrowser = null;
            MessageBox.Show("Ivocando nova Janela");
            return false;
        }
    }
}
