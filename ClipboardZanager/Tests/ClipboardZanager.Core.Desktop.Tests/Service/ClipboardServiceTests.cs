using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Core.Desktop.Events;
using ClipboardZanager.Core.Desktop.Models;
using ClipboardZanager.Core.Desktop.Services;
using ClipboardZanager.Core.Desktop.Tests.Mocks;
using ClipboardZanager.Shared.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClipboardZanager.Core.Desktop.Tests.Service
{
    [TestClass]
    public class ClipboardServiceTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            TestUtilities.Initialize();
            ServiceLocator.GetService<DataService>().RemoveAllDataAsync().Wait();
            ServiceLocator.GetService<DataService>().Cache.Clear();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (Directory.Exists(ServiceLocator.GetService<DataService>().ClipboardDataPath))
            {
                Directory.Delete(ServiceLocator.GetService<DataService>().ClipboardDataPath, true);
            }
        }

        [TestMethod]
        public void ClipboardService_AddData()
        {
            var dataService = ServiceLocator.GetService<DataService>();
            var service = GetClipboardService();

            var dataObject = new DataObject();
            dataObject.SetText("Hello World");
            var entry1 = new ClipboardHookEventArgs(dataObject, false, DateTime.Now.Ticks);

            DispatcherUtil.ExecuteOnDispatcherThread(() => service.ClipboardHook_ClipboardChanged(null, entry1), 100);
            Task.Delay(300).Wait();
            DispatcherUtil.DoEvents();

            Assert.AreEqual(dataService.DataEntries.Count, 1);
        }

        [TestMethod]
        public void ClipboardService_CreditCard()
        {
            var service = GetClipboardService();
            var dataService = ServiceLocator.GetService<DataService>();

            var dataObject = new DataObject();
            dataObject.SetText("  4974- 0411-3456- 7895 ");
            var entry1 = new ClipboardHookEventArgs(dataObject, false, DateTime.Now.Ticks);

            DispatcherUtil.ExecuteOnDispatcherThread(() =>
            {
                service.ClipboardHook_ClipboardChanged(null, entry1);
            }, 100);
            Task.Delay(300).Wait();
            DispatcherUtil.DoEvents();

            Assert.AreEqual(dataService.DataEntries.Count, 0);

            DispatcherUtil.ExecuteOnDispatcherThread(() =>
            {
                service.ClipboardHook_ClipboardChanged(null, entry1);
            }, 100);
            Task.Delay(300).Wait();
            DispatcherUtil.DoEvents();
            Assert.AreEqual(dataService.DataEntries.Count, 1);
            Assert.AreEqual(DataHelper.FromBase64<string>(dataService.DataEntries.First().Thumbnail.Value), "4974-••••-••••-7895");

            dataObject = new DataObject();
            dataObject.SetText("  4974- 0411-3451- 7895 ");
            entry1 = new ClipboardHookEventArgs(dataObject, false, DateTime.Now.Ticks);

            DispatcherUtil.ExecuteOnDispatcherThread(() =>
            {
                service.ClipboardHook_ClipboardChanged(null, entry1);
            }, 100);
            Task.Delay(300).Wait();
            DispatcherUtil.DoEvents();
            Assert.AreEqual(dataService.DataEntries.Count, 1);

            DispatcherUtil.ExecuteOnDispatcherThread(() =>
            {
                service.ClipboardHook_ClipboardChanged(null, entry1);
            }, 100);
            Task.Delay(300).Wait();
            DispatcherUtil.DoEvents();
            Assert.AreEqual(dataService.DataEntries.Count, 2);

            TestUtilities.GetSettingProvider().AvoidCreditCard = false;

            DispatcherUtil.ExecuteOnDispatcherThread(() =>
            {
                service.ClipboardHook_ClipboardChanged(null, entry1);
            }, 100);
            Task.Delay(300).Wait();
            DispatcherUtil.DoEvents();
            Assert.AreEqual(dataService.DataEntries.Count, 3);
        }

        [TestMethod]
        public async Task ClipboardService_IgnoredApplication()
        {
            var service = GetClipboardService();
            var dataService = ServiceLocator.GetService<DataService>();

            var dataObject = new DataObject();
            dataObject.SetText("Hello World");
            var entry1 = new ClipboardHookEventArgs(dataObject, false, DateTime.Now.Ticks);

            DispatcherUtil.ExecuteOnDispatcherThread(() => service.ClipboardHook_ClipboardChanged(null, entry1), 100);
            await Task.Delay(300);
            DispatcherUtil.DoEvents();
            Assert.AreEqual(dataService.DataEntries.Count, 1);

            TestUtilities.GetSettingProvider().IgnoredApplications.Add(new IgnoredApplication() { ApplicationIdentifier = @"C:\Windows\explorer.exe" });

            DispatcherUtil.ExecuteOnDispatcherThread(() =>
            {
                service.Reset();
                service.ClipboardHook_ClipboardChanged(null, entry1);
            }, 100);
            await Task.Delay(300);
            DispatcherUtil.DoEvents();
            Assert.AreEqual(dataService.DataEntries.Count, 1);
        }

        [TestMethod]
        public async Task ClipboardService_MaxDataToKeep()
        {
            var dataService = ServiceLocator.GetService<DataService>();

            var service = GetClipboardService();

            for (var i = 0; i < TestUtilities.GetSettingProvider().MaxDataToKeep + 5; i++)
            {
                var dataObject = new DataObject();
                dataObject.SetText((TestUtilities.GetSettingProvider().MaxDataToKeep + 5 - i).ToString());
                var entry = new ClipboardHookEventArgs(dataObject, false, DateTime.Now.Ticks);

                DispatcherUtil.ExecuteOnDispatcherThread(() => service.ClipboardHook_ClipboardChanged(null, entry), 100);
                await Task.Delay(100);
                DispatcherUtil.DoEvents();
            }

            Assert.AreEqual(dataService.DataEntries.Count, TestUtilities.GetSettingProvider().MaxDataToKeep);
            Assert.AreEqual(DataHelper.FromBase64<string>(dataService.DataEntries.First().Thumbnail.Value), "1");
            Assert.AreEqual(DataHelper.FromBase64<string>(dataService.DataEntries.Last().Thumbnail.Value), TestUtilities.GetSettingProvider().MaxDataToKeep.ToString());
        }

        private ClipboardService GetClipboardService()
        {
            return ServiceLocator.GetService<ClipboardService>();
        }
    }
}