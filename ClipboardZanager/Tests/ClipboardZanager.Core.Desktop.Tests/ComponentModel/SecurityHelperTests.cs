using System.Linq;
using ClipboardZanager.Core.Desktop.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClipboardZanager.Core.Desktop.Tests.ComponentModel
{
    [TestClass]
    public class SecurityHelperTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            TestUtilities.Initialize();
        }

        [TestMethod]
        public void EncryptDecryptString()
        {
            var str = "My unsecured string";
            var secureString = SecurityHelper.ToSecureString(str);
            var secureString2 = SecurityHelper.ToSecureString(str);
            var encryptedString = SecurityHelper.EncryptString(secureString);
            var encryptedString2 = SecurityHelper.EncryptString(secureString2);

            Assert.AreEqual(encryptedString, encryptedString2);

            secureString = SecurityHelper.DecryptString(encryptedString);
            secureString2 = SecurityHelper.DecryptString(encryptedString2);
            var str2 = SecurityHelper.ToUnsecureString(secureString);
            var str3 = SecurityHelper.ToUnsecureString(secureString2);

            Assert.AreEqual(str, str2);
            Assert.AreEqual(str2, str3);

            var password = SecurityHelper.ToSecureString("MyPassword");
            secureString = SecurityHelper.ToSecureString(str);
            secureString2 = SecurityHelper.ToSecureString(str);
            encryptedString = SecurityHelper.EncryptString(secureString, password);
            encryptedString2 = SecurityHelper.EncryptString(secureString2, password);

            Assert.AreEqual("NWp2ALQXrKNtf4ZDVaZx7CA+M7DEXBnt/+0wXCN7Ez6eDXT5gZ9RZg==", encryptedString);
            Assert.AreEqual(encryptedString, encryptedString2);

            secureString = SecurityHelper.DecryptString(encryptedString, password);
            secureString2 = SecurityHelper.DecryptString(encryptedString2, password);
            str2 = SecurityHelper.ToUnsecureString(secureString);
            str3 = SecurityHelper.ToUnsecureString(secureString2);

            Assert.AreEqual(str, str2);
            Assert.AreEqual(str2, str3);
        }

        [TestMethod]
        public void EncryptDecryptEmptyString()
        {
            var str = string.Empty;
            var secureString = SecurityHelper.ToSecureString(str);
            var secureString2 = SecurityHelper.ToSecureString(str);
            var encryptedString = SecurityHelper.EncryptString(secureString);
            var encryptedString2 = SecurityHelper.EncryptString(secureString2);

            Assert.AreEqual(encryptedString, encryptedString2);

            secureString = SecurityHelper.DecryptString(encryptedString);
            secureString2 = SecurityHelper.DecryptString(encryptedString2);
            var str2 = SecurityHelper.ToUnsecureString(secureString);
            var str3 = SecurityHelper.ToUnsecureString(secureString2);

            Assert.AreEqual(str, str2);
            Assert.AreEqual(str2, str3);
        }

        [TestMethod]
        public void SaltKeys()
        {
            var secureString = SecurityHelper.GetSaltKeys(SecurityHelper.ToSecureString("MyPassword")).GetBytes(16);
            Assert.IsTrue(secureString.SequenceEqual(new byte[] { 6, 82, (byte)254, 48, 47, (byte)165, 77, 86, 53, 94, 25, 125, (byte)168, (byte)237, (byte)149, 23 }));
        }
    }
}
