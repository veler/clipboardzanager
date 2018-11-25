using System;
using System.Collections;
using System.Collections.Generic;
using ClipboardZanager.Core.Desktop.Models;
using ClipboardZanager.Shared.CloudStorage;
using ClipboardZanager.Shared.Logs;
using ClipboardZanager.Shared.Services;

namespace ClipboardZanager.Core.Desktop.Tests.Mocks
{
    class ServiceSettingProviderMock : IServiceSettingProvider
    {
        internal bool KeepDataAfterReboot;
        internal bool AvoidCreditCard;
        internal bool DisablePasswordAndCreditCardSync;
        internal bool DisplayUriTitle;
        internal int MaxDataToKeep;
        internal int DateExpireLimit;
        internal ArrayList KeepDataTypes;
        internal List<IgnoredApplication> IgnoredApplications;

        public T GetSetting<T>(string settingName)
        {
            object value;
            switch (settingName)
            {
                case "DropBoxAppKey":
                    value = "foo";
                    break;

                case "OneDriveClientId":
                    value = "bar";
                    break;

                case "SynchronizationInterval":
                    value = 10;
                    break;

                case "KeepDataAfterReboot":
                    value = KeepDataAfterReboot;
                    break;

                case "AvoidCreditCard":
                    value = AvoidCreditCard;
                    break;

                case "MaxDataToKeep":
                    value = MaxDataToKeep;
                    break;

                case "DateExpireLimit":
                    value = DateExpireLimit;
                    break;

                case "KeepDataTypes":
                    value = KeepDataTypes;
                    break;

                case "IgnoredApplications":
                    value = IgnoredApplications;
                    break;

                case "DataMigrationRequired":
                    value = false;
                    break;

                case "CurrentVersion":
                    value = "";
                    break;

                case "CloudStorageProviders":
                    value = new List<ICloudStorageProvider> {
                                                                new CloudStorageProviderMock(),
                                                                new Shared.Tests.Mocks.CloudStorageProviderMock()
                                                            };
                    break;

                case "DisablePasswordAndCreditCardSync":
                    value = DisablePasswordAndCreditCardSync;
                    break;

                case "DisplayUriTitle":
                    value = DisplayUriTitle;
                    break;

                default:
                    Logger.Instance.Fatal(new KeyNotFoundException($"{settingName} not found."));
                    return default(T);
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }

        public void SetSetting(string settingName, object value)
        {
            throw new NotImplementedException();
        }

        public void ResetSettings()
        {
            KeepDataAfterReboot = true;
            AvoidCreditCard = true;
            DisablePasswordAndCreditCardSync = true;
            DisplayUriTitle = true;
            MaxDataToKeep = 25;
            DateExpireLimit = 30;
            
            IgnoredApplications = new List<IgnoredApplication>();

            KeepDataTypes = new ArrayList
                            {
                                8, // SupportedDataType.Text
                                7, // SupportedDataType.Files
                                6,  // SupportedDataType.Image
                                5,
                                4,
                                3,
                                2,
                                1,
                                0
                            };
        }
    }
}
