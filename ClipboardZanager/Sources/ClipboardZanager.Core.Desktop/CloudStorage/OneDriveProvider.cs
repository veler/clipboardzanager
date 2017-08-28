using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Shared.CloudStorage;
using ClipboardZanager.Shared.Core;
using ClipboardZanager.Shared.Exceptions;
using ClipboardZanager.Shared.Logs;
using Microsoft.Graph;
using Microsoft.OneDrive.Sdk;
using Microsoft.OneDrive.Sdk.Authentication;
using Newtonsoft.Json.Linq;

namespace ClipboardZanager.Core.Desktop.CloudStorage
{
    /// <summary>
    /// Provides a set of properties and methods designed to let the user connects to his OneDrive account.
    /// </summary>
    internal sealed class OneDriveProvider : ICloudStorageProvider
    {
        #region Fields

        private readonly string[] _scope = { "wl.signin", "wl.offline_access", "onedrive.appfolder", "onedrive.readwrite" };
        private readonly SecureString _clientId;
        private readonly SecureString _redirectUri;

        private OneDriveClient _client;

        #endregion

        #region Properties

        /// <inheritdoc/>
        public string CloudServiceName => "OneDrive";

        /// <inheritdoc/>
        public ControlTemplate CloudServiceIcon
        {
            get
            {
                var resx = new ResourceDictionary();
                resx.Source = new Uri(@"/ClipboardZanager.Core.Desktop;component/CloudStorage/OneDrive.xaml", UriKind.RelativeOrAbsolute);
                return resx["IconOneDrive"] as ControlTemplate;
            }
        }

        /// <inheritdoc/>
        public bool IsAuthenticated { get; private set; }

        /// <inheritdoc/>
        public bool CredentialExists => !string.IsNullOrWhiteSpace(GetRefreshToken());

        /// <inheritdoc/>
        public ICloudTokenProvider TokenProvider { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="OneDriveProvider"/> class.
        /// </summary>
        /// <param name="tokenProvider">An object implementing <see cref="ICloudTokenProvider"/> used to get and set the tokens for this storage provider.</param>
        public OneDriveProvider(ICloudTokenProvider tokenProvider)
        {
            Requires.NotNull(tokenProvider, nameof(tokenProvider));
            TokenProvider = tokenProvider;
            _clientId = SecurityHelper.DecryptString(TokenProvider.GetToken("ClientID"));
            _redirectUri = SecurityHelper.DecryptString(TokenProvider.GetToken("RedirectUri"));
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public async Task<bool> TryAuthenticateAsync()
        {
            var refreshToken = GetRefreshToken();
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                IsAuthenticated = false;
                return false;
            }

            var session = new AccountSession
            {
                ClientId = SecurityHelper.ToUnsecureString(_clientId),
                RefreshToken = SecurityHelper.ToUnsecureString(SecurityHelper.DecryptString(refreshToken))
            };

            var msaAuthProvider = new MsaAuthenticationProvider(SecurityHelper.ToUnsecureString(_clientId), "https://login.live.com/oauth20_desktop.srf", _scope)
            {
                CurrentAccountSession = session
            };

            try
            {
                var httpProvider = new HttpProvider(new Serializer());
                httpProvider.OverallTimeout = TimeSpan.FromMinutes(20);

                _client = new OneDriveClient("https://api.onedrive.com/v1.0", msaAuthProvider, httpProvider);
                await msaAuthProvider.AuthenticateUserAsync();

                IsAuthenticated = msaAuthProvider.IsAuthenticated;
                TokenProvider.SetToken("RefreshToken", SecurityHelper.EncryptString(SecurityHelper.ToSecureString(msaAuthProvider.CurrentAccountSession.RefreshToken)));
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
            var authenticationUriString = $"https://login.live.com/oauth20_authorize.srf?" + $"client_id={SecurityHelper.ToUnsecureString(_clientId)}&redirect_uri={Uri.EscapeDataString(SecurityHelper.ToUnsecureString(_redirectUri))}&response_type=code&display=touch&scope={Uri.EscapeDataString(string.Join(" ", _scope))}";

            var authenticationResult = await authentication.AuthenticateAsync(authenticationUriString, SecurityHelper.ToUnsecureString(_redirectUri));
            if (authenticationResult.IsCanceled)
            {
                IsAuthenticated = false;
                return false;
            }

            try
            {
                var code = authenticationResult.RedirectedUri?.ToString().Split('=').Last();

                using (var handler = new HttpClientHandler())
                {
                    handler.ClientCertificateOptions = ClientCertificateOption.Automatic;
                    handler.UseDefaultCredentials = true;
                    handler.Credentials = System.Net.CredentialCache.DefaultCredentials;

                    using (var httpProvider = new HttpProvider(handler, true))
                    {
                        var createMessage = new HttpRequestMessage(HttpMethod.Post, "https://login.live.com/oauth20_token.srf")
                        {
                            Content = new StringContent($"client_id={SecurityHelper.ToUnsecureString(_clientId)}&code={code}&grant_type=authorization_code&redirect_uri={SecurityHelper.ToUnsecureString(_redirectUri)}", Encoding.UTF8, "application/x-www-form-urlencoded")
                        };

                        var response = await httpProvider.SendAsync(createMessage);

                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            var json = JObject.Parse(await response.Content.ReadAsStringAsync());
                            var refreshToken = json["refresh_token"]?.ToString();
                            TokenProvider.SetToken("RefreshToken", SecurityHelper.EncryptString(SecurityHelper.ToSecureString(refreshToken)));
                            await TryAuthenticateAsync();
                        }
                        else
                        {
                            IsAuthenticated = false;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Instance.Error(exception);
                IsAuthenticated = false;
            }

            return IsAuthenticated;
        }

        /// <inheritdoc/>
        public async Task SignOutAsync()
        {
            var signOutAsync = GetAuthenticationProvider()?.SignOutAsync();
            if (signOutAsync != null)
            {
                await signOutAsync;
            }

            TokenProvider.SetToken("RefreshToken", string.Empty);
            IsAuthenticated = false;
            Logger.Instance.Information($"User signed out from {CloudServiceName}.");
        }

        /// <inheritdoc/>
        public async Task<string> GetUserNameAsync()
        {
            ThrowIfNotConnected();
            var json = await GetUserInformation();
            if (json != null)
            {
                return json["name"].ToString();
            }

            return string.Empty;
        }

        /// <inheritdoc/>
        public async Task<string> GetUserIdAsync()
        {
            ThrowIfNotConnected();
            var json = await GetUserInformation();
            if (json != null)
            {
                return json["id"].ToString();
            }

            return string.Empty;
        }

        /// <inheritdoc/>
        public async Task<CloudFolder> GetAppFolderAsync()
        {
            ThrowIfNotConnected();

            var folder = await GetAppFolderItem().Request().GetAsync();
            var folderPath = "/drive/root:/Applications";

            if (folder.ParentReference != null)
            {
                folderPath = folder.ParentReference.Path;
            }

            IItemChildrenCollectionPage listFolder = null;
            var files = new List<CloudFile>();

            do
            {
                if (listFolder == null)
                {
                    listFolder = await _client.Drive.Items[folder.Id].Children.Request().GetAsync();
                }
                else
                {
                    listFolder = await listFolder.NextPageRequest.GetAsync();
                }

                foreach (var file in listFolder.CurrentPage.Where(entry => entry.File != null))
                {
                    files.Add(new CloudFile
                    {
                        Name = file.Name,
                        FullPath = $"{file.ParentReference.Path}/{file.Name}",
                        LastModificationUtcDate = file.LastModifiedDateTime.GetValueOrDefault().DateTime
                    });
                }
            } while (listFolder.NextPageRequest != null);

            return new CloudFolder
            {
                Name = folder.Name,
                Size = folder.Size,
                FullPath = $"{folderPath}/{folder.Name}/",
                Files = files
            };
        }

        /// <inheritdoc/>
        public async Task<CloudFile> UploadFileAsync(Stream baseStream, string remotePath)
        {
            Logger.Instance.Information($"Uploading '{remotePath}' to {CloudServiceName}. Data size : {baseStream.Length} bytes.");
            ThrowIfNotConnected();

            var fileInfo = await _client.ItemWithPath(remotePath).Content.Request().PutAsync<Item>(baseStream);
            Logger.Instance.Information($"'{remotePath}' uploaded.");
            return new CloudFile
            {
                Name = fileInfo.Name,
                FullPath = $"{fileInfo.ParentReference.Path}/{fileInfo.Name}",
                LastModificationUtcDate = fileInfo.LastModifiedDateTime.GetValueOrDefault().UtcDateTime
            };
        }

        /// <inheritdoc/>
        public async Task DownloadFileAsync(string remotePath, Stream targetStream)
        {
            Logger.Instance.Information($"Downloading '{remotePath}' to {CloudServiceName}.");
            ThrowIfNotConnected();

            using (var stream = await _client.ItemWithPath(remotePath).Content.Request().GetAsync())
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

            await _client.ItemWithPath(remotePath).Request().DeleteAsync();

            Logger.Instance.Information($"'{remotePath}' deleted.");
        }

        /// <summary>
        /// Retrieves the refresh token for the OneDrive account.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the token</returns>
        private string GetRefreshToken()
        {
            return TokenProvider.GetToken("RefreshToken");
        }

        /// <summary>
        /// Retrieves the access token for the OneDrive account.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the token</returns>
        private string GetAccessToken()
        {
            ThrowIfNotConnected();
            return GetAuthenticationProvider().CurrentAccountSession.AccessToken;
        }

        /// <summary>
        /// Retrieves the <see cref="MsaAuthenticationProvider"/> used to authenticate with a token.
        /// </summary>
        /// <returns>The <see cref="MsaAuthenticationProvider"/></returns>
        private MsaAuthenticationProvider GetAuthenticationProvider()
        {
            return _client?.AuthenticationProvider as MsaAuthenticationProvider;
        }

        /// <summary>
        /// Retrieves the OneDrive application folder item.
        /// </summary>
        /// <returns><see cref="IItemRequestBuilder"/></returns>
        private IItemRequestBuilder GetAppFolderItem()
        {
            ThrowIfNotConnected();
            return _client.Drive.Special.AppRoot;
        }

        /// <summary>
        /// Retrieves a json that contains information about the user.
        /// </summary>
        /// <returns>A <see cref="JObject"/> that contains information about user.</returns>
        private async Task<JObject> GetUserInformation()
        {
            var accessToken = GetAccessToken();
            var uri = new Uri($"https://apis.live.net/v5.0/me?access_token={accessToken}");

            using (var httpClient = new HttpClient())
            {
                var result = await httpClient.GetAsync(uri);
                var jsonUserInfo = await result.Content.ReadAsStringAsync();

                if (jsonUserInfo != null)
                {
                    return JObject.Parse(jsonUserInfo);
                }
            }

            return null;
        }

        /// <summary>
        /// Throw an exception if the user is not authenticated.
        /// </summary>
        private void ThrowIfNotConnected()
        {
            if (!IsAuthenticated || _client == null)
            {
                Logger.Instance.Warning($"The user is not authenticated to {CloudServiceName}.");
                throw new NotAuthenticatedException();
            }
        }

        #endregion
    }
}
