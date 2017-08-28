using System;
using ClipboardZanager.Shared.Services;

namespace ClipboardZanager.Tests.Mocks
{
    class ServiceSettingProviderMock : IServiceSettingProvider
    {
        public T GetSetting<T>(string settingName)
        {
            throw new NotImplementedException();
        }

        public void SetSetting(string settingName, object value)
        {
            throw new NotImplementedException();
        }
    }
}
