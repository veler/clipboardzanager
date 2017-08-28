using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using ClipboardZanager.Shared.CloudStorage;
using ClipboardZanager.Shared.Core;
using ClipboardZanager.Shared.Logs;
using ClipboardZanager.Strings;

namespace ClipboardZanager.ComponentModel.UI.Controls
{
    /// <summary>
    /// Interaction logic for CloudStorageAuthenticationUserControl.xaml
    /// </summary>
    public partial class CloudStorageAuthenticationUserControl : UserControl, ICloudAuthentication
    {
        #region Fields

        private readonly ManualResetEvent _completionSignalEvent;
        private string _redirectUri;
        private bool _isCanceled;
        private Uri _redirectedUri;

        #endregion

        #region Events

        /// <summary>
        /// Raised when the authentication has been canceled by the user.
        /// </summary>
        public static readonly RoutedEvent AuthenticationCanceledEvent = EventManager.RegisterRoutedEvent("AuthenticationCanceled", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CloudStorageAuthenticationUserControl));

        internal event RoutedEventHandler AuthenticationCanceled
        {
            add { AddHandler(AuthenticationCanceledEvent, value); }
            remove { RemoveHandler(AuthenticationCanceledEvent, value); }
        }

        /// <summary>
        /// Raised when the authentication is completed.
        /// </summary>
        public static readonly RoutedEvent AuthenticationCompletedEvent = EventManager.RegisterRoutedEvent("AuthenticationCompleted", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CloudStorageAuthenticationUserControl));

        internal event RoutedEventHandler AuthenticationCompleted
        {
            add { AddHandler(AuthenticationCompletedEvent, value); }
            remove { RemoveHandler(AuthenticationCompletedEvent, value); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize the instance of the <see cref="CloudStorageAuthenticationUserControl"/> class
        /// </summary>
        public CloudStorageAuthenticationUserControl()
        {
            InitializeComponent();
            _completionSignalEvent = new ManualResetEvent(false);
        }

        #endregion

        #region Handled Methods

        private void WebBrowser_OnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            if (!e.Uri.ToString().StartsWith(_redirectUri, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            e.Cancel = true;

            Logger.Instance.Information($"The authentication with the UI succeeded.");
            _redirectedUri = e.Uri;
            _completionSignalEvent.Set();
            RaiseEvent(new RoutedEventArgs(AuthenticationCompletedEvent));
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            Logger.Instance.Information($"The authentication with the UI has been canceled by the user.");
            _isCanceled = true;
            _completionSignalEvent.Set();
            RaiseEvent(new RoutedEventArgs(AuthenticationCanceledEvent));
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public async Task<AuthenticationResult> AuthenticateAsync(string authenticationUri, string redirectUri)
        {
            Logger.Instance.Information($"Authentication to a cloud storage provider with the UI.");
            Requires.NotNullOrWhiteSpace(authenticationUri, nameof(authenticationUri));

            if (redirectUri.Contains(authenticationUri))
            {
                throw new ArgumentException($"The {nameof(redirectUri)} must not contains the {nameof(authenticationUri)}.");
            }

            _redirectUri = redirectUri;

            _completionSignalEvent.Reset();

            await Dispatcher.InvokeAsync(() =>
             {
                 CancelButton.Content = LanguageManager.GetInstance().MainWindow.Cancel;
                 WebBrowser.Navigate(authenticationUri);

                 _isCanceled = false;
                 _redirectedUri = null;
             });

            _completionSignalEvent.WaitOne();
            return new AuthenticationResult(_isCanceled, _redirectedUri);
        }

        #endregion
    }
}
