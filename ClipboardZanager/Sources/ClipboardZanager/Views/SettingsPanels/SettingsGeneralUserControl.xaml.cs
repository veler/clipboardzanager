using System.Windows.Controls;
using ClipboardZanager.ComponentModel.Messages;
using GalaSoft.MvvmLight.Messaging;
using ClipboardZanager.ViewModels.SettingsPanels;

namespace ClipboardZanager.Views.SettingsPanels
{
    /// <summary>
    /// Interaction logic for SettingsGeneralUserControl.xaml
    /// </summary>
    public partial class SettingsGeneralUserControl : UserControl
    {
        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="SettingsGeneralUserControl"/> class.
        /// </summary>
        public SettingsGeneralUserControl()
        {
            InitializeComponent();

            Messenger.Default.Register<Message>(this, MessageIdentifiers.ChangeHotKeyPopupOpened, ChangeHotKeyPopupOpened);
            Messenger.Default.Register<Message>(this, MessageIdentifiers.RestoreDefaultSettingsPopupOpened, RestoreDefaultSettingsPopupOpened);
        }

        #endregion

        #region Handled Methods

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ((SettingsGeneralUserControlViewModel)DataContext).LoadStartWithWindows();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called when the popup for changing keyboard shortcut is opened.
        /// </summary>
        /// <param name="message"></param>
        private void ChangeHotKeyPopupOpened(Message message)
        {
            AcceptHotKeysButton.Focus();
        }

        /// <summary>
        /// Called when the popup for restoring the default settings is opened
        /// </summary>
        /// <param name="message"></param>
        private void RestoreDefaultSettingsPopupOpened(Message message)
        {
            RestoreDefaultConfirmButton.Focus();
        }

        #endregion
    }
}
