using System;

namespace ClipboardZanager.Shared.CloudStorage
{
    /// <summary>
    /// Represents the result of a <see cref="ICloudAuthentication"/>.
    /// </summary>
    public class AuthenticationResult
    {
        #region Properties

        /// <summary>
        /// Gets a value the defines whether the operation has been canceled or not.
        /// </summary>
        public bool IsCanceled { get; }

        /// <summary>
        /// Gets the <see cref="Uri"/> returned after an authentication tentative.
        /// </summary>
        public Uri RedirectedUri { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize the instance of the <see cref="AuthenticationResult"/> class.
        /// </summary>
        /// <param name="isCanceled">Defines whether the operation has been canceled or not.</param>
        /// <param name="redirectedUri">The <see cref="Uri"/> returned after an authentication tentative.</param>
        public AuthenticationResult(bool isCanceled, Uri redirectedUri)
        {
            IsCanceled = isCanceled;
            RedirectedUri = redirectedUri;
        }

        #endregion
    }
}
