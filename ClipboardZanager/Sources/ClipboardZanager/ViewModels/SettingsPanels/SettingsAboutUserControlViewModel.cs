using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Core.Desktop.IO;
using ClipboardZanager.Shared.Logs;
using ClipboardZanager.Strings;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;

namespace ClipboardZanager.ViewModels.SettingsPanels
{
    /// <summary>
    /// Interaction logic for <see cref="SettingsAboutUserControl"/>
    /// </summary>
    internal sealed class SettingsAboutUserControlViewModel : ViewModelBase
    {
        #region Fields
        
        private bool _isCreditPopupOpened;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the <see cref="LanguageManager"/>
        /// </summary>
        public LanguageManager Language => LanguageManager.GetInstance();

        /// <summary>
        /// Gets the assembly name.
        /// </summary>
        public string Name => CoreHelper.GetApplicationName().ToString();

        /// <summary>
        /// Gets the assembly version.
        /// </summary>
        public string Version => CoreHelper.GetApplicationVersion().ToString();

        /// <summary>
        /// Gets the assembly copyright.
        /// </summary>
        public string Copyright => ((AssemblyCopyrightAttribute)Assembly.GetEntryAssembly().GetCustomAttribute(typeof(AssemblyCopyrightAttribute))).Copyright;

        /// <summary>
        /// Gets or sets a value that defines whether the popup for the credits is opened
        /// </summary>
        public bool IsCreditPopupOpened
        {
            get { return _isCreditPopupOpened; }
            set
            {
                _isCreditPopupOpened = value;
                RaisePropertyChanged();
            }
        }

        #endregion


        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="SettingsAboutUserControlViewModel"/> class.
        /// </summary>
        internal SettingsAboutUserControlViewModel()
        {
            InitializeCommands();
        }

        #endregion

        #region Commands

        /// <summary>
        /// Initialize the commands of the View Model
        /// </summary>
        private void InitializeCommands()
        {
            SendLogs = new RelayCommand(ExecuteSendLogsCommand, CanExecuteSendLogsCommand);
            OpenWebsite = new RelayCommand<string>(ExecuteOpenWebsiteCommand);
            CreditButtonCommand = new RelayCommand(ExecuteCreditButtonCommand);
        }

        #region SendLogs

        /// <summary>
        /// Gets or sets a <see cref="RelayCommand"/> executed when we click on the Send the logs to the author
        /// </summary>
        public RelayCommand SendLogs { get; private set; }

        private bool CanExecuteSendLogsCommand()
        {
            return SystemInfoHelper.CheckForInternetConnection();
        }

        private async void ExecuteSendLogsCommand()
        {
            var logSession = Logger.InstanceLogSession as LogSession;

            if (logSession == null || !File.Exists(logSession.GetPreviousSessionLogFilePath()))
            {
                return;
            }

            var url = "http://files.velersoftware.com/clipboardzanager/logs/send.php";
            var outgoingQueryString = HttpUtility.ParseQueryString(String.Empty);
            outgoingQueryString.Add("logs", File.ReadAllText(logSession.GetPreviousSessionLogFilePath()));
            var postdata = Encoding.ASCII.GetBytes(outgoingQueryString.ToString());

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 7.1; Trident/5.0)";
            request.Accept = "/";
            request.UseDefaultCredentials = true;
            request.Proxy.Credentials = CredentialCache.DefaultCredentials;

            var stream = request.GetRequestStream();
            stream.Write(postdata, 0, postdata.Length);
            stream.Flush();
            stream.Close();

            HttpWebResponse resp = (await request.GetResponseAsync()) as HttpWebResponse;
        }

        #endregion

        #region OpenWebsite

        /// <summary>
        /// Gets or sets a <see cref="RelayCommand"/> executed when we click on the link
        /// </summary>
        public RelayCommand<string> OpenWebsite { get; private set; }

        private void ExecuteOpenWebsiteCommand(string url)
        {
            Process.Start(url);
        }

        #endregion

        #region CreditButtonCommand

        /// <summary>
        /// Gets or sets a <see cref="RelayCommand"/> executed when we click on the Credit button
        /// </summary>
        public RelayCommand CreditButtonCommand { get; private set; }

        private void ExecuteCreditButtonCommand()
        {
            IsCreditPopupOpened = !IsCreditPopupOpened;
        }

        #endregion

        #endregion
    }
}
