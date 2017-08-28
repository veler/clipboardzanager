using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClipboardZanager.Shared.Exceptions;
using ClipboardZanager.Shared.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClipboardZanager.Shared.Tests.CloudStorage
{
    [TestClass]
    public class CloudStorageTests
    {
        [TestMethod]
        public async Task CloudAuthentication()
        {
            var authentication = new CloudAuthenticationMock();

            var success = await authentication.AuthenticateAsync("http://authenticationUri", "http://exceptedUi");
            var fail = await authentication.AuthenticateAsync(string.Empty, "http://exceptedUi");

            Assert.IsFalse(success.IsCanceled);
            Assert.AreEqual(success.RedirectedUri.OriginalString, "http://exceptedUi");
            Assert.IsTrue(fail.IsCanceled);
        }

        [TestMethod]
        public void CloudTokenProvider()
        {
            var tokenProvider = new CloudTokenProviderMock();

            var token = tokenProvider.GetToken("MyToken");
            Assert.AreEqual(token, "123456789abc");

            try
            {
                var token2 = tokenProvider.GetToken("MyToken2");
                Assert.Fail();
            }
            catch
            {
            }
        }

        [TestMethod]
        public async Task CloudStorageProvider()
        {
            var storageProvider = new CloudStorageProviderMock();
            var stream = new MemoryStream();

            Assert.AreEqual(storageProvider.CloudServiceName, "MockCloudStorageProvider");
            Assert.IsFalse(storageProvider.IsAuthenticated);
            Assert.IsTrue(storageProvider.CredentialExists);

            try
            {
                await storageProvider.DownloadFileAsync("/path/MyApp/myFile.txt", stream);
                Assert.Fail();
            }
            catch (NotAuthenticatedException)
            {
            }
            catch (Exception)
            {
                Assert.Fail();
            }

            Assert.IsFalse(await storageProvider.TryAuthenticateAsync());
            Assert.IsFalse(storageProvider.IsAuthenticated);

            Assert.IsTrue(await storageProvider.TryAuthenticateWithUiAsync(new CloudAuthenticationMock()));
            Assert.IsTrue(storageProvider.IsAuthenticated);

            try
            {
                await storageProvider.DownloadFileAsync("/path/MyApp/myFile.txt", null);
                Assert.Fail();
            }
            catch (NullReferenceException)
            {
            }
            catch (Exception)
            {
                Assert.Fail();
            }

            try
            {
                await storageProvider.DownloadFileAsync("/path/MyApp/myFile2.txt", stream);
                Assert.Fail();
            }
            catch (FileNotFoundException)
            {
            }
            catch (Exception)
            {
                Assert.Fail();
            }

            await storageProvider.DownloadFileAsync("/path/MyApp/myFile.txt", stream);
            Assert.IsTrue(stream.ToArray().SequenceEqual(new byte[] { 0, 1, 2, 3, 4 }));

            Assert.IsTrue(storageProvider.IsAuthenticated);
            await storageProvider.SignOutAsync();
            Assert.IsFalse(storageProvider.IsAuthenticated);
        }
    }
}
