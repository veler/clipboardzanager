using System.Windows.Controls;
using ClipboardZanager.ComponentModel.Messages;
using GalaSoft.MvvmLight.Messaging;

namespace ClipboardZanager.Views.SettingsPanels
{
    /// <summary>
    /// Interaction logic for SettingsSynchronizationUserControl.xaml
    /// </summary>
    public partial class SettingsSynchronizationUserControl : UserControl
    {
        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="SettingsSynchronizationUserControl"/> class.
        /// </summary>
        internal SettingsSynchronizationUserControl()
        {
            InitializeComponent();

            Messenger.Default.Register<Message>(this, MessageIdentifiers.LinkCloudStorageServicePopupOpened, LinkCloudStorageServicePopupOpened);
            Messenger.Default.Register<Message>(this, MessageIdentifiers.UnlinkCloudStorageServicePopupOpened, UnlinkCloudStorageServicePopupOpened);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called when the popup for linking the app to a cloud storage service is opened
        /// </summary>
        /// <param name="message"></param>
        private void LinkCloudStorageServicePopupOpened(Message message)
        {
            CloudProviderItemsControl.Focus();
        }

        /// <summary>
        /// Called when the popup for unlinking the app to a cloud storage service is opened
        /// </summary>
        /// <param name="message"></param>
        private void UnlinkCloudStorageServicePopupOpened(Message message)
        {
            UnlinkCloudConfirmButton.Focus();
        }

        #endregion
    }
}
