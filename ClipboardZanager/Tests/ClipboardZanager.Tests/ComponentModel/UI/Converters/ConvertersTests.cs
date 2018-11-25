using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using ClipboardZanager.ComponentModel.Enums;
using ClipboardZanager.ComponentModel.UI.Converters;
using ClipboardZanager.Core.Desktop.Events;
using ClipboardZanager.Core.Desktop.Models;
using ClipboardZanager.Core.Desktop.Services;
using ClipboardZanager.Core.Desktop.Tests;
using ClipboardZanager.Shared.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClipboardZanager.Tests.ComponentModel.UI.Converters
{
    [TestClass]
    public class ConvertersTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            TestUtilities.Initialize();
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
        public void BooleanToBrushConverter()
        {
            var trueColor = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            var falseColor = new SolidColorBrush(Color.FromRgb(255, 0, 255));

            var converter = new BooleanToBrushConverter();
            converter.TrueValue = trueColor;
            converter.FalseValue = falseColor;
            converter.HighContrastTrueValue = trueColor;
            converter.HighContrastFalseValue = falseColor;

            Assert.AreEqual(trueColor, converter.Convert(true, typeof(ConvertersTests), null, CultureInfo.CurrentCulture));
            Assert.AreEqual(falseColor, converter.Convert(false, typeof(ConvertersTests), null, CultureInfo.CurrentCulture));
        }

        [TestMethod]
        public void IntegerManipulationConverter()
        {
            var converter = new IntegerManipulationConverter();
            converter.Manipulation = IntegerManipulation.Multiplication;
            converter.Value = 2;
            Assert.AreEqual(6, converter.Convert(3, typeof(ConvertersTests), null, CultureInfo.CurrentCulture));

            try
            {
                converter.Convert("test", typeof(ConvertersTests), null, CultureInfo.CurrentCulture);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void EnumToBooleanConverter()
        {
            var converter = new EnumToBooleanConverter();
            Assert.AreEqual(true, converter.Convert(PasteBarPosition.Top, typeof(ConvertersTests), "Top", CultureInfo.CurrentCulture));
            Assert.AreEqual(false, converter.Convert(PasteBarPosition.Bottom, typeof(ConvertersTests), "Top", CultureInfo.CurrentCulture));
        }

        [TestMethod]
        public void EnumToVisibilityConverter()
        {
            var converter = new EnumToVisibilityConverter();
            Assert.AreEqual(Visibility.Visible, converter.Convert(SettingsViewMode.General, typeof(ConvertersTests), "General", CultureInfo.CurrentCulture));
            Assert.AreEqual(Visibility.Collapsed, converter.Convert(SettingsViewMode.General, typeof(ConvertersTests), "Data", CultureInfo.CurrentCulture));
        }

        [TestMethod]
        public void BooleanToVerticalAlignmentConverter()
        {
            var converter = new BooleanToVerticalAlignmentConverter();
            converter.True = VerticalAlignment.Bottom;
            converter.False = VerticalAlignment.Top;
            Assert.AreEqual(VerticalAlignment.Bottom, converter.Convert(true, typeof(ConvertersTests), null, CultureInfo.CurrentCulture));
            Assert.AreEqual(VerticalAlignment.Top, converter.Convert(false, typeof(ConvertersTests), null, CultureInfo.CurrentCulture));
        }

        [TestMethod]
        public void BooleanToVisibilityConverter()
        {
            var converter = new BooleanToVisibilityConverter();
            Assert.AreEqual(Visibility.Visible, converter.Convert(true, typeof(ConvertersTests), null, CultureInfo.CurrentCulture));
            Assert.AreEqual(Visibility.Collapsed, converter.Convert(false, typeof(ConvertersTests), null, CultureInfo.CurrentCulture));
            converter.IsInverted = true;
            Assert.AreEqual(Visibility.Collapsed, converter.Convert(true, typeof(ConvertersTests), null, CultureInfo.CurrentCulture));
            Assert.AreEqual(Visibility.Visible, converter.Convert(false, typeof(ConvertersTests), null, CultureInfo.CurrentCulture));
        }

        [TestMethod]
        public void BooleanToInvertedBooleanConverter()
        {
            var converter = new BooleanToInvertedBooleanConverter();
            Assert.AreEqual(false, converter.Convert(true, typeof(ConvertersTests), null, CultureInfo.CurrentCulture));
            Assert.AreEqual(true, converter.Convert(false, typeof(ConvertersTests), null, CultureInfo.CurrentCulture));
        }

        [TestMethod]
        public void BooleanToIntegerConverter()
        {
            var converter = new BooleanToIntegerConverter();
            converter.TrueValue = 2;
            converter.FalseValue = 3;
            Assert.AreEqual(2, converter.Convert(true, typeof(ConvertersTests), null, CultureInfo.CurrentCulture));
            Assert.AreEqual(3, converter.Convert(false, typeof(ConvertersTests), null, CultureInfo.CurrentCulture));
        }

        [TestMethod]
        public void BooleanToThicknessConverter()
        {
            var converter = new BooleanToThicknessConverter();
            converter.TrueValue = new Thickness(0, 0, 0, 0);
            converter.FalseValue = new Thickness(1, 1, 1, 1);
            Assert.AreEqual(new Thickness(0, 0, 0, 0), converter.Convert(true, typeof(ConvertersTests), null, CultureInfo.CurrentCulture));
            Assert.AreEqual(new Thickness(1, 1, 1, 1), converter.Convert(false, typeof(ConvertersTests), null, CultureInfo.CurrentCulture));
        }

        [TestMethod]
        public void ThumbnailToValueConverter()
        {
            var converter = new ThumbnailToValueConverter();

            var service = ServiceLocator.GetService<DataService>();
            var dataObject = new DataObject();
            dataObject.SetFileDropList(new StringCollection { "C:/file1.txt", "C:/folder/file2.txt", "C:/folder/file3.txt", "C:/folder/file4.txt" });
            var entry = new ClipboardHookEventArgs(dataObject, false, DateTime.Now.Ticks);

            service.AddDataEntry(entry, new List<DataIdentifier>(), ServiceLocator.GetService<WindowsService>().GetForegroundWindow(), false);

            var dataEntry = service.DataEntries.First();

            var files = (List<string>)converter.Convert(dataEntry.Thumbnail, typeof(ConvertersTests), "Files", CultureInfo.CurrentCulture);

            Assert.IsTrue(files.SequenceEqual(new List<string> { "C:/file1.txt", "C:/folder/file2.txt", "C:/folder/file3.txt", "..." }));
        }

        [TestMethod]
        public void NullToVisibilityConverter()
        {
            var converter = new NullToVisibilityConverter();
            Assert.AreEqual(Visibility.Visible, converter.Convert("", typeof(ConvertersTests), null, CultureInfo.CurrentCulture));
            Assert.AreEqual(Visibility.Collapsed, converter.Convert("hello", typeof(ConvertersTests), null, CultureInfo.CurrentCulture));
            converter.IsInverted = true;
            Assert.AreEqual(Visibility.Collapsed, converter.Convert("", typeof(ConvertersTests), null, CultureInfo.CurrentCulture));
            Assert.AreEqual(Visibility.Visible, converter.Convert("hello", typeof(ConvertersTests), null, CultureInfo.CurrentCulture));
        }

        [TestMethod]
        public void FlowDirectionToStringConverter()
        {
            var converter = new FlowDirectionToStringConverter();
            converter.LeftToRightValue = "Hello";
            converter.RightToLeftValue = "World";
            Assert.AreEqual("Hello", converter.Convert(FlowDirection.LeftToRight, typeof(ConvertersTests), null, CultureInfo.CurrentCulture));
            Assert.AreEqual("World", converter.Convert(FlowDirection.RightToLeft, typeof(ConvertersTests), null, CultureInfo.CurrentCulture));
        }
    }
}
