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
    public partial class TabItemHeaderAction : UserControl
    {
        public event EventHandler<TabItem?> ClickMe;
        public TabItem? TabItemMe { get; set; } = null;
        public TabItemHeaderAction()
        {
            InitializeComponent();
        }
        private void Tab_Click(object sender, RoutedEventArgs e)
        {
            if(ClickMe != null)
                ClickMe(this, TabItemMe);
        }
        public EventHandler<TabItem?> AddEventClick
        {
            set {
                this.ClickMe += value;
            }
        }
    }
}
