using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Grudy
{
    /// <summary>
    /// Interação lógica para TabItemHeader.xam
    /// </summary>
    public partial class TabItemHeader : UserControl
    {
        public event EventHandler<TabItem?> CloseMe;
        public TabItem? TabItemMe { get; set; } = null;
        public TabItemHeader()
        {
            InitializeComponent();
        }
        public string Text { get => boxText.Text; set=> boxText.Text = value; }

        private void CloseTab_Click(object sender, RoutedEventArgs e)
        {
            if(CloseMe!= null)
                CloseMe(this, TabItemMe);
        }
    }
}
