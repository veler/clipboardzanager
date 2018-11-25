using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Core.Desktop.Enums;
using ClipboardZanager.Core.Desktop.Events;
using ClipboardZanager.Core.Desktop.IO;
using ClipboardZanager.Core.Desktop.Models;
using ClipboardZanager.Core.Desktop.Services;
using ClipboardZanager.Core.Desktop.Tests.Mocks;
using ClipboardZanager.Shared.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace ClipboardZanager.Core.Desktop.Tests.Service
{
    [TestClass]
    public class CloudStorageServiceTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            TestUtilities.Initialize();
            GetDataService().RemoveAllDataAsync().Wait();
            GetDataService().Cache.Clear();
            GetCloudStorageService().SignOutAllAsync().Wait();
            GetCloudStorageService().Reset();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (Directory.Exists(GetDataService().ClipboardDataPath))
            {
                Directory.Delete(GetDataService().ClipboardDataPath, true);
            }

            if (Directory.Exists(GetCloudStorageProviderMock().TemporaryFolder))
            {
                Directory.Delete(GetCloudStorageProviderMock().TemporaryFolder, true);
            }
        }

        [TestMethod]
        public async Task CloudStorageService()
        {
            var service = GetCloudStorageService();
            var providers = service.CloudStorageProviders;

            await service.SignOutAllAsync();

            Assert.AreEqual(providers.Count, 2);
            Assert.AreEqual(providers[0].Name, "CloudStorageProviderMock");
            Assert.AreEqual(providers[1].Name, "MockCloudStorageProvider");

            Assert.AreEqual(service.CurrentCloudStorageProvider.CloudServiceName, "CloudStorageProviderMock");

            Assert.IsTrue(service.IsLinkedToAService);
        }

        [TestMethod]
        public async Task CloudStorageService_Synchronize_DataAddedStatus()
        {
            /*
             Scenario :
             We add two data entries to the local data. There is actually nothing on the server.
             The two data "entries" must be uploaded. There will be no "real" data on the server, only the entries.
             */

            var dataService = GetDataService();
            var cloudStorageService = GetCloudStorageService();

            var date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

            var dataObject = new DataObject();
            dataObject.SetText("Hello");
            var entry = new ClipboardHookEventArgs(dataObject, false, date.Ticks);
            dataService.AddDataEntry(entry, new List<DataIdentifier>(), GetWindowsService().GetForegroundWindow(), false);

            dataObject = new DataObject();
            dataObject.SetText("Good Bye");
            entry = new ClipboardHookEventArgs(dataObject, false, date.Ticks);
            dataService.AddDataEntry(entry, new List<DataIdentifier>(), GetWindowsService().GetForegroundWindow(), false);

            Assert.IsTrue(dataService.Cache.All(cacheDataEntry => cacheDataEntry.Status == DataEntryStatus.Added));

            await cloudStorageService.SynchronizeAsync();

            var cloudDataEntries = await GetDataEntriesFromServerAsync();

            Assert.AreEqual(2, cloudDataEntries.Count);
            Assert.AreEqual(2, dataService.Cache.Count);
            Assert.IsTrue(cloudDataEntries.All(cloudDataEntry => cloudDataEntry.Date == date));
            Assert.IsTrue(dataService.Cache.All(cacheDataEntry => cacheDataEntry.Status == DataEntryStatus.DidNotChanged));
        }

        [TestMethod]
        public async Task CloudStorageService_Synchronize_DataAddedAndDeletedBeforeSync()
        {
            /*
             Scenario :
             We add two data entries to the local data. There is actually nothing on the server.
             One of the two data will be deleted locally.
             It must upload only one data.
             There will be no "real" data on the server, only the entries.
             */

            var dataService = GetDataService();
            var cloudStorageService = GetCloudStorageService();

            var dataObject = new DataObject();
            dataObject.SetText("Hello");
            var entry = new ClipboardHookEventArgs(dataObject, false, DateTime.Now.Ticks);
            dataService.AddDataEntry(entry, new List<DataIdentifier>(), GetWindowsService().GetForegroundWindow(), false);

            dataObject = new DataObject();
            dataObject.SetText("Good Bye");
            entry = new ClipboardHookEventArgs(dataObject, false, DateTime.Now.Ticks);
            dataService.AddDataEntry(entry, new List<DataIdentifier>(), GetWindowsService().GetForegroundWindow(), false);

            Assert.IsTrue(dataService.Cache.All(cacheDataEntry => cacheDataEntry.Status == DataEntryStatus.Added));

            await dataService.RemoveDataAsync(dataService.DataEntries.Last().Identifier, dataService.DataEntries.Last().DataIdentifiers);

            Assert.AreEqual(2, dataService.Cache.Count);
            Assert.AreEqual(DataEntryStatus.Added, dataService.Cache.First().Status);
            Assert.AreEqual(DataEntryStatus.Deleted, dataService.Cache.Last().Status);

            await cloudStorageService.SynchronizeAsync();

            var cloudDataEntries = await GetDataEntriesFromServerAsync();

            Assert.AreEqual(1, cloudDataEntries.Count);
            Assert.AreEqual(1, dataService.Cache.Count);
            Assert.IsTrue(dataService.Cache.All(cacheDataEntry => cacheDataEntry.Status == DataEntryStatus.DidNotChanged));
        }

        [TestMethod]
        public async Task CloudStorageService_Synchronize_DataAddedButExpired()
        {
            /*
             Scenario :
             We add two data entries to the local data. There is actually nothing on the server.
             The two data entries are expired (date limit) and must be deleted, and not uploaded. There will be no "real" data on the server, only the entries.
             */

            var dataService = GetDataService();
            var cloudStorageService = GetCloudStorageService();

            var date = new DateTime(2017, 1, 1);

            var dataObject = new DataObject();
            dataObject.SetText("Hello");
            var entry = new ClipboardHookEventArgs(dataObject, false, date.Ticks);
            dataService.AddDataEntry(entry, new List<DataIdentifier>(), GetWindowsService().GetForegroundWindow(), false);

            dataObject = new DataObject();
            dataObject.SetText("Good Bye");
            entry = new ClipboardHookEventArgs(dataObject, false, date.Ticks);
            dataService.AddDataEntry(entry, new List<DataIdentifier>(), GetWindowsService().GetForegroundWindow(), false);

            Assert.IsTrue(dataService.Cache.All(cacheDataEntry => cacheDataEntry.Status == DataEntryStatus.Deleted));

            await cloudStorageService.SynchronizeAsync();

            var cloudDataEntries = await GetDataEntriesFromServerAsync();

            Assert.AreEqual(0, cloudDataEntries.Count);
            Assert.AreEqual(0, dataService.Cache.Count);
        }

        [TestMethod]
        public async Task CloudStorageService_Synchronize_DataAdded()
        {
            /*
             Scenario :
             We add one real data to the local data and set it to a favorite. There is actually nothing on the server.
             The data must be uploaded. There will be one real data on the server.
             */

            var dataService = GetDataService();
            var cloudStorageService = GetCloudStorageService();
            var cloudStorageProviderMock = GetCloudStorageProviderMock();
            var service = GetClipboardService();

            var dataObject = new DataObject();
            dataObject.SetText("Hello", TextDataFormat.UnicodeText);
            dataObject.SetText("Hello", TextDataFormat.Text);
            var entry1 = new ClipboardHookEventArgs(dataObject, false, DateTime.Now.Ticks);

            DispatcherUtil.ExecuteOnDispatcherThread(() => service.ClipboardHook_ClipboardChanged(null, entry1), 100);
            await Task.Delay(300);
            DispatcherUtil.DoEvents();

            dataService.DataEntries.First().IsFavorite = true;
            await dataService.ReorganizeAsync(true);

            Assert.AreEqual(1, dataService.Cache.Count);
            Assert.AreEqual(1, dataService.DataEntries.Count);
            Assert.IsTrue(dataService.DataEntries.First().IsFavorite);
            Assert.IsTrue(dataService.Cache.All(cacheDataEntry => cacheDataEntry.Status == DataEntryStatus.Added));

            await cloudStorageService.SynchronizeAsync();

            var cloudDataEntries = await GetDataEntriesFromServerAsync();

            Assert.AreEqual(1, cloudDataEntries.Count);
            Assert.IsTrue(cloudDataEntries.First().IsFavorite);

            var expectedFileDataPathOnTheServer = cloudStorageProviderMock.TemporaryFolder + "/" + cloudDataEntries.First().DataIdentifiers.Single(item => item.FormatName == "Text").Identifier + ".dat";
            Assert.IsTrue(File.Exists(expectedFileDataPathOnTheServer));

            var valueByte = (await ReadFileFromServerAsync(expectedFileDataPathOnTheServer)).ToArray();
            var value = Encoding.ASCII.GetString(valueByte);
            Assert.AreEqual("Hello\0", value);
        }

        [TestMethod]
        public async Task CloudStorageService_Synchronize_DataFilesAdded()
        {
            /*
             Scenario :
             We add one real data to the local data which is a file. There is actually nothing on the server.
             The data must NOT be uploaded.
             */

            var dataService = GetDataService();
            var cloudStorageService = GetCloudStorageService();
            var cloudStorageProviderMock = GetCloudStorageProviderMock();
            var service = GetClipboardService();

            var dataObject = new DataObject();
            dataObject.SetFileDropList(new StringCollection { @"C:\Windows\explorer.exe" });
            var entry1 = new ClipboardHookEventArgs(dataObject, false, DateTime.Now.Ticks);

            DispatcherUtil.ExecuteOnDispatcherThread(() => service.ClipboardHook_ClipboardChanged(null, entry1), 100);
            await Task.Delay(300);
            DispatcherUtil.DoEvents();

            dataService.DataEntries.First().IsFavorite = true;
            await dataService.ReorganizeAsync(true);

            Assert.AreEqual(1, dataService.Cache.Count);
            Assert.AreEqual(1, dataService.DataEntries.Count);
            Assert.AreEqual(ThumbnailDataType.Files, dataService.DataEntries.First().Thumbnail.Type);
            Assert.IsTrue(dataService.Cache.All(cacheDataEntry => cacheDataEntry.Status == DataEntryStatus.Added));

            await cloudStorageService.SynchronizeAsync();

            var cloudDataEntries = await GetDataEntriesFromServerAsync();

            Assert.AreEqual(0, cloudDataEntries.Count);
            var filesOnServer = Directory.GetFiles(cloudStorageProviderMock.TemporaryFolder);
            Assert.AreEqual(1, filesOnServer.Length);
            Assert.IsTrue(filesOnServer.First().EndsWith(".clipboard"));
        }

        [TestMethod]
        public async Task CloudStorageService_Synchronize_NoDataPresent()
        {
            /*
             Scenario :
             There is actually nothing on the server and locally but we try to synchronize.
             */

            var dataService = GetDataService();
            var cloudStorageService = GetCloudStorageService();

            Assert.AreEqual(0, dataService.Cache.Count);
            Assert.AreEqual(0, dataService.DataEntries.Count);

            await cloudStorageService.SynchronizeAsync();

            var cloudDataEntries = await GetDataEntriesFromServerAsync();

            Assert.AreEqual(0, cloudDataEntries.Count);
        }

        [TestMethod]
        public async Task CloudStorageService_Synchronize_DataPresentOnServerButNotLocally()
        {
            /*
             Scenario :
             There is a data on the server but nothing locally.
             The data must be downloaded.
             */

            var fakeDataFolder = await CreateFakeContentOnServerAsync();

            DispatcherUtil.ExecuteOnDispatcherThread(() =>
            {
                TestCleanup();
                TestUtilities.Initialize();
                GetDataService().RemoveAllDataAsync().Wait();
                GetDataService().Cache.Clear();
                GetCloudStorageService().Reset();
            }, 100);

            var dataService = GetDataService();
            var cloudStorageService = GetCloudStorageService();
            var cloudStorageProviderMock = GetCloudStorageProviderMock();

            // Create all of the directories
            foreach (var dirPath in Directory.GetDirectories(fakeDataFolder, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(fakeDataFolder, cloudStorageProviderMock.TemporaryFolder));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (var newPath in Directory.GetFiles(fakeDataFolder, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(fakeDataFolder, cloudStorageProviderMock.TemporaryFolder), true);
            }

            Directory.Delete(fakeDataFolder, true);

            Assert.AreEqual(0, dataService.Cache.Count);
            Assert.AreEqual(0, dataService.DataEntries.Count);

            await cloudStorageService.SynchronizeAsync();

            var cloudDataEntries = await GetDataEntriesFromServerAsync();

            Assert.AreEqual(1, cloudDataEntries.Count);

            Assert.AreEqual(1, dataService.Cache.Count);
            Assert.AreEqual(1, dataService.DataEntries.Count);
            Assert.IsTrue(dataService.Cache.All(cacheDataEntry => cacheDataEntry.Status == DataEntryStatus.DidNotChanged));
        }

        [TestMethod]
        public async Task CloudStorageService_Synchronize_DeletedLocallyAndRemotely()
        {
            /*
             Scenario :
             There is a data on the server and the same data locally.
             We delete it from local. It must be deleted on the server too.
             */

            var dataService = GetDataService();
            var cloudStorageService = GetCloudStorageService();
            var cloudStorageProviderMock = GetCloudStorageProviderMock();
            var service = GetClipboardService();

            var dataObject = new DataObject();
            dataObject.SetText("Hello", TextDataFormat.UnicodeText);
            dataObject.SetText("Hello", TextDataFormat.Text);
            var entry1 = new ClipboardHookEventArgs(dataObject, false, DateTime.Now.Ticks);

            DispatcherUtil.ExecuteOnDispatcherThread(() => service.ClipboardHook_ClipboardChanged(null, entry1), 100);
            await Task.Delay(300);
            DispatcherUtil.DoEvents();

            Assert.AreEqual(1, dataService.Cache.Count);
            Assert.AreEqual(1, dataService.DataEntries.Count);
            Assert.IsTrue(dataService.Cache.All(cacheDataEntry => cacheDataEntry.Status == DataEntryStatus.Added));

            await cloudStorageService.SynchronizeAsync();

            var cloudDataEntries = await GetDataEntriesFromServerAsync();

            Assert.AreEqual(1, cloudDataEntries.Count);
            Assert.IsTrue(dataService.Cache.All(cacheDataEntry => cacheDataEntry.Status == DataEntryStatus.DidNotChanged));

            var filesOnServer = Directory.GetFiles(cloudStorageProviderMock.TemporaryFolder);
            Assert.AreEqual(3, filesOnServer.Length);

            await dataService.RemoveDataAsync(dataService.DataEntries.First().Identifier, dataService.DataEntries.First().DataIdentifiers);

            Assert.AreEqual(1, dataService.Cache.Count);
            Assert.AreEqual(0, dataService.DataEntries.Count);
            Assert.IsTrue(dataService.Cache.All(cacheDataEntry => cacheDataEntry.Status == DataEntryStatus.Deleted));

            await cloudStorageService.SynchronizeAsync();

            cloudDataEntries = await GetDataEntriesFromServerAsync();

            Assert.AreEqual(0, cloudDataEntries.Count);
            Assert.AreEqual(0, dataService.Cache.Count);

            filesOnServer = Directory.GetFiles(cloudStorageProviderMock.TemporaryFolder);
            Assert.AreEqual(1, filesOnServer.Length);
        }

        [TestMethod]
        public async Task CloudStorageService_Synchronize_DeletedRemotelyButNotLocally()
        {
            /*
             Scenario :
             There is a data locally that is marked as "already been synchronized", so it has been at least one time on the server.
             When we synchronize, we discover that the data is not present on the server, so it has probably been deleted by another device.
             The data must be deleted locally.
             */

            var dataService = GetDataService();
            var cloudStorageService = GetCloudStorageService();
            var cloudStorageProviderMock = GetCloudStorageProviderMock();
            var service = GetClipboardService();

            var dataObject = new DataObject();
            dataObject.SetText("Hello", TextDataFormat.UnicodeText);
            dataObject.SetText("Hello", TextDataFormat.Text);
            var entry1 = new ClipboardHookEventArgs(dataObject, false, DateTime.Now.Ticks);

            DispatcherUtil.ExecuteOnDispatcherThread(() => service.ClipboardHook_ClipboardChanged(null, entry1), 100);
            await Task.Delay(300);
            DispatcherUtil.DoEvents();

            await cloudStorageService.SynchronizeAsync();

            Assert.AreEqual(1, dataService.DataEntries.Count);
            Assert.AreEqual(1, dataService.Cache.Count);
            Assert.AreEqual(DataEntryStatus.DidNotChanged, dataService.Cache.First().Status);

            // remove the data from the server.
            File.Delete(cloudStorageProviderMock.TemporaryFolder + @"\.clipboard");

            await cloudStorageService.SynchronizeAsync();

            var cloudDataEntries = await GetDataEntriesFromServerAsync();

            Assert.AreEqual(0, cloudDataEntries.Count);
            Assert.AreEqual(0, dataService.DataEntries.Count);
            Assert.AreEqual(0, dataService.Cache.Count);
        }

        [TestMethod]
        public async Task CloudStorageService_Synchronize_CannotSynchronize()
        {
            /*
             Scenario :
             We add one real data to the local data and set it to "cannot synchronize". There is actually nothing on the server.
             The data must NOT be uploaded.
             */

            var dataService = GetDataService();
            var cloudStorageService = GetCloudStorageService();
            var cloudStorageProviderMock = GetCloudStorageProviderMock();
            var service = GetClipboardService();

            var dataObject = new DataObject();
            dataObject.SetText("Hello", TextDataFormat.UnicodeText);
            dataObject.SetText("Hello", TextDataFormat.Text);
            var entry1 = new ClipboardHookEventArgs(dataObject, false, DateTime.Now.Ticks);

            DispatcherUtil.ExecuteOnDispatcherThread(() => service.ClipboardHook_ClipboardChanged(null, entry1), 100);
            await Task.Delay(300);
            DispatcherUtil.DoEvents();

            dataService.DataEntries.First().CanSynchronize = false;

            Assert.AreEqual(1, dataService.Cache.Count);
            Assert.AreEqual(1, dataService.DataEntries.Count);
            Assert.IsFalse(dataService.DataEntries.First().CanSynchronize);
            Assert.IsTrue(dataService.Cache.All(cacheDataEntry => cacheDataEntry.Status == DataEntryStatus.Added));

            await cloudStorageService.SynchronizeAsync();

            var cloudDataEntries = await GetDataEntriesFromServerAsync();

            Assert.AreEqual(0, cloudDataEntries.Count);
            Assert.AreEqual(1, dataService.DataEntries.Count);
            Assert.AreEqual(1, dataService.Cache.Count);
            Assert.AreEqual(DataEntryStatus.DidNotChanged, dataService.Cache.First().Status);

            var filesOnServer = Directory.GetFiles(cloudStorageProviderMock.TemporaryFolder);
            Assert.AreEqual(1, filesOnServer.Length);
            Assert.IsTrue(filesOnServer.First().EndsWith(".clipboard"));
        }

        [TestMethod]
        public async Task CloudStorageService_Synchronize_SynchronizedThenCannotSynchronize()
        {
            /*
             Scenario :
             We add one real data to the local data and we synchronize. Then we set this data to "cannot synchronize".
             The data must be removed from the server.
             */

            var dataService = GetDataService();
            var cloudStorageService = GetCloudStorageService();
            var cloudStorageProviderMock = GetCloudStorageProviderMock();
            var service = GetClipboardService();

            var dataObject = new DataObject();
            dataObject.SetText("Hello", TextDataFormat.UnicodeText);
            dataObject.SetText("Hello", TextDataFormat.Text);
            var entry1 = new ClipboardHookEventArgs(dataObject, false, DateTime.Now.Ticks);

            DispatcherUtil.ExecuteOnDispatcherThread(() => service.ClipboardHook_ClipboardChanged(null, entry1), 100);
            await Task.Delay(300);
            DispatcherUtil.DoEvents();

            await cloudStorageService.SynchronizeAsync();

            var cloudDataEntries = await GetDataEntriesFromServerAsync();

            Assert.AreEqual(1, cloudDataEntries.Count);
            Assert.AreEqual(1, dataService.DataEntries.Count);
            Assert.AreEqual(1, dataService.Cache.Count);
            Assert.AreEqual(DataEntryStatus.DidNotChanged, dataService.Cache.First().Status);

            var filesOnServer = Directory.GetFiles(cloudStorageProviderMock.TemporaryFolder);
            Assert.AreEqual(3, filesOnServer.Length);

            dataService.DataEntries.First().CanSynchronize = false;

            await cloudStorageService.SynchronizeAsync();

            cloudDataEntries = await GetDataEntriesFromServerAsync();

            Assert.AreEqual(0, cloudDataEntries.Count);
            Assert.AreEqual(1, dataService.DataEntries.Count);
            Assert.AreEqual(1, dataService.Cache.Count);
            Assert.AreEqual(DataEntryStatus.DidNotChanged, dataService.Cache.First().Status);

            filesOnServer = Directory.GetFiles(cloudStorageProviderMock.TemporaryFolder);
            Assert.AreEqual(1, filesOnServer.Length);
            Assert.IsTrue(filesOnServer.First().EndsWith(".clipboard"));
        }

        private CloudStorageService GetCloudStorageService()
        {
            return ServiceLocator.GetService<CloudStorageService>();
        }

        private CloudStorageProviderMock GetCloudStorageProviderMock()
        {
            return (CloudStorageProviderMock)GetCloudStorageService().CurrentCloudStorageProvider;
        }

        private DataService GetDataService()
        {
            return ServiceLocator.GetService<DataService>();
        }

        private WindowsService GetWindowsService()
        {
            return ServiceLocator.GetService<WindowsService>();
        }

        private ClipboardService GetClipboardService()
        {
            return ServiceLocator.GetService<ClipboardService>();
        }

        private async Task<SecureString> GetCloudDataPasswordAsync()
        {
            var cloudStorageProviderMock = GetCloudStorageProviderMock();
            var userId = SecurityHelper.ToSecureString(await cloudStorageProviderMock.GetUserIdAsync());
            return SecurityHelper.ToSecureString(SecurityHelper.EncryptString(userId, SecurityHelper.ToSecureString(await cloudStorageProviderMock.GetUserNameAsync())));
        }

        private async Task<List<CloudDataEntry>> GetDataEntriesFromServerAsync()
        {
            var cloudStorageProviderMock = GetCloudStorageProviderMock();
            if (Directory.Exists(cloudStorageProviderMock.TemporaryFolder))
            {
                using (var memoryStream = new MemoryStream())
                {
                    await cloudStorageProviderMock.DownloadFileAsync(cloudStorageProviderMock.TemporaryFolder + @"\.clipboard", memoryStream);
                    var cloudDataPassword = await GetCloudDataPasswordAsync();
                    using (var aesStream = new AesStream(memoryStream, cloudDataPassword, SecurityHelper.GetSaltKeys(cloudDataPassword).GetBytes(16)))
                    {
                        var data = new byte[aesStream.Length];
                        aesStream.Read(data, 0, data.Length);
                        return JsonConvert.DeserializeObject<List<CloudDataEntry>>(Encoding.UTF8.GetString(data));
                    }
                }
            }

            return null;
        }

        private async Task<MemoryStream> ReadFileFromServerAsync(string filePath)
        {
            var cloudStorageProviderMock = GetCloudStorageProviderMock();
            if (File.Exists(filePath))
            {
                var password = await GetCloudDataPasswordAsync();

                var result = new MemoryStream();
                using (var temporaryLocalFileStream = new MemoryStream())
                {
                    await cloudStorageProviderMock.DownloadFileAsync(filePath, temporaryLocalFileStream);
                    temporaryLocalFileStream.Position = 0;
                    using (var cloudAesStream = new AesStream(temporaryLocalFileStream, password, SecurityHelper.GetSaltKeys(password).GetBytes(16)))
                    {
                        CopyStream(cloudAesStream, result);
                    }
                }

                return result;
            }

            return null;
        }

        private void CopyStream(Stream fromStream, Stream toStream)
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

        private async Task<string> CreateFakeContentOnServerAsync()
        {
            var cloudStorageService = GetCloudStorageService();
            var cloudStorageProviderMock = GetCloudStorageProviderMock();
            var service = GetClipboardService();

            var dataObject = new DataObject();
            dataObject.SetText("Hello", TextDataFormat.UnicodeText);
            dataObject.SetText("Hello", TextDataFormat.Text);
            var entry1 = new ClipboardHookEventArgs(dataObject, false, DateTime.Now.Ticks);

            DispatcherUtil.ExecuteOnDispatcherThread(() => service.ClipboardHook_ClipboardChanged(null, entry1), 100);
            await Task.Delay(300);
            DispatcherUtil.DoEvents();

            await cloudStorageService.SynchronizeAsync();

            // Create all of the directories
            foreach (var dirPath in Directory.GetDirectories(cloudStorageProviderMock.TemporaryFolder, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(cloudStorageProviderMock.TemporaryFolder, cloudStorageProviderMock.TemporaryFolder + "_temp"));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (var newPath in Directory.GetFiles(cloudStorageProviderMock.TemporaryFolder, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(cloudStorageProviderMock.TemporaryFolder, cloudStorageProviderMock.TemporaryFolder + "_temp"), true);
            }

            return cloudStorageProviderMock.TemporaryFolder + "_temp";
        }
    }
}
