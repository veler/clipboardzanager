using System.Threading.Tasks;

namespace ClipboardZanager.Shared.CloudStorage
{
    /// <summary>
    /// Provides a set of functions designed to manage the authentication to a cloud service.
    /// </summary>
    public interface ICloudAuthentication
    {
        /// <summary>
        /// Try to authenticate the user and wait for authentication completed.
        /// </summary>
        /// <param name="authenticationUri">The authentication page Uri.</param>
        /// <param name="redirectUri">The expected Uri that we must detect after the authentication. It must be different from the <see cref="authenticationUri"/>.</param>
        /// <returns>Returns a <see cref="AuthenticationResult"/> that describes the result of the authentication.</returns>
        Task<AuthenticationResult> AuthenticateAsync(string authenticationUri, string redirectUri);
    }
}
