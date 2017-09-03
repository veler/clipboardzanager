using System.Threading.Tasks;
using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Core.Desktop.Tests;
using ClipboardZanager.Tests.Mocks;
using ClipboardZanager.ViewModels.SettingsPanels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClipboardZanager.Tests.ViewModels.SettingsPanel
{
    [TestClass]
    public class SettingsDataUserControlViewModelTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            TestUtilities.Initialize();
        }

        [TestMethod]
        public void SettingsDataUserControlViewModel_MaxAndMinValues()
        {
            var viewmodel = new SettingsDataUserControlViewModel();

            RestoreDefault(new SettingsGeneralUserControlViewModel());

            Assert.AreEqual(viewmodel.DateExpireLimit, "30");
            Assert.AreEqual(viewmodel.MaxDataToKeep, "25");

            viewmodel.DateExpireLimit = "1000";
            Assert.AreEqual(viewmodel.DateExpireLimit, "90");

            viewmodel.MaxDataToKeep = "1000";
            Assert.AreEqual(viewmodel.MaxDataToKeep, "100");

            viewmodel.DateExpireLimit = "0";
            Assert.AreEqual(viewmodel.DateExpireLimit, "1");

            viewmodel.MaxDataToKeep = "0";
            Assert.AreEqual(viewmodel.MaxDataToKeep, "1");
        }

        private async void RestoreDefault(SettingsGeneralUserControlViewModel viewModel)
        {
            viewModel.ConfirmRestoreDefaultCommand.CheckBeginExecute();
            Task.Delay(300).Wait();

            CoreHelper.SetAppStartsAtLogin(false);
        }
    }
}
