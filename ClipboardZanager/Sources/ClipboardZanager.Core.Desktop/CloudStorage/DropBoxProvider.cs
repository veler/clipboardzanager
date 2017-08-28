using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Shared.CloudStorage;
using ClipboardZanager.Shared.Core;
using ClipboardZanager.Shared.Exceptions;
using ClipboardZanager.Shared.Logs;
using Dropbox.Api;
using Dropbox.Api.Files;

namespace ClipboardZanager.Core.Desktop.CloudStorage
{
    /// <summary>
    /// Provides a set of properties and methods designed to let the user connects to his DropBox account.
    /// </summary>
    internal sealed class DropBoxProvider : ICloudStorageProvider
    {
        #region Fields

        private readonly SecureString _appKey;
        private readonly SecureString _redirectUri;
        private DropboxClient _client;

        #endregion

        #region Properties

        /// <inheritdoc/>
        public string CloudServiceName => "DropBox";

        /// <inheritdoc/>
        public ControlTemplate CloudServiceIcon
        {
            get
            {
                var resx = new ResourceDictionary();
                resx.Source = new Uri(@"/ClipboardZanager.Core.Desktop;component/CloudStorage/DropBox.xaml", UriKind.RelativeOrAbsolute);
                return resx["IconDropBox"] as ControlTemplate;
            }
        }

        /// <inheritdoc/>
        public bool IsAuthenticated { get; private set; }

        /// <inheritdoc/>
        public bool CredentialExists => !string.IsNullOrWhiteSpace(GetAccessToken());

        /// <inheritdoc/>
        public ICloudTokenProvider TokenProvider { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="OneDriveProvider"/> class.
        /// </summary>
        /// <param name="tokenProvider">An object implementing <see cref="ICloudTokenProvider"/> used to get and set the tokens for this storage provider.</param>
        public DropBoxProvider(ICloudTokenProvider tokenProvider)
        {
            Requires.NotNull(tokenProvider, nameof(tokenProvider));
            TokenProvider = tokenProvider;
            _appKey = SecurityHelper.DecryptString(TokenProvider.GetToken("AppKey"));
            _redirectUri = SecurityHelper.DecryptString(TokenProvider.GetToken("RedirectUri"));
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public async Task<bool> TryAuthenticateAsync()
        {
            var accessToken = GetAccessToken();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                IsAuthenticated = false;
                return false;
            }

            var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(20)
            };

            try
            {
                var config = new DropboxClientConfig(CoreHelper.GetApplicationName())
                {
                    HttpClient = httpClient
                };
                _client = new DropboxClient(SecurityHelper.ToUnsecureString(SecurityHelper.DecryptString(accessToken)), config);
                await _client.Users.GetCurrentAccountAsync();
                IsAuthenticated = true;
                Logger.Instance.Information($"User authenticated to {CloudServiceName}.");
            }
            catch (Exception exception)
            {
                Logger.Instance.Error(exception);
                IsAuthenticated = false;
            }

            return IsAuthenticated;
        }

        /// <inheritdoc/>
        public async Task<bool> TryAuthenticateWithUiAsync(ICloudAuthentication authentication)
        {
            var oauth2State = Guid.NewGuid().ToString("N");
            var authenticationUri = DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Token, SecurityHelper.ToUnsecureString(_appKey), new Uri(SecurityHelper.ToUnsecureString(_redirectUri)), oauth2State);

            var authenticationResult = await authentication.AuthenticateAsync(authenticationUri.ToString(), SecurityHelper.ToUnsecureString(_redirectUri));
            if (authenticationResult.IsCanceled)
            {
                IsAuthenticated = false;
                return IsAuthenticated;
            }

            var result = DropboxOAuth2Helper.ParseTokenFragment(authenticationResult.RedirectedUri);
            if (result.State != oauth2State)
            {
                IsAuthenticated = false;
                return IsAuthenticated;
            }

            TokenProvider.SetToken("AccessToken", SecurityHelper.EncryptString(SecurityHelper.ToSecureString(result.AccessToken)));
            return await TryAuthenticateAsync();
        }

        /// <inheritdoc/>
        public Task SignOutAsync()
        {
            _client?.Dispose();

            TokenProvider.SetToken("AccessToken", string.Empty);
            IsAuthenticated = false;

            Logger.Instance.Information($"User signed out from {CloudServiceName}.");
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task<string> GetUserNameAsync()
        {
            ThrowIfNotConnected();
            var account = await _client.Users.GetCurrentAccountAsync();
            return account.Name.DisplayName;
        }

        /// <inheritdoc/>
        public async Task<string> GetUserIdAsync()
        {
            ThrowIfNotConnected();
            var account = await _client.Users.GetCurrentAccountAsync();
            return account.AccountId;
        }

        /// <inheritdoc/>
        public async Task<CloudFolder> GetAppFolderAsync()
        {
            ThrowIfNotConnected();

            ListFolderResult listFolder = null;
            var files = new List<CloudFile>();

            do
            {
                if (listFolder == null)
                {
                    listFolder = await _client.Files.ListFolderAsync(string.Empty);
                }
                else
                {
                    listFolder = await _client.Files.ListFolderContinueAsync(listFolder.Cursor);
                }

                foreach (var file in listFolder.Entries.Where(entry => entry.IsFile))
                {
                    files.Add(new CloudFile
                    {
                        Name = file.Name,
                        FullPath = file.PathLower,
                        LastModificationUtcDate = file.AsFile.ServerModified
                    });
                }
            } while (listFolder.HasMore);

            return new CloudFolder
            {
                Name = "ClipboardZanager",
                Size = 0,
                FullPath = "/",
                Files = files
            };
        }

        /// <inheritdoc/>
        public async Task<CloudFile> UploadFileAsync(Stream baseStream, string remotePath)
        {
            Logger.Instance.Information($"Uploading '{remotePath}' to {CloudServiceName}. Data size : {baseStream.Length} bytes.");
            ThrowIfNotConnected();

            var fileInfo = await _client.Files.UploadAsync(remotePath, WriteMode.Overwrite.Instance, body: baseStream);
            Logger.Instance.Information($"'{remotePath}' uploaded.");
            return new CloudFile
            {
                Name = fileInfo.Name,
                FullPath = fileInfo.PathDisplay,
                LastModificationUtcDate = fileInfo.ServerModified
            };
        }

        /// <inheritdoc/>
        public async Task DownloadFileAsync(string remotePath, Stream targetStream)
        {
            Logger.Instance.Information($"Downloading '{remotePath}' to {CloudServiceName}.");
            ThrowIfNotConnected();

            using (var response = await _client.Files.DownloadAsync(remotePath))
            using (var stream = await response.GetContentAsStreamAsync())
            {
                stream.CopyTo(targetStream);
            }
            Logger.Instance.Information($"'{remotePath}' downloaded.");
        }

        /// <inheritdoc/>
        public async Task DeleteFileAsync(string remotePath)
        {
            Logger.Instance.Information($"Deleting '{remotePath}' from {CloudServiceName}.");
            ThrowIfNotConnected();

            await _client.Files.PermanentlyDeleteAsync(remotePath);

            Logger.Instance.Information($"'{remotePath}' deleted.");
        }

        /// <summary>
        /// Retrieves the refresh token for the OneDrive account.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the token</returns>
        private string GetAccessToken()
        {
            return TokenProvider.GetToken("AccessToken");
        }

        /// <summary>
        /// Throw an exception if the user is not authenticated.
        /// </summary>
        private void ThrowIfNotConnected()
        {
            if (!IsAuthenticated)
            {
                Logger.Instance.Warning($"The user is not authenticated to {CloudServiceName}.");
                throw new NotAuthenticatedException();
            }
        }

        #endregion
    }
}
