using System;
using System.Threading.Tasks;
using ClipboardZanager.Shared.CloudStorage;

namespace ClipboardZanager.Shared.Tests.Mocks
{
    internal class CloudAuthenticationMock : ICloudAuthentication
    {
        public Task<AuthenticationResult> AuthenticateAsync(string authenticationUri, string redirectUri)
        {
            return Task.Run(() =>
            {
                if (string.IsNullOrEmpty(authenticationUri))
                {
                    return new AuthenticationResult(true, new Uri(redirectUri));
                }

                return new AuthenticationResult(false, new Uri(redirectUri));
            });
        }
    }
}
