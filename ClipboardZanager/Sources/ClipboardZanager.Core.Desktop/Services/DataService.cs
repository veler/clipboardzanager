using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Core.Desktop.Enums;
using ClipboardZanager.Core.Desktop.Events;
using ClipboardZanager.Core.Desktop.IO;
using ClipboardZanager.Core.Desktop.Models;
using ClipboardZanager.Shared.Core;
using ClipboardZanager.Shared.Logs;
using ClipboardZanager.Shared.Services;
using Window = ClipboardZanager.Core.Desktop.Models.Window;

namespace ClipboardZanager.Core.Desktop.Services
{
    /// <summary>
    /// Provides a service that to manage the clipboard entry on the hard drive.
    /// </summary>
    internal sealed class DataService : IService
    {
        #region Fields

        private readonly Regex _creditCardRegex = new Regex(@"^(?:4[0-9]{12}(?:[0-9]{3})?|5[1-5][0-9]{14}|6(?:011|5[0-9][0-9])[0-9]{12}|3[47][0-9]{13}|3(?:0[0-5]|[68][0-9])[0-9]{11}|(?:2131|1800|35\d{3})\d{11})$", RegexOptions.Compiled);
        private readonly Regex _hasNumber = new Regex(@"[0-9]+", RegexOptions.Compiled);
        private readonly Regex _hasUpperChar = new Regex(@"[A-Z]+", RegexOptions.Compiled);

        private bool _lastCopiedDataWasCreditCard;
        private bool _lastCopiedDataWasPassword;
        private string _dataEntryFilePath;
        private string _cacheFilePath;
        private SecureString _dataEntryFilePassword;
        private SecureString _detectedPasswordOrCreditCard;
        private IServiceSettingProvider _settingProvider;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the full path to the clipboard data folder.
        /// </summary>
        internal string ClipboardDataPath { get; private set; }

        /// <summary>
        /// Gets the list of data entries
        /// </summary>
        internal AsyncObservableCollection<DataEntry> DataEntries { get; private set; }

        /// <summary>
        /// Gets cache that describes that status of all data entries.
        /// </summary>
        internal List<DataEntryCache> Cache { get; private set; }

        #endregion

        #region Events

        /// <summary>
        /// Raised when a credit card number is detected.
        /// </summary>
        internal event EventHandler<EventArgs> CreditCardNumberDetected;

        /// <summary>
        /// Raised when a credit card number is kept.
        /// </summary>
        internal event EventHandler<EventArgs> CreditCardNumberSaved;

        /// <summary>
        /// Raised when a password is detected.
        /// </summary>
        internal event EventHandler<EventArgs> PasswordDetected;

        /// <summary>
        /// Raised when a password is kept.
        /// </summary>
        internal event EventHandler<EventArgs> PasswordSaved;

        /// <summary>
        /// Raised when the migration is completed.
        /// </summary>
        internal event EventHandler<DataMigrationProgressEventArgs> DataMigrationProgress;

        #endregion

        #region Methods

        /// <inheritdoc/>
        public void Initialize(IServiceSettingProvider settingProvider)
        {
            _settingProvider = settingProvider;

            var appDataFolder = CoreHelper.GetAppDataFolder();

            Requires.NotNullOrWhiteSpace(appDataFolder, nameof(appDataFolder));

            DataEntries = new AsyncObservableCollection<DataEntry>();
            Cache = new List<DataEntryCache>();

            ClipboardDataPath = Path.Combine(appDataFolder, Consts.ClipboardDataFolderName);
            _dataEntryFilePath = Path.Combine(ClipboardDataPath, Consts.DataEntryFileName);
            _cacheFilePath = Path.Combine(ClipboardDataPath, Consts.CacheFileName);

            _dataEntryFilePassword = SecurityHelper.ToSecureString(SecurityHelper.EncryptString(SecurityHelper.ToSecureString(_settingProvider.GetSetting<string>("DropBoxAppKey") + _settingProvider.GetSetting<string>("OneDriveClientId"))));

            if (settingProvider.GetSetting<bool>("DataMigrationRequired"))
            {
                var delayer = new Delayer<object>(TimeSpan.FromMilliseconds(10));
                delayer.Action += async (sender1, eventArgs) =>
                {
                    await Task.Run(() => MigrateDataEntryFileAsync());
                };
                delayer.ResetAndTick();
            }
            else
            {
                LoadDataEntryFileAsync();
            }

            Logger.Instance.Information($"{GetType().Name} initialized.");
        }

        /// <inheritdoc/>
        public void Reset()
        {
            _lastCopiedDataWasCreditCard = false;
            _lastCopiedDataWasPassword = false;
            _detectedPasswordOrCreditCard = null;
        }

        /// <summary>
        /// Determines whether the input string looks like a credit card number or not.
        /// </summary>
        /// <param name="input">The string to test</param>
        /// <returns>Returns True is the string looks like a credit card number</returns>
        internal bool IsCreditCard(string input)
        {
            input = Regex.Replace(input, @"[^\d]", "");
            return _creditCardRegex.IsMatch(input);
        }

        /// <summary>
        /// Determines whether the input string looks like a password or not and comes from a web browser.
        /// </summary>
        /// <param name="input">The string to test</param>
        /// <param name="foregroundWindow">The windows where the user copied the data</param>
        /// <returns>Returns True is the string looks like a password and comes from a web browser.</returns>
        internal bool IsPassword(string input, Window foregroundWindow)
        {
            if (!Consts.WebBrowserIdentifier.Any(identifier => foregroundWindow.ApplicationIdentifier.Contains(identifier)))
            {
                return false;
            }

            var hasNumber = _hasNumber.IsMatch(input);
            var hasUpperChar = _hasUpperChar.IsMatch(input);
            var hasMinimum8Chars = input.Length >= 8;
            var hasMaximum32Chars = input.Length <= 32;
            var hasMaximum2Whitespaces = input.Count(char.IsWhiteSpace) <= 2;
            var inputWithoutSpecial = new string(input.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)).ToArray());

            return hasNumber && hasUpperChar && hasMinimum8Chars && hasMaximum32Chars && hasMaximum2Whitespaces && input != inputWithoutSpecial;
        }

        /// <summary>
        /// One time on two, if the passed text is exactly equals to the last call to this method, returns true and raise event to notify that a credit card will be kept by the application.
        /// </summary>
        /// <param name="text">The supposed credit card number.</param>
        /// <returns>True if the passed text is exactly equals to the last call to this method and that the application's settings defines that the software must avoid the credit card numbers.</returns>
        internal bool KeepOrIgnoreCreditCard(string text)
        {
            var ignored = false;

            if (_lastCopiedDataWasCreditCard && SecurityHelper.ToUnsecureString(_detectedPasswordOrCreditCard) == text)
            {
                CreditCardNumberSaved?.Invoke(this, new EventArgs());
            }
            else
            {
                if (_settingProvider.GetSetting<bool>("AvoidCreditCard"))
                {
                    ignored = true;
                    _lastCopiedDataWasCreditCard = true;
                    _detectedPasswordOrCreditCard = SecurityHelper.ToSecureString(text);
                }

                CreditCardNumberDetected?.Invoke(this, new EventArgs());
            }

            return ignored;
        }

        /// <summary>
        /// One time on two, if the passed text is exactly equals to the last call to this method, returns true and raise event to notify that a password will be kept by the application.
        /// </summary>
        /// <param name="text">The supposed password.</param>
        /// <returns>True if the passed text is exactly equals to the last call to this method and that the application's settings defines that the software must avoid the passwords.</returns>
        internal bool KeepOrIgnorePassword(string text)
        {
            var ignored = false;

            if (_lastCopiedDataWasPassword && SecurityHelper.ToUnsecureString(_detectedPasswordOrCreditCard) == text)
            {
                PasswordSaved?.Invoke(this, new EventArgs());
            }
            else
            {
                if (_settingProvider.GetSetting<bool>("AvoidPasswords"))
                {
                    ignored = true;
                    _lastCopiedDataWasPassword = true;
                    _detectedPasswordOrCreditCard = SecurityHelper.ToSecureString(text);
                }
                PasswordDetected?.Invoke(this, new EventArgs());
            }

            return ignored;
        }

        /// <summary>
        /// Generates a list of <see cref="DataIdentifier"/> for the given formats.
        /// </summary>
        /// <param name="formats">The list of data formats from the clipboard.</param>
        /// <returns>A list of <see cref="DataIdentifier"/></returns>
        internal List<DataIdentifier> GetDataIdentifiers(string[] formats)
        {
            var identifiers = new List<DataIdentifier>();

            foreach (var format in GetFormatsToKeep(formats))
            {
                identifiers.Add(new DataIdentifier { Identifier = GenerateNewGuid(), FormatName = format });
            }

            return identifiers;
        }

        /// <summary>
        /// Sort the data. The favorites will be placed on top of the list.
        /// </summary>
        /// <param name="saveDataEntryFile">Defines whether the data entry file must be saved</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        internal async Task ReorganizeAsync(bool saveDataEntryFile)
        {
            var indexOfFirstNonFavorite = DataEntries.IndexOf(DataEntries.FirstOrDefault(dataEntry => !dataEntry.IsFavorite));

            if (indexOfFirstNonFavorite > -1)
            {
                for (var i = indexOfFirstNonFavorite; i < DataEntries.Count; i++)
                {
                    var item = DataEntries[i];
                    if (!item.IsFavorite)
                    {
                        continue;
                    }

                    DataEntries.RemoveAt(i);
                    DataEntries.Insert(0, item);

                    var cacheItem = Cache.SingleOrDefault(dataEntryCache => dataEntryCache.Identifier == item.Identifier);
                    if (cacheItem != null)
                    {
                        Cache.Remove(cacheItem);
                        Cache.Insert(0, cacheItem);
                    }
                }
            }

            Logger.Instance.Information($"The data entries have been automatically reorganized.");

            if (saveDataEntryFile)
            {
                await SaveDataEntryFileAsync();
            }
        }

        /// <summary>
        /// Add the specific clipboard data to the data entries.
        /// </summary>
        /// <param name="e">The clipboard data from the hook.</param>
        /// <param name="identifiers">The list of identifiers for each data format.</param>
        /// <param name="foregroundWindow">The foreground window.</param>
        /// <param name="isCreditCard">Determines whether the data is a credit card number.</param>
        /// <param name="isPassword">Determines whether the data is a password.</param>
        internal async void AddDataEntry(ClipboardHookEventArgs e, List<DataIdentifier> identifiers, Window foregroundWindow, bool isCreditCard, bool isPassword)
        {
            Requires.NotNull(e, nameof(e));
            Requires.NotNull(identifiers, nameof(identifiers));
            Requires.NotNull(foregroundWindow, nameof(foregroundWindow));

            var entry = new DataEntry
            {
                Identifier = GenerateNewGuid(),
                Icon = foregroundWindow.Icon,
                Thumbnail = GenerateThumbnail(e.DataObject, isCreditCard, isPassword),
                Date = new DateTime(e.Time),
                IsCut = e.IsCut,
                IsFavorite = false,
                CanSynchronize = true,
                IconIsFromWindowStore = foregroundWindow.IsWindowsStoreApp,
                DataIdentifiers = identifiers
            };

            var cache = new DataEntryCache
            {
                Identifier = entry.Identifier,
                Status = DataEntryStatus.Added
            };

            DataEntries.Insert(0, entry);
            Cache.Insert(0, cache);

            entry = DataEntries.First();
            if (entry.Thumbnail.Type == ThumbnailDataType.Link)
            {
                // We doing it here to avoid blocking the part that runs on the UI thread.
                var value = DataHelper.FromBase64<Link>(entry.Thumbnail.Value);
                value.Title = SystemInfoHelper.GetWebPageTitle(value.Uri);
                entry.Thumbnail.Value = DataHelper.ToBase64<Link>(value);
            }

            await PurgeCacheAsync();
        }

        /// <summary>
        /// Remove a data from the data service
        /// </summary>
        /// <param name="identifier">The <see cref="Guid"/> that represents the data entry</param>
        /// <param name="identifiers">The list of <see cref="DataIdentifier"/> that represents the data</param>
        /// <param name="saveDataEntryFile">Defines wether the data entry file must be saved</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        internal async Task RemoveDataAsync(Guid identifier, List<DataIdentifier> identifiers, bool saveDataEntryFile = true)
        {
            Requires.NotNull(identifier, nameof(identifier));
            Requires.NotNull(identifiers, nameof(identifiers));

            Logger.Instance.Information($"The data {identifier} will be removed.");
            DataEntries.Remove(DataEntries.Single(entry => entry.Identifier == identifier));

            if (ServiceLocator.GetService<CloudStorageService>().IsLinkedToAService)
            {
                Cache.Single(entry => entry.Identifier == identifier).Status = DataEntryStatus.Deleted;
            }
            else
            {
                Cache.Remove(Cache.Single(entry => entry.Identifier == identifier));
            }

            foreach (var dataIdentifier in identifiers)
            {
                var dataFilePath = Path.Combine(ClipboardDataPath, $"{dataIdentifier.Identifier}.dat");

                if (File.Exists(dataFilePath))
                {
                    try
                    {
                        File.Delete(dataFilePath);
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.Error(ex);
                    }
                }
            }

            Logger.Instance.Information($"The data {identifier} has been removed.");

            if (saveDataEntryFile)
            {
                await SaveDataEntryFileAsync();
            }
        }

        /// <summary>
        /// Remove all the data from the data service
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        internal async Task RemoveAllDataAsync()
        {
            DataEntries.Clear();

            foreach (var dataEntryCache in Cache)
            {
                dataEntryCache.Status = DataEntryStatus.Deleted;
            }

            ClearCache();
            await Task.CompletedTask;
        }

        /// <summary>
        /// Ensure that the directory that must contains all the clipboard data exists.
        /// </summary>
        internal void EnsureDataFolderExists()
        {
            if (!Directory.Exists(ClipboardDataPath))
            {
                Directory.CreateDirectory(ClipboardDataPath);
                Logger.Instance.Information($"The data folder has been created.");
            }
        }

        /// <summary>
        /// Check if the specified <see cref="DataEntry"/> match the <see cref="SearchQuery"/>
        /// </summary>
        /// <param name="searchQuery">The query typed by the user</param>
        /// <param name="dataEntry">The data entry to check</param>
        /// <returns>Returns True if it match.</returns>
        internal bool MatchSearchQuery(SearchQuery searchQuery, DataEntry dataEntry)
        {
            if (dataEntry.Thumbnail.Type == ThumbnailDataType.Unknow)
            {
                return false;
            }

            Logger.Instance.Information($"Trying to match the data {dataEntry.Identifier}.");

            var dataObject = new DataObject();

            if (dataEntry.Thumbnail.Type == ThumbnailDataType.String)
            {
                var identifier = dataEntry.DataIdentifiers.FirstOrDefault(id => id.FormatName == DataFormats.UnicodeText);
                if (identifier != null)
                {
                    var dataFilePath = Path.Combine(ClipboardDataPath, $"{identifier.Identifier}.dat");
                    if (File.Exists(dataFilePath))
                    {
                        Logger.Instance.Information("Loading the unicode text data from the entry.");
                        var dataPassword = SecurityHelper.ToSecureString(SecurityHelper.EncryptString(SecurityHelper.ToSecureString(identifier.Identifier.ToString())));
                        Requires.NotNull(dataPassword, nameof(dataPassword));

                        using (var fileStream = File.OpenRead(dataFilePath))
                        using (var aesStream = new AesStream(fileStream, dataPassword, SecurityHelper.GetSaltKeys(dataPassword).GetBytes(16)))
                        {
                            aesStream.AutoDisposeBaseStream = false;
                            var buffer = new byte[aesStream.Length];
                            aesStream.Read(buffer, 0, buffer.Length);
                            dataObject.SetText(Encoding.Unicode.GetString(buffer));
                            Logger.Instance.Information("Unicode text data from the entry loaded.");
                        }
                    }
                }
            }

            switch (searchQuery.Type)
            {
                case SearchType.All:
                    // We avoid the pictures because it makes no sense to search a picture with a text in the case of this app.

                    if (dataEntry.Thumbnail.Type == ThumbnailDataType.Link)
                    {
                        return MatchLink(DataHelper.FromBase64<Link>(dataEntry.Thumbnail.Value), searchQuery.Query);
                    }

                    if (dataEntry.Thumbnail.Type == ThumbnailDataType.Files)
                    {
                        return MatchFiles(dataEntry.Thumbnail.GetFilesPath(), searchQuery.QueryRegex);
                    }

                    return MatchText(dataObject, searchQuery.Query);

                case SearchType.Text:
                    if (dataEntry.Thumbnail.Type == ThumbnailDataType.String)
                    {
                        return MatchText(dataObject, searchQuery.Query);
                    }
                    break;

                case SearchType.Link:
                    if (dataEntry.Thumbnail.Type == ThumbnailDataType.Link)
                    {
                        return MatchLink(DataHelper.FromBase64<Link>(dataEntry.Thumbnail.Value), searchQuery.Query);
                    }
                    break;

                case SearchType.File:
                    if (dataEntry.Thumbnail.Type == ThumbnailDataType.Files)
                    {
                        return MatchFiles(dataEntry.Thumbnail.GetFilesPath(), searchQuery.QueryRegex);
                    }
                    break;

                case SearchType.Image:
                    if (dataEntry.Thumbnail.Type == ThumbnailDataType.Bitmap)
                    {
                        return string.IsNullOrEmpty(searchQuery.Query);
                    }
                    break;

                default:
                    return false;
            }

            return false;
        }

        /// <summary>
        /// Set the data from the specified data entry to the clipboard.
        /// </summary>
        /// <param name="dataEntry">The data entry to copy</param>
        internal void CopyData(DataEntry dataEntry)
        {
            Requires.NotNull(dataEntry, nameof(dataEntry));
            DispatcherHelper.ThrowIfNotStaThread();

            var clipboardService = ServiceLocator.GetService<ClipboardService>();
            clipboardService.SetClipboard(dataEntry.DataIdentifiers);
        }

        /// <summary>
        /// Make the difference between the local cache and the cache from the Cloud.
        /// </summary>
        /// <param name="cloudDataEntryFromServer">The list of <see cref="CloudDataEntry"/> that comes from the cloud.</param>
        /// <returns>A list of <see cref="CloudDataEntry"/> that corresponds to the new cache to send to the cloud.</returns>
        internal List<CloudDataEntry> DifferenceLocalAndCloudDataEntries(IReadOnlyCollection<CloudDataEntry> cloudDataEntryFromServer)
        {
            Requires.NotNull(cloudDataEntryFromServer, nameof(cloudDataEntryFromServer));
            Logger.Instance.Information("Make the difference between the local cache and the cache from the Cloud.");

            var result = new List<CloudDataEntry>();

            foreach (var cacheEntry in Cache)
            {
                var addToResult = false;

                switch (cacheEntry.Status)
                {
                    case DataEntryStatus.Added:
                        // If the data has been added locally, we should add it on the server
                        addToResult = true;
                        break;

                    case DataEntryStatus.DidNotChanged:
                        // If the data has already been synchronized and is still present locally.
                        if (cloudDataEntryFromServer.Any(dataEntry => dataEntry.Identifier == cacheEntry.Identifier))
                        {
                            // And that the data exists on the server too, so we keep it locally.
                            addToResult = true;
                        }
                        // Otherwise, we don't keep it because it has probably bee removed byt another device from the server.
                        break;

                    case DataEntryStatus.Deleted:
                        // If the data has been deleted locally, we don't keep it on the server.
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (addToResult)
                {
                    var dataEntry = DataEntries.FirstOrDefault(item => item.Identifier == cacheEntry.Identifier);

                    if (dataEntry != null && dataEntry.CanSynchronize && dataEntry.Thumbnail.Type != ThumbnailDataType.Files)
                    {
                        var dataToAdd = new CloudDataEntry
                        {
                            Identifier = dataEntry.Identifier,
                            DataIdentifiers = dataEntry.DataIdentifiers,
                            Date = dataEntry.Date,
                            IsFavorite = dataEntry.IsFavorite,
                            ThumbnailValue = dataEntry.Thumbnail.Value,
                            ThumbnailDataType = dataEntry.Thumbnail.Type,
                            Icon = dataEntry._icon
                        };

                        result.Add(dataToAdd);
                    }
                }
            }

            foreach (var cloudDataEntry in cloudDataEntryFromServer)
            {
                // If the data is in the cloud but not locally, we add/download it.
                if (Cache.All(dataEntry => dataEntry.Identifier != cloudDataEntry.Identifier))
                {
                    result.Add(cloudDataEntry);
                }
            }

            return result;
        }

        /// <summary>
        /// Set all the status of cache data entries to <see cref="DataEntryStatus.DidNotChanged"/> and save the cache.
        /// </summary>
        /// <param name="cloudDataEntries">The data entries from the cloud</param>
        /// <param name="onlyRemoveLocalDataNotPresentOnServer">Defines wether if this method must only remove the local data that are not present on the server</param>
        /// <param name="localFrozenCache">A "frozen copy/clone" of the cache. This value must not match the <see cref="Cache"/> property if we do a Equals.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        internal async Task MakeCacheSynchronized(List<CloudDataEntry> cloudDataEntries, bool onlyRemoveLocalDataNotPresentOnServer, List<DataEntryCache> localFrozenCache)
        {
            var mouseAndKeyboardHookService = ServiceLocator.GetService<MouseAndKeyboardHookService>();
            var paused = mouseAndKeyboardHookService.IsPaused;
            if (!paused)
            {
                mouseAndKeyboardHookService.Pause();
            }

            if (onlyRemoveLocalDataNotPresentOnServer)
            {
                var save = false;
                foreach (var cacheEntry in localFrozenCache.AsParallel().Where(cacheEntry => cacheEntry.Status == DataEntryStatus.DidNotChanged && cloudDataEntries.All(cloudDataEntry => cloudDataEntry.Identifier != cacheEntry.Identifier)))
                {
                    save = true;
                    var dataEntry = DataEntries.FirstOrDefault(data => data.Identifier == cacheEntry.Identifier);
                    if (dataEntry != null && dataEntry.CanSynchronize)
                    {
                        await RemoveDataAsync(dataEntry.Identifier, dataEntry.DataIdentifiers, false);
                    }
                }

                if (save)
                {
                    await SaveDataEntryFileAsync();
                }

                if (!paused)
                {
                    mouseAndKeyboardHookService.DelayedResume(TimeSpan.FromSeconds(1));
                }
                return;
            }

            Requires.NotNull(localFrozenCache, nameof(localFrozenCache));
            Requires.IsFalse(Cache.Equals(localFrozenCache));

            foreach (var frozenDataEntryCache in localFrozenCache.Where(dataEntryCache => dataEntryCache.Status == DataEntryStatus.Added || dataEntryCache.Status == DataEntryStatus.Deleted))
            {
                var dataEntryCache = Cache.SingleOrDefault(item => item.Identifier == frozenDataEntryCache.Identifier);
                if (dataEntryCache == null)
                {
                    continue;
                }

                if (dataEntryCache.Status == DataEntryStatus.Added)
                {
                    dataEntryCache.Status = DataEntryStatus.DidNotChanged;
                }
                else if (dataEntryCache.Status == DataEntryStatus.Deleted)
                {
                    Cache.Remove(dataEntryCache);
                }
            }

            foreach (var cloudDataEntry in cloudDataEntries.AsParallel().Where(cloudDataEntry => Cache.All(dataEntryCache => cloudDataEntry.Identifier != dataEntryCache.Identifier)))
            {
                var entry = new DataEntry
                {
                    Identifier = cloudDataEntry.Identifier,
                    Thumbnail = new Thumbnail { Type = cloudDataEntry.ThumbnailDataType, Value = cloudDataEntry.ThumbnailValue },
                    Date = cloudDataEntry.Date,
                    IsCut = false,
                    IsFavorite = cloudDataEntry.IsFavorite,
                    CanSynchronize = true,
                    IconIsFromWindowStore = false,
                    DataIdentifiers = cloudDataEntry.DataIdentifiers
                };

                if (!string.IsNullOrWhiteSpace(cloudDataEntry.Icon))
                {
                    entry.Icon = (BitmapImage)DataHelper.ByteArrayToBitmapSource(DataHelper.ByteArrayFromBase64(cloudDataEntry.Icon));
                }

                var cache = new DataEntryCache
                {
                    Identifier = entry.Identifier,
                    Status = DataEntryStatus.DidNotChanged
                };

                var indexOfFirstNonFavorite = DataEntries.IndexOf(DataEntries.FirstOrDefault(dataEntry => !dataEntry.IsFavorite));
                if (entry.IsFavorite)
                {
                    if (indexOfFirstNonFavorite > -1)
                    {
                        var favoritesItem = DataEntries.Take(indexOfFirstNonFavorite).ToList();
                        favoritesItem.Add(entry);
                        favoritesItem = favoritesItem.OrderByDescending(dataEntry => dataEntry.Date).ToList();

                        var index = favoritesItem.IndexOf(favoritesItem.Single(dataEntry => dataEntry.Identifier == entry.Identifier));
                        DataEntries.Insert(index, entry);
                        Cache.Insert(index, cache);
                    }
                    else
                    {
                        DataEntries.Insert(0, entry);
                        Cache.Insert(0, cache);
                    }
                }
                else
                {
                    if (indexOfFirstNonFavorite > -1)
                    {
                        var nonFavoritesItems = DataEntries.Skip(indexOfFirstNonFavorite).ToList();
                        nonFavoritesItems.Add(entry);
                        nonFavoritesItems = nonFavoritesItems.OrderByDescending(dataEntry => dataEntry.Date).ToList();

                        var index = indexOfFirstNonFavorite + nonFavoritesItems.IndexOf(nonFavoritesItems.Single(dataEntry => dataEntry.Identifier == entry.Identifier));
                        DataEntries.Insert(index, entry);
                        Cache.Insert(index, cache);
                    }
                    else
                    {
                        var items = DataEntries.ToList();
                        items.Add(entry);
                        items = items.OrderByDescending(dataEntry => dataEntry.Date).ToList();

                        var index = items.IndexOf(items.Single(dataEntry => dataEntry.Identifier == entry.Identifier));
                        DataEntries.Insert(index, entry);
                        Cache.Insert(index, cache);
                    }
                }
            }

            await PurgeCacheAsync();
            if (!paused)
            {
                mouseAndKeyboardHookService.DelayedResume(TimeSpan.FromSeconds(1));
            }
        }

        /// <summary>
        /// Read a stream by respecting the <see cref="Consts.ClipboardDataBufferSize"/> constant as a rule, and write the data into another stream. Basically it is doing a copy by using low memory.
        /// </summary>
        /// <param name="fromStream">The stream to copy.</param>
        /// <param name="toStream">The empty stream that will receive the data.</param>
        internal void CopyData(Stream fromStream, Stream toStream)
        {
            var readLength = 0;
            var dataSize = (int)fromStream.Length;
            while (readLength < dataSize)
            {
                var bufferSize = Consts.ClipboardDataBufferSize;
                var buffer = new byte[bufferSize];

                // resize the buffer if we are at the end of the data and that it remains less than the default buffer size to read.
                if (dataSize - readLength < bufferSize)
                {
                    bufferSize = dataSize - readLength;
                    buffer = new byte[bufferSize];
                }

                fromStream.Read(buffer, 0, bufferSize);
                toStream.Write(buffer, 0, bufferSize);

                readLength += bufferSize;
            }

            toStream.Position = 0;
        }

        /// <summary>
        /// Load the data entry from the hard drive.
        /// </summary>
        private async void LoadDataEntryFileAsync()
        {
            EnsureDataFolderExists();

            if (!_settingProvider.GetSetting<bool>("KeepDataAfterReboot"))
            {
                ClearCache();
            }

            if (File.Exists(_dataEntryFilePath))
            {
                try
                {
                    LoadDataEntries(_dataEntryFilePassword);
                    Logger.Instance.Information($"The data entry file has been loaded.");
                }
                catch (Exception ex)
                {
                    // The assembly as been rebuilt. The file is not readable from a build to another.
                    Logger.Instance.Warning($"The data entry file failed to be loaded : {ex.Message}. The assembly as may be been rebuilt. The file is not readable from a build to another.");
                    ClearCache();
                }
            }

            if (File.Exists(_cacheFilePath))
            {
                try
                {
                    LoadCache(_dataEntryFilePassword);
                    Logger.Instance.Information($"The data entry cache file has been loaded.");
                }
                catch (Exception ex)
                {
                    // The assembly as been rebuilt. The file is not readable from a build to another.
                    Logger.Instance.Warning($"The data entry cache file failed to be loaded : {ex.Message}. The assembly as may be been rebuilt. The file is not readable from a build to another.");
                    ClearCache();
                }
            }

            await PurgeCacheAsync();
        }

        /// <summary>
        /// Load the data entry from a previous version of the app, and migrate it to the current version.
        /// </summary>
        private async void MigrateDataEntryFileAsync()
        {
            if (string.IsNullOrWhiteSpace(_settingProvider.GetSetting<string>("CurrentVersion")))
            {
                _settingProvider.SetSetting("DataMigrationRequired", false);
                _settingProvider.SetSetting("CurrentVersion", CoreHelper.GetApplicationVersion().ToString());
                return;
            }

            DataMigrationProgress?.Invoke(this, new DataMigrationProgressEventArgs(0, false, false));

            EnsureDataFolderExists();

            if (!_settingProvider.GetSetting<bool>("KeepDataAfterReboot"))
            {
                ClearCache();
            }

            var failed = false;
            var oldVersion = SecurityHelper.ToSecureString(_settingProvider.GetSetting<string>("CurrentVersion"));
            var dropBoxAppKey = _settingProvider.GetSetting<string>("DropBoxAppKey");
            var oneDriveClientId = _settingProvider.GetSetting<string>("OneDriveClientId");
            var oldDataEntryFilePassword = SecurityHelper.ToSecureString(SecurityHelper.EncryptString(SecurityHelper.ToSecureString(SecurityHelper.EncryptString(SecurityHelper.DecryptString(dropBoxAppKey), oldVersion) + SecurityHelper.EncryptString(SecurityHelper.DecryptString(oneDriveClientId), oldVersion)), oldVersion));
            DataMigrationProgress?.Invoke(this, new DataMigrationProgressEventArgs(5, false, false));

            if (File.Exists(_dataEntryFilePath))
            {
                try
                {
                    LoadDataEntries(oldDataEntryFilePassword);
                    Logger.Instance.Information($"The data entry file has been loaded and migrated.");
                    DataMigrationProgress?.Invoke(this, new DataMigrationProgressEventArgs(10, false, false));
                }
                catch (Exception ex)
                {
                    // The assembly as been rebuilt. The file is not readable from a build to another.
                    Logger.Instance.Warning($"The data entry file failed to be migrated : {ex.Message}");
                    ClearCache();
                    failed = true;
                }
            }

            if (!failed && File.Exists(_cacheFilePath))
            {
                try
                {
                    LoadCache(oldDataEntryFilePassword);
                    Logger.Instance.Information($"The data entry cache file has been loaded and migrated.");
                    DataMigrationProgress?.Invoke(this, new DataMigrationProgressEventArgs(15, false, false));
                }
                catch (Exception ex)
                {
                    // The assembly as been rebuilt. The file is not readable from a build to another.
                    Logger.Instance.Warning($"The data entry cache file failed to be migrated : {ex.Message}");
                    ClearCache();
                    failed = true;
                }
            }

            if (!failed)
            {
                try
                {
                    var dataIdentifiers = DataEntries.SelectMany(dataEntry => dataEntry.DataIdentifiers);
                    var dataIdentifiersCount = dataIdentifiers.Count();
                    var i = 0.0;

                    foreach (var dataIdentifier in dataIdentifiers)
                    {
                        var fileName = $"{dataIdentifier.Identifier}.dat";
                        var dataFilePath = Path.Combine(ClipboardDataPath, fileName);
                        var temporaryDataFilePath = Path.Combine(ClipboardDataPath, $"{fileName}.temp");
                        var oldDataPassword = SecurityHelper.ToSecureString(SecurityHelper.EncryptString(SecurityHelper.ToSecureString(dataIdentifier.Identifier.ToString()), oldVersion));
                        var newDataPassword = SecurityHelper.ToSecureString(SecurityHelper.EncryptString(SecurityHelper.ToSecureString(dataIdentifier.Identifier.ToString())));

                        if (!File.Exists(dataFilePath))
                        {
                            throw new FileNotFoundException(dataFilePath, dataFilePath);
                        }

                        using (var fileStream = File.OpenRead(dataFilePath))
                        using (var temporaryFileStream = File.Open(temporaryDataFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                        using (var aesStream = new AesStream(fileStream, oldDataPassword, SecurityHelper.GetSaltKeys(oldDataPassword).GetBytes(16)))
                        using (var cloudAesStream = new AesStream(temporaryFileStream, newDataPassword, SecurityHelper.GetSaltKeys(newDataPassword).GetBytes(16)))
                        {
                            CopyData(aesStream, cloudAesStream);
                        }

                        File.Delete(dataFilePath);
                        File.Move(temporaryDataFilePath, dataFilePath);

                        i++;
                        DataMigrationProgress?.Invoke(this, new DataMigrationProgressEventArgs((int)((i / dataIdentifiersCount) * 80) + 15, false, false));
                    }
                }
                catch (Exception ex)
                {
                    // The assembly as been rebuilt. The file is not readable from a build to another.
                    Logger.Instance.Warning($"A data file failed to be migrated : {ex.Message}");
                    ClearCache();
                    failed = true;
                }
            }

            await PurgeCacheAsync();

            _settingProvider.SetSetting("DataMigrationRequired", false);
            _settingProvider.SetSetting("CurrentVersion", CoreHelper.GetApplicationVersion().ToString());

            DataMigrationProgress?.Invoke(this, new DataMigrationProgressEventArgs(100, true, failed));
        }

        /// <summary>
        /// Load the data entry file from the hard drive.
        /// </summary>
        /// <param name="password">The password to read the data.</param>
        private void LoadDataEntries(SecureString password)
        {
            AsyncObservableCollection<DataEntry> entries;

            using (var fileStream = File.OpenRead(_dataEntryFilePath))
            using (var aesStream = new AesStream(fileStream, password, SecurityHelper.GetSaltKeys(password).GetBytes(16)))
            {
                aesStream.AutoDisposeBaseStream = false;
                var data = new byte[aesStream.Length];
                aesStream.Read(data, 0, data.Length);
                entries = DataHelper.FromByteArray<AsyncObservableCollection<DataEntry>>(data);
            }

            foreach (var dataEntry in entries)
            {
                DataEntries.Add(dataEntry);
            }
        }

        /// <summary>
        /// Load the cache file from the hard drive.
        /// </summary>
        /// <param name="password">The password to decrypt the data.</param>
        private void LoadCache(SecureString password)
        {
            List<DataEntryCache> entries;

            using (var fileStream = File.OpenRead(_cacheFilePath))
            using (var aesStream = new AesStream(fileStream, password, SecurityHelper.GetSaltKeys(password).GetBytes(16)))
            {
                aesStream.AutoDisposeBaseStream = false;
                var data = new byte[aesStream.Length];
                aesStream.Read(data, 0, data.Length);
                entries = DataHelper.FromByteArray<List<DataEntryCache>>(data);
            }

            foreach (var dataEntryCache in entries)
            {
                Cache.Add(dataEntryCache);
            }
        }

        /// <summary>
        /// Save the data entry to the hard drive.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private Task SaveDataEntryFileAsync()
        {
            EnsureDataFolderExists();

            SaveDataFile(_dataEntryFilePath, DataEntries);
            Logger.Instance.Information($"The data entry file has been saved.");

            SaveDataFile(_cacheFilePath, Cache);
            Logger.Instance.Information($"The data entry cache file has been saved.");

            return Task.CompletedTask;
        }

        /// <summary>
        /// Encrypt and save the specified data on the hard drive.
        /// </summary>
        /// <param name="filePath">The full path to the file to save.</param>
        /// <param name="dataToSave">The data to save.</param>
        private void SaveDataFile(string filePath, object dataToSave)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            using (var fileStream = File.OpenWrite(filePath))
            using (var aesStream = new AesStream(fileStream, _dataEntryFilePassword, SecurityHelper.GetSaltKeys(_dataEntryFilePassword).GetBytes(16)))
            {
                aesStream.AutoDisposeBaseStream = false;
                var data = DataHelper.ToByteArray(dataToSave);
                aesStream.Write(data, 0, data.Length);
                aesStream.Position = 0;
            }
        }

        /// <summary>
        /// Clean the data by applying the limit count of data and the expire date.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task PurgeCacheAsync()
        {
            var dataToRemove = new List<DataEntry>();
            var maxDataToKeep = _settingProvider.GetSetting<int>("MaxDataToKeep");
            var expireLimit = TimeSpan.FromDays(_settingProvider.GetSetting<int>("DateExpireLimit"));

            await ReorganizeAsync(false);

            if (DataEntries.Count > maxDataToKeep)
            {
                dataToRemove = DataEntries.Skip(maxDataToKeep).Where(dataEntry => !dataEntry.IsFavorite).ToList();
            }

            dataToRemove = dataToRemove.Union(DataEntries.AsParallel().Where(dataEntry => DateTime.Now - dataEntry.Date > expireLimit && !dataEntry.IsFavorite)).ToList();

            foreach (var data in dataToRemove)
            {
                await RemoveDataAsync(data.Identifier, data.DataIdentifiers, false);
            }

            Logger.Instance.Information($"The data cache have been purged.");
            await SaveDataEntryFileAsync();
        }

        /// <summary>
        /// Remove all data from the software cache.
        /// </summary>
        private void ClearCache()
        {
            // delete all files except the cache file.
            if (Directory.Exists(ClipboardDataPath))
            {
                foreach (var file in Directory.GetFiles(ClipboardDataPath).Where(file => file != _cacheFilePath))
                {
                    File.Delete(file);
                }
            }

            Logger.Instance.Information($"The data cache has been cleared.");
        }

        /// <summary>
        /// Generate a <see cref="Thumbnail"/> from the clipboard's data
        /// </summary>
        /// <param name="dataObject">The <see cref="DataObject"/> that contains the clipboard's data</param>
        /// <param name="isCreditCard">Indicated that the <see cref="DataObject"/> contains a credit card number</param>
        /// <param name="isPassword">Indicated that the <see cref="DataObject"/> contains a password</param>
        /// <returns>A <see cref="Thumbnail"/> that represent a small part of the clipboard's data</returns>
        private Thumbnail GenerateThumbnail(DataObject dataObject, bool isCreditCard, bool isPassword)
        {
            var value = string.Empty;
            var type = ThumbnailDataType.Unknow;

            if (dataObject.ContainsImage() && dataObject.GetDataPresent(DataFormats.Dib, false))
            {
                try
                {
                    value = DataHelper.ToBase64(DataHelper.BitmapSourceToByteArray(DataHelper.DeviceIndependentBitmapToBitmapSource(dataObject.GetData(DataFormats.Dib, false) as MemoryStream, 256)));
                    type = ThumbnailDataType.Bitmap;
                }
                catch (Exception ex)
                {
                    type = ThumbnailDataType.String;
                    value = DataHelper.ToBase64(string.Format("Unable to generate a thumbnail : {0}", ex.Message));
                    Logger.Instance.Warning(string.Format("Unable to generate a thumbnail : {0}", ex.Message));
                }
            }
            else if (dataObject.ContainsFileDropList())
            {
                var filesSource = dataObject.GetFileDropList().Cast<string>().ToList();

                value = DataHelper.ToBase64(filesSource);
                type = ThumbnailDataType.Files;
            }
            else if (dataObject.ContainsText())
            {
                var text = dataObject.GetText();

                if (isCreditCard)
                {
                    if (!string.IsNullOrEmpty(text))
                    {
                        text = text.Replace("-", string.Empty);
                        text = text.Replace("_", string.Empty);
                        text = text.Replace(" ", string.Empty);
                    }

                    if (text.Length == 16)
                    {
                        text = text.Substring(0, 4) + '-' + new string(Consts.PasswordMask, 4) + '-' + new string(Consts.PasswordMask, 4) + '-' + text.Substring(12);
                    }
                    else
                    {
                        text = new string(Consts.PasswordMask, text.Length);
                    }
                }
                else if (isPassword)
                {
                    text = text.Substring(0, 1) + new string(Consts.PasswordMask, text.Length - 2) + text.Substring(text.Length - 1);
                }
                else if (text.Length > 253)
                {
                    text = text.Substring(0, Math.Min(text.Length, 250));
                    text += "...";
                }

                var isUri = Uri.TryCreate(text, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps || uriResult.Scheme == Uri.UriSchemeFtp || uriResult.Scheme == Uri.UriSchemeMailto);
                if (isUri)
                {
                    type = ThumbnailDataType.Link;
                    value = DataHelper.ToBase64(new Link { Uri = text });
                }
                else
                {
                    type = ThumbnailDataType.String;
                    value = DataHelper.ToBase64(text);
                }
            }

            return new Thumbnail { Type = type, Value = value };
        }

        /// <summary>
        /// Returns the list of clipboard data format to keep, depending on the user's settings and what is in the clipboard.
        /// </summary>
        /// <param name="formats">The list of data formats from the clipboard.</param>
        /// <returns>A list of format to keep.</returns>
        private IEnumerable<string> GetFormatsToKeep(string[] formats)
        {
            var types = GetPotentialAssociatedDataTypes();

            var detectedDataTypes = new Dictionary<SupportedDataType, IEnumerable<string>>();

            foreach (SupportedDataType dataType in Enum.GetValues(typeof(SupportedDataType)))
            {
                var detectedTypes = formats.Intersect(types[dataType]);
                if (detectedTypes.Any())
                {
                    detectedDataTypes.Add(dataType, detectedTypes);
                }
            }

            if (detectedDataTypes.Count > 0)
            {
                var result = detectedDataTypes.OrderByDescending(item => item.Value.Count()).First();

                if (_settingProvider.GetSetting<ArrayList>("KeepDataTypes").Contains((int)result.Key))
                {
                    return result.Value;
                }
            }

            return new List<string>();
        }

        /// <summary>
        /// Determines wether the data from the clipboard (which must be a text) contains the specified string.
        /// </summary>
        /// <param name="dataObject">The data from the clipboard</param>
        /// <param name="query">The string to search</param>
        /// <returns>True if it is contained</returns>
        private bool MatchText(DataObject dataObject, string query)
        {
            if (dataObject.ContainsText())
            {
                var text = dataObject.GetText();
                var toLowerQuery = query.ToLower();

                if (toLowerQuery == query)
                {
                    if (text.ToLower().Contains(toLowerQuery))
                    {
                        return true;
                    }
                }
                else if (text.Contains(query))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines wether the data from the clipboard (which must be a link) contains the specified string.
        /// </summary>
        /// <param name="link">The link</param>
        /// <param name="query">The string to search</param>
        /// <returns>True if it is contained</returns>
        private bool MatchLink(Link link, string query)
        {
            var toLowerQuery = query.ToLower();

            if (toLowerQuery == query)
            {
                if (link.Title.ToLower().Contains(toLowerQuery) || link.Uri.ToLower().Contains(toLowerQuery))
                {
                    return true;
                }
            }
            else if (link.Title.Contains(query) || link.Uri.Contains(query))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines wether the data from the clipboard (which must be a list of files) contains the specified string.
        /// </summary>
        /// <param name="files">The list of files from the clipboard</param>
        /// <param name="queryRegex">The regex to search</param>
        /// <returns>True if it is contained</returns>
        private bool MatchFiles(ICollection<string> files, Regex queryRegex)
        {
            if (queryRegex != null)
            {
                return files.AsParallel().Any(file => queryRegex.IsMatch(file.ToLower()));
            }

            return false;
        }

        /// <summary>
        /// Gets the list of clipboard data format associated to a <see cref="SupportedDataType"/>
        /// </summary>
        /// <returns>A dictionary of <see cref="SupportedDataType"/></returns>
        private Dictionary<SupportedDataType, string[]> GetPotentialAssociatedDataTypes()
        {
            var result = new Dictionary<SupportedDataType, string[]>
            {
                { SupportedDataType.AdobePhotoshop, new[] { DataFormats.Dib, "Photoshop Paste in Place", "Photoshop Clip Source", "Object Descriptor" } },
                { SupportedDataType.MicrosoftOutlook, new[] { "RenPrivateSourceFolder", "RenPrivateMessages", "RenPrivateAppointment", "FileGroupDescriptor", "FileGroupDescriptorW", "Object Descriptor", "Text", DataFormats.UnicodeText, "Csv" } },
                { SupportedDataType.MicrosoftPowerPoint, new[] { "Preferred DropEffect", "InShellDragLoop", "Object Descriptor", "PowerPoint 12.0 Internal Theme", "PowerPoint 12.0 Internal Color Scheme", "PowerPoint 12.0 Internal Shapes", "PowerPoint 12.0 Internal Slides", "PowerPoint 14.0 Slides Package", "ActiveClipBoard", "Art::GVML", "PNG", "JFIF", "GIF", "Bitmap", "HTML Format", "Rich Text Format", DataFormats.UnicodeText } },
                { SupportedDataType.MicrosoftExcel, new[] { "Biff12", "Biff8", "Biff5", "SymbolicLink", "DataInterchangeFormat", "XML Spreadsheet", "HTML Format", DataFormats.UnicodeText, "Text", "Csv", "Hyperlink", "Rich Text Format", "Object Descriptor" } },
                { SupportedDataType.MicrosoftWord, new[] { "Object Descriptor", "Rich Text Format", "HTML Format", "Text", DataFormats.UnicodeText, "Link Source Descriptor", "ObjectLink" } },
                { SupportedDataType.Image, new[] { DataFormats.Dib } },
                { SupportedDataType.Files, new[] { "Shell IDList Array", "DataObjectAttributes", "DataObjectAttributesRequiringElevation", "Preferred DropEffect", "AsyncFlag", DataFormats.FileDrop, "FileName", "FileNameW", "FileGroupDescriptorW" } },
                { SupportedDataType.Text, new[] { "Text", DataFormats.UnicodeText, "HTML Format", "Rich Text Format", "Locale", "OEMText" } },
                { SupportedDataType.Unknow, new[] { string.Empty } }
            };
            return result;
        }

        /// <summary>
        /// Generate a new unique <see cref="Guid"/> not used by the service
        /// </summary>
        /// <returns>The new <see cref="Guid"/></returns>
        private Guid GenerateNewGuid()
        {
            Guid guid;
            var guidString = string.Empty;

            do
            {
                guid = Guid.NewGuid();
                guidString = guid.ToString();
            } while (DataEntries.Any(entry => entry.Identifier.ToString().Equals(guidString, StringComparison.OrdinalIgnoreCase) ||
                                               entry.DataIdentifiers.Any(dataIdentifier => dataIdentifier.Identifier.ToString().Equals(guidString, StringComparison.OrdinalIgnoreCase))));

            return guid;
        }

        #endregion
    }
}
