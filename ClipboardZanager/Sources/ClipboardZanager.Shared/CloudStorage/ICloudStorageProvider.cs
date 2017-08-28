using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ClipboardZanager.Shared.CloudStorage
{
    /// <summary>
    /// Provides a set of properties and methods used to interact with a cloud storage service.
    /// </summary>
    public interface ICloudStorageProvider
    {
        /// <summary>
        /// Gets the name of the cloud service.
        /// </summary>
        string CloudServiceName { get; }

        /// <summary>
        /// Gets a <see cref="ControlTemplate"/> that represents the full path to the icon of the cloud service
        /// </summary>
        ControlTemplate CloudServiceIcon { get; }

        /// <summary>
        /// Gets a value that defines whether the user is authenticated.
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// Gets a value that defines whether credential exists for this provider in the application.
        /// </summary>
        bool CredentialExists { get; }

        /// <summary>
        /// Gets the <see cref="ICloudTokenProvider"/> associated to this storage provider.
        /// </summary>
        ICloudTokenProvider TokenProvider { get; }

        /// <summary>
        /// Try to authenticate to the cloud service with the informations in cache, without any UI.
        /// </summary>
        /// <returns>True is the user is authenticated.</returns>
        Task<bool> TryAuthenticateAsync();

        /// <summary>
        /// Try to authenticate to the cloud service without any data from the cache. Usually, the authentication need to access to display an authentication page in the <see cref="ICloudAuthentication"/>.
        /// </summary>  
        /// <param name="authentication">An object implementing <see cref="ICloudAuthentication"/> used to perform the authentication.</param>
        /// <returns>True is the user is authenticated.</returns>
        Task<bool> TryAuthenticateWithUiAsync(ICloudAuthentication authentication);

        /// <summary>
        /// Sign the user out.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SignOutAsync();

        /// <summary>
        /// Gets the display name of the user.
        /// </summary>
        /// <returns>The display name of the user.</returns>
        Task<string> GetUserNameAsync();

        /// <summary>
        /// Gets the unique ID of the user.
        /// </summary>
        /// <returns>The ID of the user.</returns>
        Task<string> GetUserIdAsync();

        /// <summary>
        /// Retrieves informations about the application folder.
        /// </summary>
        /// <returns>A <see cref="CloudFolder"/> that contains informations about the application folder.</returns>
        Task<CloudFolder> GetAppFolderAsync();

        /// <summary>
        /// Upload a file to the specified remote path and overwrite if it already exists.
        /// </summary>
        /// <param name="baseStream">The stream that contains the data to upload.</param>
        /// <param name="remotePath">The destination path on the could service.</param>
        /// <returns>A <see cref="CloudFile"/> that contains informations about the uploaded file.</returns>
        Task<CloudFile> UploadFileAsync(Stream baseStream, string remotePath);

        /// <summary>
        /// Download a file to the specified local path and overwrite if it already exists.
        /// </summary>
        /// <param name="remotePath">The remote file to download.</param>
        /// <param name="targetStream">The stream where the data will be saved.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task DownloadFileAsync(string remotePath, Stream targetStream);

        /// <summary>
        /// Delete a file on the server.
        /// </summary>
        /// <param name="remotePath">The remote file to delete.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task DeleteFileAsync(string remotePath);
    }
}
