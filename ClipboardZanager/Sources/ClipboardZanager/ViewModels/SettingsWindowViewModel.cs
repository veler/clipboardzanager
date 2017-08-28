using System;
using ClipboardZanager.ComponentModel.Enums;
using ClipboardZanager.ComponentModel.UI.Controls;
using ClipboardZanager.Shared.Logs;
using ClipboardZanager.Strings;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using ClipboardZanager.Core.Desktop.ComponentModel;

namespace ClipboardZanager.ViewModels
{
    /// <summary>
    /// Interaction logic for <see cref="SettingsWindow"/>
    /// </summary>
    internal sealed class SettingsWindowViewModel : ViewModelBase
    {
        #region Fields

        private SettingsViewMode _viewMode = SettingsViewMode.General;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the <see cref="LanguageManager"/>
        /// </summary>
        public LanguageManager Language => LanguageManager.GetInstance();

        /// <summary>
        /// Gets or sets a value that defines which view must be displayed into the setting window.
        /// </summary>
        public SettingsViewMode ViewMode
        {
            get { return _viewMode; }
            set
            {
                Logger.Instance.Information($"The user switched to the '{value}' view mode in the settings window.");
                _viewMode = value;
                RaisePropertyChanged();
            }
        }

        #endregion 

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="SettingsWindowViewModel"/> class.
        /// </summary>
        internal SettingsWindowViewModel()
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
            CloseButtonCommand = new RelayCommand<BlurredWindow>(ExecuteCloseButtonCommand);
        }

        #region CloseButton

        /// <summary>
        /// Gets or sets a <see cref="RelayCommand"/> executed when click on the close button
        /// </summary>
        public RelayCommand<BlurredWindow> CloseButtonCommand { get; private set; }

        private void ExecuteCloseButtonCommand(BlurredWindow window)
        {
            Logger.Instance.Information($"The settings window has been closed.");
            window.Close();
        }

        #endregion

        #endregion
    }
}
