using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Core.Desktop.Enums;
using ClipboardZanager.Core.Desktop.Events;
using ClipboardZanager.Core.Desktop.Models;
using ClipboardZanager.Core.Desktop.Services;
using ClipboardZanager.Core.Desktop.Tests.Mocks;
using ClipboardZanager.Shared.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClipboardZanager.Core.Desktop.Tests.Service
{
    [TestClass]
    public class DataServiceTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            TestUtilities.Initialize();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            GetDataService().RemoveAllDataAsync().Wait();
            GetDataService().Cache.Clear();

            if (Directory.Exists(GetDataService().ClipboardDataPath))
            {
                Directory.Delete(GetDataService().ClipboardDataPath, true);
            }
        }

        [TestMethod]
        public void DataService_IsCreditCard()
        {
            var service = GetDataService();

            Assert.IsTrue(service.IsCreditCard("  4974- 0411-3456- 7895 "));
            Assert.IsTrue(service.IsCreditCard("  4974- 0411-   3456- 7895 "));
            Assert.IsTrue(service.IsCreditCard("hello  4974- 0411-   3456- 7895 world"));
            Assert.IsTrue(service.IsCreditCard("hello  4974- 0411- hey  3456- 7895 world"));
            Assert.IsFalse(service.IsCreditCard("hello  1234- 0411- hey  3456- 7895 world"));
        }

        [TestMethod]
        public void DataService_IsPassword()
        {
            var service = GetDataService();

            var window1 = new Models.Window(IntPtr.Zero, "Edge", new Process(), "Microsoft.MicrosoftEdge", null, true);
            var window2 = new Models.Window(IntPtr.Zero, "Notepad", new Process(), "Notepad.exe", null, true);

            Assert.IsFalse(service.IsPassword("Hello", window1));
            Assert.IsTrue(service.IsPassword("M|cr0sof t", window1));
            Assert.IsFalse(service.IsPassword("M|cr0sof t", window2));
        }

        [TestMethod]
        public void DataService_KeepOrIgnoreCreditCard()
        {
            var service = GetDataService();

            Assert.IsTrue(service.KeepOrIgnoreCreditCard("  4974- 0411-3456- 7895 "));
            Assert.IsFalse(service.KeepOrIgnoreCreditCard("  4974- 0411-3456- 7895 "));

            TestUtilities.GetSettingProvider().AvoidCreditCard = false;

            Assert.IsFalse(service.KeepOrIgnoreCreditCard("  4974- 0411-3456- 7895 "));
            Assert.IsFalse(service.KeepOrIgnoreCreditCard("  4974- 0411-3456- 7895 "));
            Assert.IsFalse(service.KeepOrIgnoreCreditCard("  4974- 0411-3456- 7895 "));

            TestUtilities.GetSettingProvider().AvoidCreditCard = true;

            Assert.IsTrue(service.KeepOrIgnoreCreditCard("  4974- 0412-3456- 7895 "));
            Assert.IsTrue(service.KeepOrIgnoreCreditCard("  4974- 0411-3456- 7895 "));
            Assert.IsTrue(service.KeepOrIgnoreCreditCard("  4974- 0412-3456- 7895 "));
        }

        [TestMethod]
        public void DataService_KeepOrIgnorePassword()
        {
            var service = GetDataService();

            Assert.IsTrue(service.KeepOrIgnorePassword("M|cr0sof t"));
            Assert.IsFalse(service.KeepOrIgnorePassword("M|cr0sof t"));

            TestUtilities.GetSettingProvider().AvoidPasswords = false;

            Assert.IsFalse(service.KeepOrIgnorePassword("M|cr0sof t"));
            Assert.IsFalse(service.KeepOrIgnorePassword("M|cr0sof t"));
            Assert.IsFalse(service.KeepOrIgnorePassword("M|cr0sof t"));

            TestUtilities.GetSettingProvider().AvoidPasswords = true;

            Assert.IsTrue(service.KeepOrIgnorePassword("M||cr0sof t"));
            Assert.IsTrue(service.KeepOrIgnorePassword("M|||cr0sof t"));
            Assert.IsTrue(service.KeepOrIgnorePassword("M||cr0sof t"));
        }

        [TestMethod]
        public void DataService_GetDataIdentifiers()
        {
            var service = GetDataService();
            TestUtilities.GetSettingProvider().KeepDataTypes = new ArrayList
                                                               {
                                                                   8, // SupportedDataType.Text
                                                                   7, // SupportedDataType.Files
                                                                   6,  // SupportedDataType.Image
                                                               };

            var identifiers = service.GetDataIdentifiers(new[] { "Shell IDList Array", "DataObjectAttributes", "DataObjectAttributesRequiringElevation", "Preferred DropEffect", "AsyncFlag", DataFormats.FileDrop, "FileName", "FileNameW", "FileGroupDescriptorW" });

            Assert.AreEqual(identifiers.Count, 9);

            identifiers = service.GetDataIdentifiers(new[] { DataFormats.Dib, "Photoshop Paste in Place", "Photoshop Clip Source", "Object Descriptor" });

            Assert.AreEqual(identifiers.Count, 0);
        }

        [TestMethod]
        public async Task DataService_AddRemoveData()
        {
            var service = GetDataService();

            try
            {
                service.AddDataEntry(null, new List<DataIdentifier>(), ServiceLocator.GetService<WindowsService>().GetForegroundWindow(), false, false);
                Assert.Fail();
            }
            catch
            {
            }

            var dataObject = new DataObject();
            dataObject.SetText("Hello World");
            var entry1 = new ClipboardHookEventArgs(dataObject, false, DateTime.Now.Ticks);
            service.AddDataEntry(entry1, new List<DataIdentifier>(), ServiceLocator.GetService<WindowsService>().GetForegroundWindow(), false, false);
            var guid1 = service.DataEntries.Last().Identifier;
            var guid11 = service.Cache.Last().Identifier;

            dataObject = new DataObject();
            dataObject.SetText("Hello World 2");
            var entry2 = new ClipboardHookEventArgs(dataObject, false, DateTime.Now.Ticks);
            service.AddDataEntry(entry2, new List<DataIdentifier>(), ServiceLocator.GetService<WindowsService>().GetForegroundWindow(), false, false);
            var guid2 = service.DataEntries.First().Identifier;
            var guid22 = service.Cache.First().Identifier;

            Assert.AreNotEqual(guid1, guid2);
            Assert.AreNotEqual(guid1, guid22);
            Assert.AreNotEqual(guid11, guid2);

            await service.RemoveDataAsync(service.DataEntries.First().Identifier, service.DataEntries.First().DataIdentifiers);

            Assert.AreEqual(1, service.DataEntries.Count);
            Assert.AreEqual(2, service.Cache.Count);
            Assert.AreEqual(DataEntryStatus.Deleted, service.Cache.First().Status);
            Assert.AreEqual(DataEntryStatus.Added, service.Cache.Last().Status);
        }

        [TestMethod]
        public void DataService_DataEntry()
        {
            var service = GetDataService();

            var dataObject = new DataObject();
            dataObject.SetText("Hello World");
            var entry = new ClipboardHookEventArgs(dataObject, false, DateTime.Now.Ticks);

            service.AddDataEntry(entry, new List<DataIdentifier>(), ServiceLocator.GetService<WindowsService>().GetForegroundWindow(), false, false);

            var dataEntry = service.DataEntries.Last();
            var dataEntryCache = service.Cache.Last();

            Assert.IsTrue(dataEntry.CanSynchronize);
            Assert.IsFalse(dataEntry.IsFavorite);
            Assert.IsFalse(dataEntry.IsCut);
            Assert.IsTrue(dataEntry.Icon != null);
            Assert.IsTrue(dataEntry.Identifier != null);
            Assert.IsTrue(dataEntry.Date != null);

            Assert.AreEqual(dataEntry.Identifier, dataEntryCache.Identifier);
            Assert.AreEqual(DataEntryStatus.Added, dataEntryCache.Status);
        }

        [TestMethod]
        public void DataService_DataEntry_Thumbnail_Text()
        {
            var service = GetDataService();
            var dataObject = new DataObject();
            dataObject.SetText("Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.");
            var entry = new ClipboardHookEventArgs(dataObject, false, DateTime.Now.Ticks);

            service.AddDataEntry(entry, new List<DataIdentifier>(), ServiceLocator.GetService<WindowsService>().GetForegroundWindow(), false, false);

            var dataEntry = service.DataEntries.First();

            Assert.AreEqual(dataEntry.Thumbnail.Type, ThumbnailDataType.String);
            Assert.AreEqual(DataHelper.FromBase64<string>(dataEntry.Thumbnail.Value), "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It h...");
        }

        [TestMethod]
        public void DataService_DataEntry_Thumbnail_Link()
        {
            var service = GetDataService();
            var dataObject = new DataObject();
            dataObject.SetText("http://www.google.com");
            var entry = new ClipboardHookEventArgs(dataObject, false, DateTime.Now.Ticks);
            service.AddDataEntry(entry, new List<DataIdentifier>(), ServiceLocator.GetService<WindowsService>().GetForegroundWindow(), false, false);
            var dataEntry = service.DataEntries.First();

            Assert.AreEqual(dataEntry.Thumbnail.Type, ThumbnailDataType.Link);
            Assert.AreEqual(DataHelper.FromBase64<Link>(dataEntry.Thumbnail.Value).Uri, "http://www.google.com");
            Assert.AreEqual(DataHelper.FromBase64<Link>(dataEntry.Thumbnail.Value).Title, "Google");

            dataObject = new DataObject();
            dataObject.SetText("1");
            entry = new ClipboardHookEventArgs(dataObject, false, DateTime.Now.Ticks);
            service.AddDataEntry(entry, new List<DataIdentifier>(), ServiceLocator.GetService<WindowsService>().GetForegroundWindow(), false, false);
            dataEntry = service.DataEntries.First();

            Assert.AreEqual(dataEntry.Thumbnail.Type, ThumbnailDataType.String);

            dataObject = new DataObject();
            dataObject.SetText("C:\\file.txt");
            entry = new ClipboardHookEventArgs(dataObject, false, DateTime.Now.Ticks);
            service.AddDataEntry(entry, new List<DataIdentifier>(), ServiceLocator.GetService<WindowsService>().GetForegroundWindow(), false, false);
            dataEntry = service.DataEntries.First();

            Assert.AreEqual(dataEntry.Thumbnail.Type, ThumbnailDataType.String);
        }

        [TestMethod]
        public void DataService_DataEntry_Thumbnail_Files()
        {
            var service = GetDataService();
            var dataObject = new DataObject();
            dataObject.SetFileDropList(new StringCollection { "C:/file1.txt", "C:/folder/file2.txt", "C:/folder/file3.txt", "C:/folder/file4.txt" });
            var entry = new ClipboardHookEventArgs(dataObject, false, DateTime.Now.Ticks);

            service.AddDataEntry(entry, new List<DataIdentifier>(), ServiceLocator.GetService<WindowsService>().GetForegroundWindow(), false, false);

            var dataEntry = service.DataEntries.First();

            Assert.AreEqual(dataEntry.Thumbnail.Type, ThumbnailDataType.Files);
            Assert.IsTrue(DataHelper.FromBase64<List<string>>(dataEntry.Thumbnail.Value).SequenceEqual(new List<string> { "C:/file1.txt", "C:/folder/file2.txt", "C:/folder/file3.txt", "C:/folder/file4.txt" }));
        }

        [TestMethod]
        public void DataService_DataEntry_Thumbnail_Unknow()
        {
            var service = GetDataService();
            var dataObject = new DataObject();
            dataObject.SetData("DataFormat", new SerializableClass() { Data = "Hello World" });
            var entry = new ClipboardHookEventArgs(dataObject, false, DateTime.Now.Ticks);

            service.AddDataEntry(entry, new List<DataIdentifier>(), ServiceLocator.GetService<WindowsService>().GetForegroundWindow(), false, false);

            var dataEntry = service.DataEntries.FirstOrDefault();

            Assert.IsNotNull(dataEntry);
            Assert.AreEqual(dataEntry.Thumbnail.Type, ThumbnailDataType.Unknow);
        }

        [TestMethod]
        public void DataService_ExpireLimit()
        {
            var service = GetDataService();

            TestUtilities.GetSettingProvider().MaxDataToKeep += 10;

            for (var i = 0; i < TestUtilities.GetSettingProvider().DateExpireLimit + 5; i++)
            {
                var dataObject = new DataObject();
                dataObject.SetText((TestUtilities.GetSettingProvider().DateExpireLimit + 5 - i).ToString());
                var entry = new ClipboardHookEventArgs(dataObject, false, DateTime.Now.Ticks - TimeSpan.FromDays(TestUtilities.GetSettingProvider().DateExpireLimit + 5 - i).Ticks);
                service.AddDataEntry(entry, new List<DataIdentifier>(), ServiceLocator.GetService<WindowsService>().GetForegroundWindow(), false, false);
            }

            Assert.AreEqual(service.DataEntries.Count, TestUtilities.GetSettingProvider().DateExpireLimit - 1);
            Assert.AreEqual(DataHelper.FromBase64<string>(service.DataEntries.First().Thumbnail.Value), "1");
            Assert.AreEqual(DataHelper.FromBase64<string>(service.DataEntries.Last().Thumbnail.Value), (TestUtilities.GetSettingProvider().DateExpireLimit - 1).ToString());
        }

        [TestMethod]
        public void DataService_MaxDataToKeep()
        {
            var service = GetDataService();

            for (var i = 0; i < TestUtilities.GetSettingProvider().MaxDataToKeep + 5; i++)
            {
                var dataObject = new DataObject();
                dataObject.SetText((TestUtilities.GetSettingProvider().MaxDataToKeep + 5 - i).ToString());
                var entry = new ClipboardHookEventArgs(dataObject, false, DateTime.Now.Ticks);
                service.AddDataEntry(entry, new List<DataIdentifier>(), ServiceLocator.GetService<WindowsService>().GetForegroundWindow(), false, false);
            }

            Assert.AreEqual(service.DataEntries.Count, TestUtilities.GetSettingProvider().MaxDataToKeep);
            Assert.AreEqual(DataHelper.FromBase64<string>(service.DataEntries.First().Thumbnail.Value), "1");
            Assert.AreEqual(DataHelper.FromBase64<string>(service.DataEntries.Last().Thumbnail.Value), TestUtilities.GetSettingProvider().MaxDataToKeep.ToString());
        }

        [TestMethod]
        public async Task DataService_Favorite()
        {
            var service = GetDataService();

            for (var i = 0; i < 10; i++)
            {
                var dataObject = new DataObject();
                dataObject.SetText(i.ToString());
                var entry = new ClipboardHookEventArgs(dataObject, false, DateTime.Now.Ticks);
                service.AddDataEntry(entry, new List<DataIdentifier>(), ServiceLocator.GetService<WindowsService>().GetForegroundWindow(), false, false);
            }

            var dataObject2 = new DataObject();
            dataObject2.SetText("-1");
            var entry2 = new ClipboardHookEventArgs(dataObject2, false, DateTime.Now.Ticks);
            service.AddDataEntry(entry2, new List<DataIdentifier>(), ServiceLocator.GetService<WindowsService>().GetForegroundWindow(), false, false);

            Assert.AreEqual(service.DataEntries.Count, 11);
            Assert.AreEqual(DataHelper.FromBase64<string>(service.DataEntries.First().Thumbnail.Value), "-1");
            Assert.AreEqual(DataHelper.FromBase64<string>(service.DataEntries.Last().Thumbnail.Value), "0");

            service.DataEntries.Last().IsFavorite = true;
            await service.ReorganizeAsync(true);

            Assert.AreEqual(service.DataEntries.Count, 11);
            Assert.AreEqual(DataHelper.FromBase64<string>(service.DataEntries.First().Thumbnail.Value), "0");
            Assert.AreEqual(DataHelper.FromBase64<string>(service.DataEntries.ToList()[1].Thumbnail.Value), "-1");
        }

        [TestMethod]
        public async Task DataService_RemoveAll()
        {
            var service = GetDataService();

            for (var i = 0; i < 10; i++)
            {
                var dataObject = new DataObject();
                dataObject.SetText(i.ToString());
                var entry = new ClipboardHookEventArgs(dataObject, false, DateTime.Now.Ticks);
                service.AddDataEntry(entry, new List<DataIdentifier>(), ServiceLocator.GetService<WindowsService>().GetForegroundWindow(), false, false);
            }

            Assert.AreEqual(service.DataEntries.Count, 10);
            Assert.AreEqual(service.Cache.Count, 10);

            Assert.IsTrue(service.Cache.All(dataEntryCache => dataEntryCache.Status == DataEntryStatus.Added));

            await service.RemoveAllDataAsync();

            Assert.AreEqual(service.DataEntries.Count, 0);
            Assert.AreEqual(service.Cache.Count, 10);

            Assert.IsTrue(service.Cache.All(dataEntryCache => dataEntryCache.Status == DataEntryStatus.Deleted));
        }

        [TestMethod]
        public void DataService_DisablePasswordAndCreditCardSync()
        {
            var window = new Models.Window(IntPtr.Zero, "Edge", new Process(), "Microsoft.MicrosoftEdge", null, true);

            var service = GetDataService();
            var dataObject = new DataObject();
            var entry = new ClipboardHookEventArgs(dataObject, false, DateTime.Now.Ticks);

            service.AddDataEntry(entry, new List<DataIdentifier>(), window, false, false);

            var dataEntry = service.DataEntries[0];
            Assert.IsTrue(dataEntry.CanSynchronize);

            service.AddDataEntry(entry, new List<DataIdentifier>(), window, true, false);

            dataEntry = service.DataEntries[0];
            Assert.IsFalse(dataEntry.CanSynchronize);

            service.AddDataEntry(entry, new List<DataIdentifier>(), window, false, true);

            dataEntry = service.DataEntries[0];
            Assert.IsFalse(dataEntry.CanSynchronize);

            TestUtilities.GetSettingProvider().DisablePasswordAndCreditCardSync = false;

            service.AddDataEntry(entry, new List<DataIdentifier>(), window, false, false);

            dataEntry = service.DataEntries[0];
            Assert.IsTrue(dataEntry.CanSynchronize);

            service.AddDataEntry(entry, new List<DataIdentifier>(), window, true, false);

            dataEntry = service.DataEntries[0];
            Assert.IsTrue(dataEntry.CanSynchronize);

            service.AddDataEntry(entry, new List<DataIdentifier>(), window, false, true);

            dataEntry = service.DataEntries[0];
            Assert.IsTrue(dataEntry.CanSynchronize);

        }

        private DataService GetDataService()
        {
            return ServiceLocator.GetService<DataService>();
        }
    }
}
