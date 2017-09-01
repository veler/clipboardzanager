using System.Windows.Input;
using ClipboardZanager.ComponentModel.UI.Controls;
using ClipboardZanager.ViewModels;

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

            if (((FirstStartWindowViewModel)DataContext).IsMigrationRequired)
            {
                FlipView.Items.Remove(LanguageTab);
                FlipView.Items.Remove(IgnoredAppTab);
                FlipView.Items.Remove(SynchronizationTab);
                FlipView.Items.Remove(TutorialTab);
                FlipView.SelectedIndex = 0;
            }
        }

        private void DragZoneGrid_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
