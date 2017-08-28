using System.Windows.Input;
using ClipboardZanager.ComponentModel.UI.Controls;

namespace ClipboardZanager.Views
{
    /// <summary>
    /// Interaction logic for FirstStartWindow.xaml
    /// </summary>
    public partial class FirstStartWindow : BlurredWindow
    {
        public FirstStartWindow()
        {
            InitializeComponent();
        }

        private void DragZoneGrid_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
