using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ClipboardZanager.Core.Desktop.Enums;
using ClipboardZanager.Core.Desktop.Events;
using ClipboardZanager.Core.Desktop.Models;
using ClipboardZanager.Core.Desktop.Services;
using ClipboardZanager.Core.Desktop.Tests;
using ClipboardZanager.Core.Desktop.Tests.Mocks;
using ClipboardZanager.Shared.Services;
using ClipboardZanager.Tests.Mocks;
using ClipboardZanager.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClipboardZanager.Tests.ViewModels
{
    [TestClass]
    public class PasteBarWindowViewModelTests
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
        public void PasteBarWindowViewModel_NoPresentData()
        {
            var viewmodel = new PasteBarWindowViewModel();
            Assert.IsTrue(viewmodel.NoPresentData);
        }

        [TestMethod]
        public async Task PasteBarWindowViewModel_DeleteItem()
        {
            var dataService = ServiceLocator.GetService<DataService>();
            var dataObject = new DataObject();
            dataObject.SetText("Lorem Ipsum");
            var entry = new ClipboardHookEventArgs(dataObject, false, DateTime.Now.Ticks);

            dataService.AddDataEntry(entry, new List<DataIdentifier>(), ServiceLocator.GetService<WindowsService>().GetForegroundWindow(), false, false);

            var viewmodel = new PasteBarWindowViewModel();

            Assert.IsFalse(viewmodel.NoPresentData);
            viewmodel.DeleteItemCommand.CheckBeginExecute(viewmodel.DataEntries.First());
            await Task.Delay(300);
            Assert.IsTrue(viewmodel.NoPresentData);
        }

        [TestMethod]
        public void PasteBarWindowViewModel_SearchQuery()
        {
            var query1 = "test.txt";
            var searchQuery1 = new SearchQuery(query1, SearchType.File);
            Assert.AreEqual(searchQuery1.Query, "test.txt");
            Assert.AreEqual(searchQuery1.Type, SearchType.File);

            var query2 = " conto so   ";
            var searchQuery2 = new SearchQuery(query2, SearchType.Link);
            Assert.AreEqual(searchQuery2.Query, "conto so");
            Assert.AreEqual(searchQuery2.Type, SearchType.Link);

            var query3 = "hello";
            var searchQuery3 = new SearchQuery(query3, SearchType.Text);
            Assert.AreEqual(searchQuery3.Query, "hello");
            Assert.AreEqual(searchQuery3.Type, SearchType.Text);

            var query4 = "hello";
            var searchQuery4 = new SearchQuery(query4, SearchType.All);
            Assert.AreEqual(searchQuery4.Query, "hello");
            Assert.AreEqual(searchQuery4.Type, SearchType.All);

            var query5 = "   ";
            var searchQuery5 = new SearchQuery(query5, SearchType.File);
            Assert.AreEqual(searchQuery5.Query, "   ");
            Assert.AreEqual(searchQuery5.Type, SearchType.File);
        }

        [TestMethod]
        public async Task PasteBarWindowViewModel_Search()
        {
            var dataService = ServiceLocator.GetService<DataService>();
            var service = ServiceLocator.GetService<ClipboardService>();

            var dataObject = new DataObject();
            dataObject.SetText("Lorem Ipsum");
            var entry = new ClipboardHookEventArgs(dataObject, false, DateTime.Now.Ticks);
            DispatcherUtil.ExecuteOnDispatcherThread(() =>
            {
                service.ClipboardHook_ClipboardChanged(null, entry);
                Task.Delay(300).Wait();
                DispatcherUtil.DoEvents();
            }, 100);

            dataObject = new DataObject();
            dataObject.SetText("Lorem ipsum");
            entry = new ClipboardHookEventArgs(dataObject, false, DateTime.Now.Ticks);
            DispatcherUtil.ExecuteOnDispatcherThread(() =>
            {
                service.ClipboardHook_ClipboardChanged(null, entry);
                Task.Delay(300).Wait();
                DispatcherUtil.DoEvents();
            }, 100);

            dataObject = new DataObject();
            dataObject.SetText("#ffffff");
            entry = new ClipboardHookEventArgs(dataObject, false, DateTime.Now.Ticks);
            DispatcherUtil.ExecuteOnDispatcherThread(() =>
            {
                service.ClipboardHook_ClipboardChanged(null, entry);
                Task.Delay(300).Wait();
                DispatcherUtil.DoEvents();
            }, 100);

            dataObject = new DataObject();
            dataObject.SetText("http://www.ipsum.com/");
            entry = new ClipboardHookEventArgs(dataObject, false, DateTime.Now.Ticks);  
            DispatcherUtil.ExecuteOnDispatcherThread(() =>
            {
                service.ClipboardHook_ClipboardChanged(null, entry);
                Task.Delay(300).Wait();
                DispatcherUtil.DoEvents();
            }, 100);

            dataObject = new DataObject();
            dataObject.SetFileDropList(new StringCollection { "C:/file1.txt", "C:/folder/file2.txt", "C:/folder/file3.txt", "C:/folder/file4.txt" });
            entry = new ClipboardHookEventArgs(dataObject, false, DateTime.Now.Ticks);
            DispatcherUtil.ExecuteOnDispatcherThread(() =>
            {
                service.ClipboardHook_ClipboardChanged(null, entry);
                Task.Delay(300).Wait();
                DispatcherUtil.DoEvents();
            }, 100);

            var viewmodel = new PasteBarWindowViewModel();
            Assert.AreEqual(viewmodel.CollectionView.Cast<DataEntry>().Count(), 5);

            viewmodel.SearchQueryString = "ipsum";
            viewmodel.SearchCommand.CheckBeginExecute(true);
            viewmodel.IgnoreSearch = false;
            await Task.Delay(500);
            DispatcherUtil.DoEvents();

            Assert.AreEqual(viewmodel.CollectionView.Cast<DataEntry>().Count(), 3);

            viewmodel.SearchQueryString = "Ipsum";
            viewmodel.SearchCommand.CheckBeginExecute(true);
            viewmodel.IgnoreSearch = false;
            await Task.Delay(500);
            DispatcherUtil.DoEvents();

            Assert.AreEqual(viewmodel.CollectionView.Cast<DataEntry>().Count(), 1);

            viewmodel.SearchQueryString = "#fff";
            viewmodel.SearchCommand.CheckBeginExecute(true);
            viewmodel.IgnoreSearch = false;
            await Task.Delay(500);
            DispatcherUtil.DoEvents();

            Assert.AreEqual(viewmodel.CollectionView.Cast<DataEntry>().Count(), 1);

            viewmodel.SearchType = SearchType.Link;
            viewmodel.SearchQueryString = "ipsum";
            viewmodel.SearchCommand.CheckBeginExecute(true);
            viewmodel.IgnoreSearch = false;
            await Task.Delay(500);
            DispatcherUtil.DoEvents();

            Assert.AreEqual(viewmodel.CollectionView.Cast<DataEntry>().Count(), 1);

            viewmodel.SearchType = SearchType.File;
            viewmodel.SearchQueryString = "";
            viewmodel.SearchCommand.CheckBeginExecute(true);
            viewmodel.IgnoreSearch = false;
            await Task.Delay(500);
            DispatcherUtil.DoEvents();

            Assert.AreEqual(viewmodel.CollectionView.Cast<DataEntry>().Count(), 1);

            viewmodel.SearchType = SearchType.All;
            viewmodel.SearchQueryString = "*.txt";
            viewmodel.SearchCommand.CheckBeginExecute(true);
            viewmodel.IgnoreSearch = false;
            await Task.Delay(500);
            DispatcherUtil.DoEvents();

            Assert.AreEqual(viewmodel.CollectionView.Cast<DataEntry>().Count(), 1);
        }
    }
}
