using System.Collections.Generic;
using ClipboardZanager.Properties;
using ClipboardZanager.Shared.CloudStorage;
using ClipboardZanager.Shared.Logs;

namespace ClipboardZanager.ComponentModel.CloudStorage
{
    /// <summary>
    /// Provides a set of functions designed to provide tokens for a <see cref="OneDriveProvider"/>.
    /// </summary>
    internal sealed class OneDriveTokenProvider : ICloudTokenProvider
    {
        /// <inheritdoc/>
        public string GetToken(string tokenName)
        {
            switch (tokenName)
            {
                case "ClientID":
                    return Settings.Default.OneDriveClientId;

                case "RedirectUri":
                    return Settings.Default.OneDriveRedirectUri;

                case "RefreshToken":
                    return Settings.Default.OneDriveRefreshToken;

                default:
                    Logger.Instance.Error(new KeyNotFoundException($"{tokenName} not found."));
                    return string.Empty;
            }
        }

        /// <inheritdoc/>
        public void SetToken(string tokenName, string value)
        {
            switch (tokenName)
            {
                case "RefreshToken":
                    Settings.Default.OneDriveRefreshToken = value;
                    break;

                default:
                    Logger.Instance.Error(new KeyNotFoundException($"{tokenName} not found."));
                    break;
            }

            Settings.Default.Save();
        }
    }
}
