using ClipboardZanager.Shared.Services;

namespace ClipboardZanager.Tests.Mocks
{
    class ServiceMock : IService
    {
        public bool Reseted = true;

        public void Initialize(IServiceSettingProvider settingProvider)
        {
            Reseted = false;
        }

        public void Reset()
        {
            Reseted = true;
        }
    }
}
