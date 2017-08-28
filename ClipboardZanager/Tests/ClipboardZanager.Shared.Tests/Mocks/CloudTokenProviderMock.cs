using System.Collections.Generic;
using ClipboardZanager.Shared.CloudStorage;

namespace ClipboardZanager.Shared.Tests.Mocks
{
    public class CloudTokenProviderMock : ICloudTokenProvider
    {
        public string GetToken(string tokenName)
        {
            if (tokenName == "MyToken")
            {
                return "123456789abc";
            }

            throw new KeyNotFoundException();
        }

        public void SetToken(string tokenName, string value)
        {
            if (tokenName != "MyToken")
            {
                throw new KeyNotFoundException();
            }
        }
    }
}
