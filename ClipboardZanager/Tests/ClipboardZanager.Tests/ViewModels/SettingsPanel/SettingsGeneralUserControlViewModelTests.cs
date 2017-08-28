using System.Threading.Tasks;
using System.Windows.Forms;
using ClipboardZanager.ComponentModel.Enums;
using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Core.Desktop.Tests;
using ClipboardZanager.Tests.Mocks;
using ClipboardZanager.ViewModels.SettingsPanels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClipboardZanager.Tests.ViewModels.SettingsPanel
{
    [TestClass]
    public class SettingsGeneralUserControlViewModelTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            TestUtilities.Initialize();
        }

        [TestMethod]
        public void AvailableLanguages()
        {
            var viewmodel = new SettingsGeneralUserControlViewModel();
            var languages = viewmodel.AvailableLanguages;

            Assert.AreEqual(languages.Count, 2);
            Assert.AreEqual(languages[0].NativeName, "English");
            Assert.AreEqual(languages[1].NativeName, "français");
        }

        [TestMethod]
        public void DefaultSettings()
        {
            var viewmodelGeneral = new SettingsGeneralUserControlViewModel();
            var viewmodelData = new SettingsDataUserControlViewModel();
            var viewmodelSecurity = new SettingsSecurityUserControlViewModel();
            var viewmodelNotifications = new SettingsNotificationsUserControlViewModel();
            var currentLang = viewmodelGeneral.CurrentLanguage;

            RestoreDefault(viewmodelGeneral);

            Assert.AreEqual(viewmodelGeneral.CurrentLanguage, currentLang);

            Assert.IsTrue(viewmodelGeneral.KeyboardGesture);
            Assert.IsTrue(viewmodelGeneral.MouseGesture);
            Assert.IsTrue(viewmodelGeneral.ClosePasteBarWhenMouseIsAway);
            Assert.IsTrue(viewmodelGeneral.ClosePasteBarWithHotKey);
            Assert.AreEqual(viewmodelGeneral.PasteBarPosition, PasteBarPosition.Top);
            Assert.AreEqual(viewmodelGeneral.DisplayedCurrentKeyboardShortcut, "Left Alt + V");

            Assert.IsTrue(viewmodelData.KeepDataAfterReboot);
            Assert.IsTrue(viewmodelData.KeepAdobePhotoshopData);
            Assert.IsTrue(viewmodelData.KeepFilesData);
            Assert.IsTrue(viewmodelData.KeepImagesData);
            Assert.IsTrue(viewmodelData.KeepMicrosoftExcelData);
            Assert.IsTrue(viewmodelData.KeepMicrosoftOutlookData);
            Assert.IsTrue(viewmodelData.KeepMicrosoftPowerPointData);
            Assert.IsTrue(viewmodelData.KeepMicrosoftWordData);
            Assert.IsTrue(viewmodelData.KeepTextData);
            Assert.AreEqual(viewmodelData.DateExpireLimit, "30");
            Assert.AreEqual(viewmodelData.MaxDataToKeep, "25");

            Assert.IsTrue(viewmodelSecurity.AvoidCreditCard);
            Assert.IsTrue(viewmodelSecurity.AvoidPasswords);
            Assert.AreEqual(viewmodelSecurity.IgnoredApplications.Count, 0);

            Assert.IsTrue(viewmodelNotifications.NotifyCreditCard);
            Assert.IsTrue(viewmodelNotifications.NotifyPassword);
            Assert.IsTrue(viewmodelNotifications.NotifySyncFailed);
        }

        [TestMethod]
        public async Task ChangeKeyboardShortcut()
        {
            var viewmodel = new SettingsGeneralUserControlViewModel();

            RestoreDefault(viewmodel);

            Assert.AreEqual(viewmodel.DisplayedCurrentKeyboardShortcut, "Left Alt + V");

            viewmodel.ToggleChangeHotKeysCommand.CheckBeginExecute();
            Assert.AreEqual(viewmodel.DisplayedTemporaryKeyboardShortcut, "Left Alt + V");

            SendKeys.SendWait("{a}");
            await Task.Delay(500);

            SendKeys.SendWait("{b}");
            await Task.Delay(500);

            SendKeys.SendWait("{c}");
            await Task.Delay(500);

            Assert.AreEqual(viewmodel.DisplayedTemporaryKeyboardShortcut, "A + B + C");
            Assert.AreEqual(viewmodel.DisplayedCurrentKeyboardShortcut, "Left Alt + V");

            viewmodel.AcceptHotKeysCommand.CheckBeginExecute();

            Assert.AreEqual(viewmodel.DisplayedCurrentKeyboardShortcut, "A + B + C");

            viewmodel.ChangeHotKeyPopupClosedCommand.CheckBeginExecute();
        }

        private void RestoreDefault(SettingsGeneralUserControlViewModel viewModel)
        {
            viewModel.ConfirmRestoreDefaultCommand.CheckBeginExecute();
            Task.Delay(1000).Wait();

            CoreHelper.UpdateStartWithWindowsShortcut(false);
        }
    }
}
