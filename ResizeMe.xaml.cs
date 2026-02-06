using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;


namespace Grudy
{
    /// <summary>
    /// Interação lógica para ResizeMe.xam
    /// </summary>
    public partial class ResizeMe : UserControl
    {
        Window _window { set; get; } = null;
        public ResizeMe()
        {
            InitializeComponent();
            _window = Window.GetWindow(this);
        }


        public Window SetWindow
        {
            set
            {
                _window = value;
            }
        }
        //class TMPress
        //{
        //    public VerticalAlignment VerticalAlignment { get; set; }
        //    public HorizontalAlignment HorizontalAlignment { get; set; }
        //    public Point Point { get; set; }
        //}
        //TMPress? mPress { get; set; } = null;

        private void GridDragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (_window == null) return;
            var va = (sender as Thumb).VerticalAlignment;
            var ha = (sender as Thumb).HorizontalAlignment;

            if (va == VerticalAlignment.Bottom && (_window.Height > _window.MinHeight || e.VerticalChange < 0))
            {
                _window.Height += e.VerticalChange;
            }

            if (va == VerticalAlignment.Top && (_window.Height > _window.MinHeight || e.VerticalChange > 0))
            {
                _window.Height -= e.VerticalChange;
                _window.Top += e.VerticalChange;
            }

            if (ha == HorizontalAlignment.Left && (_window.Width > _window.MinWidth || e.HorizontalChange > 0))
            {
                _window.Width -= e.HorizontalChange;
                _window.Left += e.HorizontalChange;
            }

            if (ha == HorizontalAlignment.Right && (_window.Width > _window.MinWidth || e.HorizontalChange < 0))
            {
                _window.Width += e.HorizontalChange;
            }
        }

        private void GridMouseDown(object sender, MouseButtonEventArgs e)
        {

        }
        private void GridMouseMove(object sender, MouseEventArgs e)
        {

        }
        private void GridMouseUp(object sender, MouseButtonEventArgs e)
        {
            //if (_window == null) return;
            //mPress = null;
        }

    }
}
