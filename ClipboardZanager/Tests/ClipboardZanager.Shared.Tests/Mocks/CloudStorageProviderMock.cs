using System;
using System.IO;
using System.Threading.Tasks;
using ClipboardZanager.Shared.CloudStorage;
using ClipboardZanager.Shared.Exceptions;
using System.Windows.Controls;

namespace ClipboardZanager.Shared.Tests.Mocks
{
    public class CloudStorageProviderMock : ICloudStorageProvider
    {
        public string CloudServiceName => "MockCloudStorageProvider";

        /// <inheritdoc/>
        public ControlTemplate CloudServiceIcon { get; }

        public bool IsAuthenticated { get; private set; }

        public bool CredentialExists => !string.IsNullOrEmpty(TokenProvider.GetToken("MyToken"));

        public ICloudTokenProvider TokenProvider => new CloudTokenProviderMock();

        public async Task<bool> TryAuthenticateAsync()
        {
            if (IsAuthenticated)
            {
                await SignOutAsync();
            }

            IsAuthenticated = false;
            return IsAuthenticated;
        }

        public async Task<bool> TryAuthenticateWithUiAsync(ICloudAuthentication authentication)
        {
            if (IsAuthenticated)
            {
                await SignOutAsync();
            }

            IsAuthenticated = !(await authentication.AuthenticateAsync("http://authenticationUri", "http://exceptedUi")).IsCanceled;
            return IsAuthenticated;
        }

        public Task SignOutAsync()
        {
            return Task.Run(() =>
            {
                IsAuthenticated = false;
                TokenProvider.SetToken("MyToken", string.Empty);
            });
        }

        public Task<string> GetUserNameAsync()
        {
            ThowIfNotConnected();

            return Task.Run(() => "John DOE");
        }

        public Task<string> GetUserIdAsync()
        {
            ThowIfNotConnected();

            return Task.Run(() => "{1df5a5312fa35f3ae1df35e1869}");
        }

        public Task<CloudFolder> GetAppFolderAsync()
        {
            return Task.Run(() =>
            {
                ThowIfNotConnected();

                return new CloudFolder { FullPath = "/path/MyApp", Name = "MyApp", Size = 0 };
            });
        }

        public Task<CloudFile> UploadFileAsync(Stream baseStream, string remotePath)
        {
            return Task.Run(() =>
            {
                ThowIfNotConnected();

                if (baseStream == null)
                {
                    throw new NullReferenceException();
                }

                return new CloudFile
                {
                    FullPath = remotePath,
                    Name = "myFile.txt",
                    LastModificationUtcDate = DateTime.Now
                };
            });
        }

        public Task DownloadFileAsync(string remotePath, Stream targetStream)
        {
            return Task.Run(() =>
            {
                ThowIfNotConnected();

                if (targetStream == null)
                {
                    throw new NullReferenceException();
                }

                if (remotePath != "/path/MyApp/myFile.txt")
                {
                    throw new FileNotFoundException();
                }

                var data = new byte[] { 0, 1, 2, 3, 4 };
                targetStream.Write(data, 0, data.Length);
            });
        }

        public Task DeleteFileAsync(string remotePath)
        {
            if (remotePath != "/path/MyApp/myFile.txt")
            {
                throw new FileNotFoundException();
            }

            return Task.CompletedTask;
        }

        private void ThowIfNotConnected()
        {
            if (!IsAuthenticated)
            {
                throw new NotAuthenticatedException();
            }
        }
    }
}
