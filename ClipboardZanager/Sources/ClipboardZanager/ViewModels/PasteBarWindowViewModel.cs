using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using ClipboardZanager.ComponentModel.Enums;
using ClipboardZanager.ComponentModel.Messages;
using ClipboardZanager.ComponentModel.UI.Controls;
using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Core.Desktop.Enums;
using ClipboardZanager.Core.Desktop.Events;
using ClipboardZanager.Core.Desktop.Models;
using ClipboardZanager.Core.Desktop.Services;
using ClipboardZanager.Properties;
using ClipboardZanager.Shared.Core;
using ClipboardZanager.Shared.Logs;
using ClipboardZanager.Shared.Services;
using ClipboardZanager.Strings;
using ClipboardZanager.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;
using Cursor = System.Windows.Forms.Cursor;
using System.Windows.Controls;
using ClipboardZanager.ComponentModel.UI;

namespace ClipboardZanager.ViewModels
{
    /// <summary>
    /// Interaction logic for <see cref="PasteBarWindow"/>
    /// </summary>
    internal sealed class PasteBarWindowViewModel : ViewModelBase
    {
        #region Fields

        private readonly DataService _dataService;
        private readonly ICollectionView _collectionView;

        private MouseAndKeyboardHookService _mouseAndKeyboardHookService;
        private AsyncObservableCollection<DataEntry> _dataEntries;
        private SearchQuery _searchQuery;
        private SearchType _searchType;
        private string _searchQueryString;
        private bool _canCloseIfMouseMovesAway;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the <see cref="LanguageManager"/>
        /// </summary>
        public LanguageManager Language => LanguageManager.GetInstance();

        /// <summary>
        /// Gets a value that defines whether a screen reader is running
        /// </summary>
        public bool IsScreenReaderRunning => SystemInfoHelper.IsScreenReaderRunning();

        /// <summary>
        /// Gets or sets the list of <see cref="DataEntry"/> from the clipboard.
        /// </summary>
        public AsyncObservableCollection<DataEntry> DataEntries
        {
            get { return _dataEntries; }
            set
            {
                _dataEntries = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets query in the search text box
        /// </summary>
        public string SearchQueryString
        {
            get { return _searchQueryString; }
            set
            {
                _searchQueryString = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the type or research.
        /// </summary>
        public SearchType SearchType
        {
            get { return _searchType; }
            set
            {
                _searchType = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Ignore the changes in the <see cref="SearchTextBox"/> of the <see cref="PasteBarWindow"/>. It is usefull when there is several monitors.
        /// </summary>
        internal bool IgnoreSearch { get; set; }

        /// <summary>
        /// Gets the collection view
        /// </summary>
        public ICollectionView CollectionView => _collectionView;

        /// <summary>
        /// Gets a value that defines whether any data from the clipboard is present or not.
        /// </summary>
        public bool NoPresentData => DataEntries.Count == 0;

        /// <summary>
        /// Gets a value that defines whether the performed search returned no result.
        /// </summary>
        public bool NoSearchResult => DataEntries.Count > 0 && CollectionView.IsEmpty;

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="PasteBarWindowViewModel"/> class.
        /// </summary>
        internal PasteBarWindowViewModel()
        {
            InitializeCommands();

            DataEntries = new AsyncObservableCollection<DataEntry>();
            if (IsInDesignMode)
            {
                DataEntries.Add(new DataEntry
                {
                    CanSynchronize = true,
                    Date = DateTime.Now,
                    IsFavorite = false,
                    Thumbnail = new Thumbnail
                    {
                        Type = ThumbnailDataType.Files
                    }
                });
            }
            else
            {
                _dataService = ServiceLocator.GetService<DataService>();
                DataEntries = _dataService.DataEntries;
            }

            _collectionView = CollectionViewSource.GetDefaultView(DataEntries);
            _collectionView.Filter = Filter;

            DataEntries.CollectionChanged += DataEntries_CollectionChanged;
        }

        #endregion

        #region Commands

        /// <summary>
        /// Initialize the commands of the View Model
        /// </summary>
        private void InitializeCommands()
        {
            DeactivatedCommand = new RelayCommand(ExecuteDeactivatedCommand, CanExecuteDeactivatedCommand);
            HideBarButtonCommand = new RelayCommand(ExecuteHideBarButtonCommand);
            SearchCommand = new RelayCommand(ExecuteSearchCommand);
            ToggleItemOptionCommand = new RelayCommand(ExecuteToggleItemOptionCommand);
            ToggleItemDeleteConfirmationCommand = new RelayCommand<Popup>(ExecuteToggleItemDeleteConfirmationCommand);
            ToggleItemDeleteAllConfirmationCommand = new RelayCommand<Popup>(ExecuteToggleItemDeleteAllConfirmationCommand);
            DeleteItemCommand = new RelayCommand<DataEntry>(ExecuteDeleteItemCommand);
            ClickHyperlinkCommand = new RelayCommand<string>(ExecuteClickHyperlinkCommand);
            ContextMenuOpenedCommand = new RelayCommand(ExecuteContextMenuOpeningCommand);
            CopyCommand = new RelayCommand<DataEntry>(ExecuteCopyCommand, CanExecuteCopyCommand);
            PasteCommand = new RelayCommand<DataEntry>(ExecutePasteCommand, CanExecutePasteCommand);
            DeleteAllCommand = new RelayCommand<Popup>(ExecuteDeleteAllCommand);
        }

        #region Deactivated

        /// <summary>
        /// Gets or sets a <see cref="RelayCommand"/> executed when the <see cref="PasteBarWindow"/> is deactivated
        /// </summary>
        public RelayCommand DeactivatedCommand { get; private set; }

        private bool CanExecuteDeactivatedCommand()
        {
            if (Debugger.IsAttached)
            {
                return false;
            }

            return true;
        }

        private void ExecuteDeactivatedCommand()
        {
            Logger.Instance.Information("Paste bar deactived.");
            HideBarButtonCommand.Execute(null);
        }

        #endregion

        #region HideBarButton

        /// <summary>
        /// Gets or sets a <see cref="RelayCommand"/> executed when click on the close button
        /// </summary>
        public RelayCommand HideBarButtonCommand { get; private set; }

        private void ExecuteHideBarButtonCommand()
        {
            if (_mouseAndKeyboardHookService != null)
            {
                if (Settings.Default.ClosePasteBarWhenMouseIsAway)
                {
                    _mouseAndKeyboardHookService.MouseAction -= MouseAndKeyboardHookService_MouseAction;
                }

                if (Settings.Default.ClosePasteBarWithHotKey)
                {
                    _mouseAndKeyboardHookService.UnregisterHotKey(Consts.PasteShortcutName);
                    _mouseAndKeyboardHookService.HotKeyDetected -= MouseAndKeyboardHookService_HotKeyDetected;
                }
                _mouseAndKeyboardHookService.Pause();
            }

            Logger.Instance.Information($"The paste bar window has been hidden.");

            MessengerInstance.Send(new ComponentModel.Messages.Message(), MessageIdentifiers.HidePasteBarWindow);
            var delayer = new Delayer<object>(TimeSpan.FromMilliseconds(200));
            delayer.Action += (sender, args) =>
            {
                _mouseAndKeyboardHookService = null;
                _canCloseIfMouseMovesAway = false;
                if (!string.IsNullOrEmpty(SearchQueryString) || SearchType != SearchType.All)
                {
                    SearchQueryString = string.Empty;
                    SearchType = SearchType.All;
                    Search();
                }
            };
            delayer.ResetAndTick();
        }

        #endregion 

        #region Search

        /// <summary>
        /// Gets or sets a <see cref="RelayCommand"/> executed when the user search something
        /// </summary>
        public RelayCommand SearchCommand { get; private set; }

        private void ExecuteSearchCommand()
        {
            if (IgnoreSearch)
            {
                return;
            }

            IgnoreSearch = true;

            var delayer = new Delayer<object>(TimeSpan.FromMilliseconds(250));
            delayer.Action += (sender, args) =>
            {
                IgnoreSearch = false;
            };
            delayer.ResetAndTick();

            var delayer2 = new Delayer<object>(TimeSpan.FromMilliseconds(10));
            delayer2.Action += (sender, args) =>
            {
                Logger.Instance.Information("Search in the paste bar started.");
                Search();
                RaisePropertyChanged(nameof(NoSearchResult));
            };
            delayer2.ResetAndTick();

            if (CoreHelper.IsUnitTesting())
            {
                Task.Delay(50).Wait();
                DispatcherHelper.DoEvents();
            }
        }

        #endregion

        #region ToggleItemOption

        /// <summary>
        /// Gets or sets a <see cref="RelayCommand"/> executed when the user click on an option of an item (favorite/do not synchronize...)
        /// </summary>
        public RelayCommand ToggleItemOptionCommand { get; private set; }

        private async void ExecuteToggleItemOptionCommand()
        {
            await _dataService.ReorganizeAsync(true);
        }

        #endregion

        #region ToggleItemDeleteConfirmation

        /// <summary>
        /// Gets or sets a <see cref="RelayCommand"/> executed when the user click on the Delete button of an item
        /// </summary>
        public RelayCommand<Popup> ToggleItemDeleteConfirmationCommand { get; private set; }

        private void ExecuteToggleItemDeleteConfirmationCommand(Popup popup)
        {
            popup.Opened += (sender, e) =>
            {
                Keyboard.ClearFocus();
                var firstFocus = VisualHelper.FindVisualChildren<System.Windows.Controls.Button>(popup.Child).FirstOrDefault();
                firstFocus?.Focus();
            };
            popup.IsOpen = !popup.IsOpen;
        }

        #endregion

        #region DeleteItem

        /// <summary>
        /// Gets or sets a <see cref="RelayCommand"/> executed when the user confirm the deletation of an item
        /// </summary>
        public RelayCommand<DataEntry> DeleteItemCommand { get; private set; }

        private async void ExecuteDeleteItemCommand(DataEntry dataEntry)
        {
            Logger.Instance.Information("Delete command activated.");
            await _dataService.RemoveDataAsync(dataEntry.Identifier, dataEntry.DataIdentifiers);
            RaisePropertyChanged(nameof(NoPresentData));
        }

        #endregion

        #region ClickHyperlink

        /// <summary>
        /// Gets or sets a <see cref="RelayCommand"/> executed when the user click on the hyper link of a data thumbnail
        /// </summary>
        public RelayCommand<string> ClickHyperlinkCommand { get; private set; }

        private void ExecuteClickHyperlinkCommand(string uri)
        {
            Logger.Instance.Information("Hyperlink activated.");
            Process.Start(uri);
            HideBarButtonCommand.Execute(null);
        }

        #endregion

        #region ContextMenuOpening

        /// <summary>
        /// Gets or sets a <see cref="RelayCommand"/> executed when the context menu is opening
        /// </summary>
        public RelayCommand ContextMenuOpenedCommand { get; private set; }

        private void ExecuteContextMenuOpeningCommand()
        {
            CopyCommand.RaiseCanExecuteChanged();
        }

        #endregion

        #region Copy

        /// <summary>
        /// Gets or sets a <see cref="RelayCommand"/> executed when the user click on the Copy menu (right click)
        /// </summary>
        public RelayCommand<DataEntry> CopyCommand { get; private set; }

        private bool CanExecuteCopyCommand(DataEntry dataEntry)
        {
            return dataEntry != null;
        }

        private void ExecuteCopyCommand(DataEntry dataEntry)
        {
            var delayer = new Delayer<DataEntry>(TimeSpan.FromMilliseconds(200));
            delayer.Action += (sender, args) =>
            {
                Logger.Instance.Information("Copy command activated.");
                Requires.NotNull(args.Data, nameof(args.Data));

                _mouseAndKeyboardHookService?.Pause();
                _dataService.CopyData(args.Data);
                _mouseAndKeyboardHookService?.Resume();
            };
            delayer.ResetAndTick(dataEntry);
        }

        #endregion

        #region Paste

        /// <summary>
        /// Gets or sets a <see cref="RelayCommand"/> executed when the user click on the Paste menu (right click) or do a double click on an item
        /// </summary>
        public RelayCommand<DataEntry> PasteCommand { get; private set; }

        private bool CanExecutePasteCommand(DataEntry dataEntry)
        {
            return dataEntry != null;
        }

        private void ExecutePasteCommand(DataEntry dataEntry)
        {
            var delayer = new Delayer<DataEntry>(TimeSpan.FromMilliseconds(200));
            delayer.Action += (sender, args) =>
            {
                Logger.Instance.Information("Paste command activated.");
                Requires.NotNull(args.Data, nameof(args.Data));
                HideBarButtonCommand.Execute(null);
                MessengerInstance.Send(new ComponentModel.Messages.Message(args.Data), MessageIdentifiers.PasteData);
            };
            delayer.ResetAndTick(dataEntry);
        }

        #endregion

        #region ToggleItemDeleteAllConfirmation

        /// <summary>
        /// Gets or sets a <see cref="RelayCommand"/> executed when the user click on the Delete all button
        /// </summary>
        public RelayCommand<Popup> ToggleItemDeleteAllConfirmationCommand { get; private set; }

        private void ExecuteToggleItemDeleteAllConfirmationCommand(Popup popup)
        {
            popup.Opened += (sender, e) =>
            {
                Keyboard.ClearFocus();
                var firstFocus = VisualHelper.FindVisualChildren<System.Windows.Controls.Button>(popup.Child).FirstOrDefault();
                firstFocus?.Focus();
            };
            popup.IsOpen = !popup.IsOpen;
        }

        #endregion

        #region DeleteAll

        /// <summary>
        /// Gets or sets a <see cref="RelayCommand"/> executed when the user click on the Delete All menu (right click)
        /// </summary>
        public RelayCommand<Popup> DeleteAllCommand { get; private set; }

        private async void ExecuteDeleteAllCommand(Popup popup)
        {
            Logger.Instance.Information("Delete all command activated.");
            popup.IsOpen = false;
            await _dataService.RemoveAllDataAsync();
            RaisePropertyChanged(nameof(NoPresentData));
        }

        #endregion

        #endregion

        #region Handled Methods

        private void MouseAndKeyboardHookService_MouseAction(object sender, MouseHookEventArgs e)
        {
            var mustClose = false;
            var activeScreen = Screen.FromPoint(new System.Drawing.Point(Cursor.Position.X, Cursor.Position.Y));
            var screen = SystemInfoHelper.GetAllScreenInfos().Single(s => s.DeviceName == activeScreen.DeviceName);

            switch (Settings.Default.PasteBarPosition)
            {
                case PasteBarPosition.Top:
                    if (e.Coords.Y >= (screen.Bounds.Bottom - screen.Bounds.Top) / 2)
                    {
                        if (_canCloseIfMouseMovesAway)
                        {
                            mustClose = true;
                        }
                    }
                    else
                    {
                        _canCloseIfMouseMovesAway = true;
                    }
                    break;

                case PasteBarPosition.Bottom:
                    if (e.Coords.Y <= (screen.Bounds.Bottom - screen.Bounds.Top) / 2)
                    {
                        if (_canCloseIfMouseMovesAway)
                        {
                            mustClose = true;
                        }
                    }
                    else
                    {
                        _canCloseIfMouseMovesAway = true;
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (mustClose)
            {
                _mouseAndKeyboardHookService.MouseAction -= MouseAndKeyboardHookService_MouseAction;
                Logger.Instance.Information($"Mouse moves away from the paste bar.");

                var delayer = new Delayer<object>(TimeSpan.FromMilliseconds(10));
                delayer.Action += (o, args) => HideBarButtonCommand.Execute(null);
                delayer.ResetAndTick();
            }
        }

        private void MouseAndKeyboardHookService_HotKeyDetected(object sender, HotKeyEventArgs e)
        {
            _mouseAndKeyboardHookService.HotKeyDetected -= MouseAndKeyboardHookService_HotKeyDetected;
            Logger.Instance.Information($"The keyboard shortcut has hit.");
            e.Handled = true;

            var delayer = new Delayer<object>(TimeSpan.FromMilliseconds(10));
            delayer.Action += (o, args) => HideBarButtonCommand.Execute(null);
            delayer.ResetAndTick();
        }

        private void DataEntries_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(NoPresentData));
            RaisePropertyChanged(nameof(NoSearchResult));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Display on the screen the paste bare with an animation.
        /// </summary>
        internal void DisplayBar()
        {
            var delayer = new Delayer<object>(TimeSpan.FromMilliseconds(250));
            delayer.Action += (sender, args) =>
            {
                if (Settings.Default.ClosePasteBarWhenMouseIsAway && !CoreHelper.IsUnitTesting())
                {
                    _mouseAndKeyboardHookService = ServiceLocator.GetService<MouseAndKeyboardHookService>();
                    _mouseAndKeyboardHookService.MouseAction += MouseAndKeyboardHookService_MouseAction;
                    _mouseAndKeyboardHookService.Resume();
                }

                if (Settings.Default.ClosePasteBarWithHotKey && !CoreHelper.IsUnitTesting())
                {
                    if (_mouseAndKeyboardHookService == null)
                    {
                        _mouseAndKeyboardHookService = ServiceLocator.GetService<MouseAndKeyboardHookService>();
                    }
                    _mouseAndKeyboardHookService.HotKeyDetected += MouseAndKeyboardHookService_HotKeyDetected;
                    _mouseAndKeyboardHookService.RegisterHotKey(Consts.PasteShortcutName, Settings.Default.KeyboardShortcut.Cast<Key>().ToArray());
                    _mouseAndKeyboardHookService.Resume();
                    Logger.Instance.Information($"The keyboard is listened. The expecting shortcut is {JsonConvert.SerializeObject(Settings.Default.KeyboardShortcut.Cast<Key>().ToArray())}.");
                }
            };
            delayer.ResetAndTick();
        }

        /// <summary>
        /// Perform a search in the data
        /// </summary>
        private void Search()
        {
            var isHookingPaused = true;
            if (_mouseAndKeyboardHookService != null)
            {
                isHookingPaused = _mouseAndKeyboardHookService.IsPaused;
                if (!isHookingPaused)
                {
                    _mouseAndKeyboardHookService.Pause();
                }
            }
            _searchQuery = new SearchQuery(SearchQueryString, SearchType);
            _collectionView.Refresh();

            if (!isHookingPaused)
            {
                _mouseAndKeyboardHookService?.DelayedResume(TimeSpan.FromSeconds(1));
            }
        }

        /// <summary>
        /// Filter the <see cref="DataEntries"/> with the <see cref="_searchQueryString"/>
        /// </summary>
        /// <param name="obj">The object from the data entries list</param>
        private bool Filter(object obj)
        {
            if (_searchQuery == null || (string.IsNullOrEmpty(_searchQuery.Query) && _searchQuery.Type == SearchType.All))
            {
                return true;
            }

            var dataEntry = obj as DataEntry;

            if (dataEntry == null)
            {
                return false;
            }

            return _dataService.MatchSearchQuery(_searchQuery, dataEntry);
        }

        #endregion
    }
}
