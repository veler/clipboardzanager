using System.Linq;
using System.Windows;
using System.Windows.Input;
using ClipboardZanager.ComponentModel.Enums;
using ClipboardZanager.ComponentModel.Messages;
using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Shared.Core;
using ClipboardZanager.ViewModels;
using GalaSoft.MvvmLight.Messaging;
using MenuItem = System.Windows.Controls.MenuItem;

namespace ClipboardZanager.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            Messenger.Default.Register<Message>(this, MessageIdentifiers.ShowNotifyIconBalloon, ShowNotifyIconBalloon);

            foreach (var menuItem in NotifyIcon.ContextMenu.Items.OfType<MenuItem>())
            {
                menuItem.DataContext = DataContext;
            }

            Hide();
            CoreHelper.MinimizeFootprint();
        }

        #endregion

        #region Handled Methods

        /// <summary>
        /// A <see cref="EventToCommand"/> would not work because the window is initialized but never loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NotifyIcon_ContextMenuClosed(object sender, System.EventArgs e)
        {
            ((MainWindowViewModel)DataContext).ContextMenuClosedCommand.Execute(null);
        }

        /// <summary>
        /// A <see cref="EventToCommand"/> would not work because the window is initialized but never loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NotifyIcon_OnMouseClick(object sender, MouseButtonEventArgs e)
        {
            var viewModel = (MainWindowViewModel)DataContext;
            if (e.ChangedButton == MouseButton.Left && viewModel.PasteCommand.CanExecute(null))
            {
                viewModel.PasteCommand.Execute(null);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Display a balloon on the notify icon with the specified message.
        /// </summary>
        /// <param name="message">The message to show.</param>
        private void ShowNotifyIconBalloon(Message message)
        {
            Requires.IsTrue(message.Values.Length == 2);

            NotifyIcon.Visibility = Visibility.Collapsed;
            NotifyIcon.Visibility = Visibility.Visible;

            NotifyIcon.ShowBalloonTip(10000, message.Values[0].ToString(), message.Values[1].ToString(), BalloonTipIcon.Info);
        }

        #endregion
    }
}
