using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Core.Desktop.Enums;
using ClipboardZanager.Core.Desktop.IO;
using ClipboardZanager.Core.Desktop.Models;
using ClipboardZanager.Shared.CloudStorage;
using ClipboardZanager.Shared.Core;
using ClipboardZanager.Shared.Core.LINQ;
using ClipboardZanager.Shared.Logs;
using ClipboardZanager.Shared.Services;
using Newtonsoft.Json;

namespace ClipboardZanager.Core.Desktop.Services
{
    /// <summary>
    /// Provides a set of functions designed to work with the cloud storage providers.
    /// </summary>
    internal sealed class CloudStorageService : IService
    {
        #region Fields

        private DispatcherTimer _synchronizeTimer;
        private List<ICloudStorageProvider> _cloudStorageProviders;
        private IServiceSettingProvider _settingProvider;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a <see cref="bool"/> that defines if the software is connected to at least one cloud storage service.
        /// </summary>
        internal bool IsLinkedToAService => _cloudStorageProviders.Any(provider => provider.CredentialExists);

        /// <summary>
        /// Gets the <see cref="ICloudStorageProvider"/> that is linked to the application. If there is no, returns null.
        /// </summary>
        internal ICloudStorageProvider CurrentCloudStorageProvider => _cloudStorageProviders.FirstOrDefault(provider => provider.CredentialExists);

        /// <summary>
        /// Gets the list of providers to display in the UI.
        /// </summary>
        internal IReadOnlyList<CloudStorageProvider> CloudStorageProviders { get; private set; }

        /// <summary>
        /// Gets a value that defines wether the synchronization is in progress or not.
        /// </summary>
        internal bool IsSynchronizing { get; private set; }

        #endregion

        #region Events

        /// <summary>
        /// Raised when the synchronization has started.
        /// </summary>
        internal event EventHandler<EventArgs> SynchronizationStarted;

        /// <summary>
        /// Raised when the synchronization has failed.
        /// </summary>
        internal event EventHandler<EventArgs> SynchronizationFailed;

        /// <summary>
        /// Raised when the synchronization has ended.
        /// </summary>
        internal event EventHandler<EventArgs> SynchronizationEnded;

        #endregion

        #region Handled Methods

        private void SynchronizeTimer_Tick(object sender, EventArgs e)
        {
            if (_settingProvider.GetSetting<bool>("AvoidMeteredConnection") && SystemInfoHelper.IsMeteredConnection())
            {
                return;
            }

            SynchronizeAsync();
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public async void Initialize(IServiceSettingProvider settingProvider)
        {
            _settingProvider = settingProvider;

            var synchronizeInterval = _settingProvider.GetSetting<int>("SynchronizationInterval");
            var providers = _settingProvider.GetSetting<List<ICloudStorageProvider>>("CloudStorageProviders");
            var isConnectedToInternet = SystemInfoHelper.CheckForInternetConnection();
            var cloudStorageProviders = new List<CloudStorageProvider>();

            _synchronizeTimer = new DispatcherTimer();
            _synchronizeTimer.Interval = TimeSpan.FromMinutes(synchronizeInterval);
            _synchronizeTimer.Tick += SynchronizeTimer_Tick;

            _cloudStorageProviders = new List<ICloudStorageProvider>();

            foreach (var provider in providers)
            {
                if (_cloudStorageProviders.Any(p => string.Compare(p.CloudServiceName, provider.CloudServiceName, StringComparison.OrdinalIgnoreCase) == 0))
                {
                    Logger.Instance.Fatal(new Exception($"The cloud storage provider '{provider.CloudServiceName}' already exists."));
                }

                if (isConnectedToInternet)
                {
                    await provider.TryAuthenticateAsync();
                }

                _cloudStorageProviders.Add(provider);
                cloudStorageProviders.Add(new CloudStorageProvider(provider.CloudServiceName, provider.CloudServiceIcon));
            }

            CloudStorageProviders = cloudStorageProviders.OrderBy(s => s.Name).ToList();

            if (isConnectedToInternet && CurrentCloudStorageProvider != null)
            {
                await CurrentCloudStorageProvider.TryAuthenticateAsync();
            }

            _synchronizeTimer.Start();
            Logger.Instance.Information($"{GetType().Name} initialized.");
        }

        /// <inheritdoc/>
        public void Reset()
        {
            if (CoreHelper.IsUnitTesting())
            {
                _cloudStorageProviders.Clear();
                Initialize(_settingProvider);
            }

            var synchronizeInterval = _settingProvider.GetSetting<int>("SynchronizationInterval");
            _synchronizeTimer.Stop();
            _synchronizeTimer.Interval = TimeSpan.FromMinutes(synchronizeInterval);
            _synchronizeTimer.Start();
        }

        /// <summary>
        /// Sign out in all providers
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        internal async Task SignOutAllAsync()
        {
            foreach (var cloudStorageProvider in _cloudStorageProviders)
            {
                try
                {
                    await cloudStorageProvider.SignOutAsync();
                    Logger.Instance.Information($"The {nameof(CloudStorageService)} has signed out from {cloudStorageProvider.CloudServiceName}.");
                }
                catch { }
            }
        }

        /// <summary>
        /// Retrieve the specified <see cref="ICloudStorageProvider"/>
        /// </summary>
        /// <param name="name">The name of the provider</param>
        /// <returns>Returns the <see cref="ICloudStorageProvider"/></returns>
        internal ICloudStorageProvider GetProviderFromName(string name)
        {
            Requires.NotNullOrWhiteSpace(name, nameof(name));
            return _cloudStorageProviders.Single(provider => string.Compare(provider.CloudServiceName, name, StringComparison.OrdinalIgnoreCase) == 0);
        }

        /// <summary>
        /// Try to authenticate to the current cloud storage provider
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        internal async Task TryAuthenticateAsync()
        {
            var isConnectedToInternet = SystemInfoHelper.CheckForInternetConnection();
            if (isConnectedToInternet && CurrentCloudStorageProvider != null)
            {
                await CurrentCloudStorageProvider.TryAuthenticateAsync();
            }
        }

        /// <summary>
        /// Force to synchronize that data with the cloud.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        internal async Task SynchronizeAsync()
        {
            if (IsSynchronizing)
            {
                return;
            }

            Logger.Instance.Information("Synchronization with the cloud started.");
            IsSynchronizing = true;

            if (CurrentCloudStorageProvider == null)
            {
                Logger.Instance.Warning("The user is not logged to any cloud storage provider. The synchronization stopped.");
                IsSynchronizing = false;
                return;
            }

            if (!CoreHelper.IsUnitTesting() && !SystemInfoHelper.CheckForInternetConnection())
            {
                Logger.Instance.Warning("There is no internet connection. The synchronization stopped.");
                IsSynchronizing = false;
                return;
            }

            SynchronizationStarted?.Invoke(this, new EventArgs());

            try
            {
                if (!await CurrentCloudStorageProvider.TryAuthenticateAsync())
                {
                    Logger.Instance.Warning("The user is not authenticated correctly. Consider unlink the app and connect again. The synchronization stopped.");
                    IsSynchronizing = false;
                    SynchronizationEnded?.Invoke(this, new EventArgs());
                    return;
                }

                var userId = SecurityHelper.ToSecureString(await CurrentCloudStorageProvider.GetUserIdAsync());

                if (string.IsNullOrWhiteSpace(SecurityHelper.ToUnsecureString(userId)))
                {
                    Logger.Instance.Warning("The user's id from the cloud storage provider has not been found. The synchronization stopped.");
                    IsSynchronizing = false;
                    SynchronizationEnded?.Invoke(this, new EventArgs());
                    SynchronizationFailed?.Invoke(this, new EventArgs());
                    return;
                }

                Logger.Instance.Information("Freezing the data before synchronize.");
                var dataService = ServiceLocator.GetService<DataService>();
                var cloudDataEntryFromServer = new List<CloudDataEntry>();
                var cloudAppFolder = await CurrentCloudStorageProvider.GetAppFolderAsync();
                var cloudDataEntryFilePath = Path.Combine(cloudAppFolder.FullPath, Consts.DataEntryFileName);
                var cloudDataPassword = SecurityHelper.ToSecureString(SecurityHelper.EncryptString(userId, SecurityHelper.ToSecureString(await CurrentCloudStorageProvider.GetUserNameAsync())));
                var localFrozenDataEntries = DataHelper.FromByteArray<AsyncObservableCollection<DataEntry>>(DataHelper.ToByteArray(dataService.DataEntries));
                var localFrozenCache = DataHelper.FromByteArray<List<DataEntryCache>>(DataHelper.ToByteArray(dataService.Cache));

                // Download data from server.
                if (cloudAppFolder.Files.Any(file => file.FullPath == cloudDataEntryFilePath))
                {
                    Logger.Instance.Information("Downloading the data entry file from the server.");
                    try
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await CurrentCloudStorageProvider.DownloadFileAsync(cloudDataEntryFilePath, memoryStream);
                            memoryStream.Position = 0;
                            using (var aesStream = new AesStream(memoryStream, cloudDataPassword, SecurityHelper.GetSaltKeys(cloudDataPassword).GetBytes(16)))
                            {
                                var data = new byte[aesStream.Length];
                                aesStream.Read(data, 0, data.Length);
                                cloudDataEntryFromServer = JsonConvert.DeserializeObject<List<CloudDataEntry>>(Encoding.UTF8.GetString(data));
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        Logger.Instance.Warning($"Unable to download or read the data file entry from the cloud for the following reason : {exception.Message}");
                        IsSynchronizing = false;
                        SynchronizationEnded?.Invoke(this, new EventArgs());
                        SynchronizationFailed?.Invoke(this, new EventArgs());
                        return;
                    }
                }
                else
                {
                    Logger.Instance.Information("There is no data entry file on the server yet.");
                }

                // Synchronize locally the data. The result must corresponds to what we will have locally and on the server at the end of the synchronization process.
                var cloudDataEntryToServer = dataService.DifferenceLocalAndCloudDataEntries(cloudDataEntryFromServer);

                // Download the needed data from the server to the local machine.
                Logger.Instance.Information("Downloading the needed data from the server to the local machine.");
                var dataToDownload = cloudDataEntryFromServer.Cast<DataEntryBase>().Except(localFrozenDataEntries, (item1, item2) => item1.Identifier == item2.Identifier).ToList();
                var taskList = new List<Task>();
                foreach (var cloudDataEntry in dataToDownload)
                {
                    if (dataToDownload.Any(data => localFrozenCache.Any(item => data.Identifier == item.Identifier && item.Status == DataEntryStatus.Deleted)))
                    {
                        continue;
                    }

                    foreach (var dataEntryDataIdentifier in cloudDataEntry.DataIdentifiers)
                    {
                        var task = DownloadDataFileAsync(dataService.ClipboardDataPath, cloudAppFolder, cloudDataPassword, dataEntryDataIdentifier);
                        taskList.Add(task);
                    }
                }
                await Task.WhenAll(taskList);

                // Delete the needed data from the server
                Logger.Instance.Information("Deleting the needed data from the server.");
                taskList = new List<Task>();
                foreach (var dataServiceDataEntry in localFrozenDataEntries.Where(item => !item.CanSynchronize))
                {
                    foreach (var dataEntryDataIdentifier in dataServiceDataEntry.DataIdentifiers)
                    {
                        var task = DeleteFileAsync(cloudAppFolder, dataEntryDataIdentifier);
                        taskList.Add(task);
                    }
                }
                await Task.WhenAll(taskList);

                taskList = new List<Task>();
                foreach (var cacheEntry in localFrozenCache.Where(item => item.Status == DataEntryStatus.Deleted))
                {
                    var dataEntry = cloudDataEntryFromServer.SingleOrDefault(item => item.Identifier == cacheEntry.Identifier);
                    if (dataEntry != null)
                    {
                        foreach (var dataEntryDataIdentifier in dataEntry.DataIdentifiers)
                        {
                            var task = DeleteFileAsync(cloudAppFolder, dataEntryDataIdentifier);
                            taskList.Add(task);
                        }

                    }
                }
                await Task.WhenAll(taskList);

                await dataService.MakeCacheSynchronized(cloudDataEntryToServer, true, localFrozenCache);
                localFrozenDataEntries = DataHelper.FromByteArray<AsyncObservableCollection<DataEntry>>(DataHelper.ToByteArray(dataService.DataEntries));
                localFrozenCache = DataHelper.FromByteArray<List<DataEntryCache>>(DataHelper.ToByteArray(dataService.Cache));

                // Upload the needed data from the server to the local machine
                Logger.Instance.Information("Uploading the needed data from the server to the local machine.");
                var dataToUpload = localFrozenDataEntries.Cast<DataEntryBase>().Except(cloudDataEntryFromServer, (item1, item2) => item1.Identifier == item2.Identifier);
                taskList = new List<Task>();
                foreach (var dataEntry in dataToUpload)
                {
                    var localDataEntry = localFrozenDataEntries.Single(item => item.Identifier == dataEntry.Identifier);
                    if (!localDataEntry.CanSynchronize || localDataEntry.Thumbnail.Type == ThumbnailDataType.Files)
                    {
                        continue;
                    }

                    foreach (var dataEntryDataIdentifier in dataEntry.DataIdentifiers)
                    {
                        var task = UploadDataFileAsync(dataService.ClipboardDataPath, cloudAppFolder, cloudDataPassword, dataEntryDataIdentifier);
                        taskList.Add(task);
                    }
                }
                await Task.WhenAll(taskList);

                // Upload the new data to the server.
                Logger.Instance.Information("Uploading the data entry file to the server.");

                using (var memoryStream = new MemoryStream())
                using (var aesStream = new AesStream(memoryStream, cloudDataPassword, SecurityHelper.GetSaltKeys(cloudDataPassword).GetBytes(16)))
                {
                    var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(cloudDataEntryToServer));
                    aesStream.Write(data, 0, data.Length);
                    aesStream.Position = 0;
                    await CurrentCloudStorageProvider.UploadFileAsync(memoryStream, cloudDataEntryFilePath);
                }

                await dataService.MakeCacheSynchronized(cloudDataEntryToServer, false, localFrozenCache);
            }
            catch (Exception exception)
            {
                Logger.Instance.Warning($"Unable to synchronize for the following reason : {exception.Message}. {exception.InnerException?.Message}");
                SynchronizationFailed?.Invoke(this, new EventArgs());
            }

            _synchronizeTimer.Stop();
            _synchronizeTimer.Start();

            IsSynchronizing = false;
            SynchronizationEnded?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Download the specified data file from the cloud storage service.
        /// </summary>
        /// <param name="clipboardDataPath">The full path to the clipboard data folder.</param>
        /// <param name="cloudFolder">The <see cref="CloudFolder"/> that contains informations about the application folder.</param>
        /// <param name="cloudDataPassword">The supposed password that we will use to decrypt the data from the cloud.</param>
        /// <param name="dataIdentifier">The data identifier used to determines which file will be downloaded.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task DownloadDataFileAsync(string clipboardDataPath, CloudFolder cloudFolder, SecureString cloudDataPassword, DataIdentifier dataIdentifier)
        {
            var dataService = ServiceLocator.GetService<DataService>();
            var fileName = $"{dataIdentifier.Identifier}.dat";
            var cloudDataFilePath = Path.Combine(cloudFolder.FullPath, fileName);
            var localDataFilePath = Path.Combine(clipboardDataPath, fileName);
            var temporaryLocalDataFilePath = Path.Combine(clipboardDataPath, $"{fileName}.temp");
            var localDataPassword = SecurityHelper.ToSecureString(SecurityHelper.EncryptString(SecurityHelper.ToSecureString(dataIdentifier.Identifier.ToString())));

            Requires.NotNull(localDataPassword, nameof(localDataPassword));

            if (cloudFolder.Files.All(file => file.Name != fileName))
            {
                Logger.Instance.Warning($"Data file {dataIdentifier.Identifier} not found on the server, it has probably been removed while synchronizing.");
                return;
            }

            using (var localFileStream = File.OpenWrite(localDataFilePath))
            using (var temporaryLocalFileStream = File.Open(temporaryLocalDataFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                await CurrentCloudStorageProvider.DownloadFileAsync(cloudDataFilePath, temporaryLocalFileStream);
                temporaryLocalFileStream.Position = 0;
                using (var cloudAesStream = new AesStream(temporaryLocalFileStream, localDataPassword, SecurityHelper.GetSaltKeys(localDataPassword).GetBytes(16)))
                using (var localAesStream = new AesStream(localFileStream, cloudDataPassword, SecurityHelper.GetSaltKeys(cloudDataPassword).GetBytes(16)))
                {
                    cloudAesStream.Position = 0;
                    dataService.CopyData(cloudAesStream, localAesStream);
                }
            }

            if (File.Exists(temporaryLocalDataFilePath))
            {
                File.Delete(temporaryLocalDataFilePath);
            }
        }

        /// <summary>
        /// Upload the specified data file to the cloud storage service.
        /// </summary>
        /// <param name="clipboardDataPath">The full path to the clipboard data folder.</param>
        /// <param name="cloudFolder">The <see cref="CloudFolder"/> that contains informations about the application folder.</param>
        /// <param name="cloudDataPassword">The supposed password that we will use to encrypt the data before uploading it to the cloud.</param>
        /// <param name="dataIdentifier">The data identifier used to determines which file will be uploaded.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task UploadDataFileAsync(string clipboardDataPath, CloudFolder cloudFolder, SecureString cloudDataPassword, DataIdentifier dataIdentifier)
        {
            var dataService = ServiceLocator.GetService<DataService>();
            var fileName = $"{dataIdentifier.Identifier}.dat";
            var cloudDataFilePath = Path.Combine(cloudFolder.FullPath, fileName);
            var localDataFilePath = Path.Combine(clipboardDataPath, fileName);
            var temporaryLocalDataFilePath = Path.Combine(clipboardDataPath, $"{fileName}.temp");
            var localDataPassword = SecurityHelper.ToSecureString(SecurityHelper.EncryptString(SecurityHelper.ToSecureString(dataIdentifier.Identifier.ToString())));

            Requires.NotNull(localDataPassword, nameof(localDataPassword));

            if (!File.Exists(localDataFilePath))
            {
                Logger.Instance.Warning($"Data file {dataIdentifier.Identifier} not found locally, it has probably been removed while synchronizing.");
                return;
            }

            using (var fileStream = File.OpenRead(localDataFilePath))
            using (var temporaryLocalFileStream = File.Open(temporaryLocalDataFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            using (var localAesStream = new AesStream(fileStream, localDataPassword, SecurityHelper.GetSaltKeys(localDataPassword).GetBytes(16)))
            using (var cloudAesStream = new AesStream(temporaryLocalFileStream, cloudDataPassword, SecurityHelper.GetSaltKeys(cloudDataPassword).GetBytes(16)))
            {
                dataService.CopyData(localAesStream, cloudAesStream);
                await CurrentCloudStorageProvider.UploadFileAsync(temporaryLocalFileStream, cloudDataFilePath);
            }

            if (File.Exists(temporaryLocalDataFilePath))
            {
                File.Delete(temporaryLocalDataFilePath);
            }
        }

        /// <summary>
        /// Delete a data from the server
        /// </summary>
        /// <param name="cloudFolder">The <see cref="CloudFolder"/> that contains informations about the application folder.</param>
        /// <param name="dataIdentifier">The data identifier used to determines which file will be uploaded.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task DeleteFileAsync(CloudFolder cloudFolder, DataIdentifier dataIdentifier)
        {
            var fileName = $"{dataIdentifier.Identifier}.dat";
            var cloudDataFilePath = Path.Combine(cloudFolder.FullPath, fileName);

            if (cloudFolder.Files.Any(file => file.FullPath == cloudDataFilePath))
            {
                await CurrentCloudStorageProvider.DeleteFileAsync(cloudDataFilePath);
            }
        }

        #endregion
    }
}
