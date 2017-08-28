using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Shared.CloudStorage;

namespace ClipboardZanager.Core.Desktop.Tests.Mocks
{
    internal class CloudStorageProviderMock : ICloudStorageProvider
    {
        internal string TemporaryFolder => CoreHelper.GetAppDataFolder();

        public string CloudServiceName => "CloudStorageProviderMock";

        public ControlTemplate CloudServiceIcon { get; }

        public bool IsAuthenticated => true;

        public bool CredentialExists => true;

        public ICloudTokenProvider TokenProvider { get; }

        public Task<bool> TryAuthenticateAsync()
        {
            return Task.FromResult(true);
        }

        public Task<bool> TryAuthenticateWithUiAsync(ICloudAuthentication authentication)
        {
            return TryAuthenticateAsync();
        }

        public Task SignOutAsync()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetUserNameAsync()
        {
            return Task.FromResult("John DOE");
        }

        public Task<string> GetUserIdAsync()
        {
            return Task.FromResult("{44B82A52-CE67-41BB-92F4-C285CE3ADF24}");
        }

        public Task<CloudFolder> GetAppFolderAsync()
        {
            var files = new List<CloudFile>();

            if (!Directory.Exists(TemporaryFolder))
            {
                Directory.CreateDirectory(TemporaryFolder);
            }

            foreach (var file in Directory.GetFiles(TemporaryFolder))
            {
                files.Add(new CloudFile
                {
                    Name = Path.GetFileName(file),
                    FullPath = file,
                    LastModificationUtcDate = new FileInfo(file).LastWriteTime
                });
            }

            return Task.FromResult(new CloudFolder
            {
                Files = files,
                FullPath = TemporaryFolder,
                Name = Path.GetDirectoryName(TemporaryFolder),
                Size = 0
            });
        }

        public Task<CloudFile> UploadFileAsync(Stream baseStream, string remotePath)
        {
            if (File.Exists(remotePath))
            {
                File.Delete(remotePath);
            }

            using (var fileStream = File.OpenWrite(remotePath))
            {
                baseStream.Position = 0;
                baseStream.CopyTo(fileStream);
                fileStream.Position = 0;
            }

            return Task.FromResult(new CloudFile
            {
                Name = Path.GetFileName(remotePath),
                FullPath = remotePath,
                LastModificationUtcDate = new FileInfo(remotePath).LastWriteTime
            });
        }

        public Task DownloadFileAsync(string remotePath, Stream targetStream)
        {
            if (!File.Exists(remotePath))
            {
                throw new FileNotFoundException(remotePath);
            }

            using (var fileStream = File.OpenRead(remotePath))
            {
                fileStream.Position = 0;
                fileStream.CopyTo(targetStream);
                targetStream.Position = 0;
            }

            return Task.CompletedTask;
        }

        public Task DeleteFileAsync(string remotePath)
        {
            File.Delete(remotePath);

            return Task.CompletedTask;
        }
    }
}
