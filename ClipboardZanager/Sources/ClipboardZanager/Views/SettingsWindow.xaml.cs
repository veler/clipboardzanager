using System.Windows.Input;
using ClipboardZanager.ComponentModel.Enums;
using ClipboardZanager.ComponentModel.UI.Controls;
using ClipboardZanager.ViewModels;

namespace ClipboardZanager.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : BlurredWindow
    {
        public SettingsWindow(SettingsViewMode viewMode)
        {
            InitializeComponent();

            var dataContext = (SettingsWindowViewModel)DataContext;
            if (viewMode == SettingsViewMode.None)
            {
                dataContext.ViewMode = SettingsViewMode.General;
            }
            else
            {
                dataContext.ViewMode = viewMode;
            }
        }

        private void DragZoneGrid_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
