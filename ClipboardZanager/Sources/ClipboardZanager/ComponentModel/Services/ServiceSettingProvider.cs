using ClipboardZanager.ComponentModel.CloudStorage;
using ClipboardZanager.Core.Desktop.CloudStorage;
using ClipboardZanager.Core.Desktop.Models;
using ClipboardZanager.Properties;
using ClipboardZanager.Shared.CloudStorage;
using ClipboardZanager.Shared.Logs;
using ClipboardZanager.Shared.Services;
using ClipboardZanager.Strings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ClipboardZanager.ComponentModel.Services
{
    /// <summary>
    /// Provides a set of functions designed to provide settings for a <see cref="IService"/>.
    /// </summary>
    internal sealed class ServiceSettingProvider : IServiceSettingProvider
    {
        /// <inheritdoc/>
        public T GetSetting<T>(string settingName)
        {
            object value;
            switch (settingName)
            {
                case "IgnoredApplications":
                    value = JsonConvert.DeserializeObject<List<IgnoredApplication>>(Settings.Default.IgnoredApplications);
                    break;

                case "AvoidCreditCard":
                    value = Settings.Default.AvoidCreditCard;
                    break;

                case "AvoidPasswords":
                    value = Settings.Default.AvoidPasswords;
                    break;

                case "DropBoxAppKey":
                    value = Settings.Default.DropBoxAppKey;
                    break;

                case "OneDriveClientId":
                    value = Settings.Default.OneDriveClientId;
                    break;

                case "KeepDataAfterReboot":
                    value = Settings.Default.KeepDataAfterReboot;
                    break;

                case "MaxDataToKeep":
                    value = Settings.Default.MaxDataToKeep;
                    break;

                case "DateExpireLimit":
                    value = Settings.Default.DateExpireLimit;
                    break;

                case "KeepDataTypes":
                    value = Settings.Default.KeepDataTypes;
                    break;

                case "AvoidMeteredConnection":
                    value = Settings.Default.AvoidMeteredConnection;
                    break;

                case "DataMigrationRequired":
                    value = Settings.Default.DataMigrationRequired;
                    break;

                case "CurrentVersion":
                    value = Settings.Default.CurrentVersion;
                    break;

                case "SynchronizationInterval":
                    value = Settings.Default.SynchronizationInterval;
                    break;

                case "CloudStorageProviders":
                    value = new List<ICloudStorageProvider> {
                                                                new DropBoxProvider(new DropBoxTokenProvider()),
                                                                new OneDriveProvider(new OneDriveTokenProvider())
                                                            };
                    break;

                default:
                    Logger.Instance.Error(new KeyNotFoundException($"{settingName} not found."));
                    return default(T);
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }

        /// <inheritdoc/>
        public void SetSetting(string settingName, object value)
        {
            switch (settingName)
            {
                case "DataMigrationRequired":
                    Settings.Default.DataMigrationRequired = (bool)value;
                    break;

                case "CurrentVersion":
                    Settings.Default.CurrentVersion = (string)value;
                    break;

                default:
                    Logger.Instance.Error(new KeyNotFoundException($"{settingName} not found."));
                    break;
            }

            Settings.Default.Save();
        }

        /// <summary>
        /// Save and apply the new settings.
        /// </summary>
        internal void SaveAndApplySettings()
        {
            Settings.Default.Save();
            Settings.Default.Reload();
            LanguageManager.GetInstance().SetCurrentCulture(new CultureInfo(Settings.Default.Language));
            Logger.Instance.Information($"The settings has been saved an reloaded.");
        }
    }
}
