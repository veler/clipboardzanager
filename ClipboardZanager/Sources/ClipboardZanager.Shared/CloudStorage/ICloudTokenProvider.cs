namespace ClipboardZanager.Shared.CloudStorage
{
    /// <summary>
    /// Provides a set of functions designed to provide tokens for a <see cref="ICloudStorageProvider"/>.
    /// </summary>
    public interface ICloudTokenProvider
    {
        /// <summary>
        /// Returns the specified token.
        /// </summary>
        /// <param name="tokenName">The token's name to get.</param>
        /// <returns>The token corresponding to the given name.</returns>
        string GetToken(string tokenName);

        /// <summary>
        /// Set the specified token.
        /// </summary>
        /// <param name="tokenName">The token's name to set.</param>
        /// <param name="value">The token.</param>
        void SetToken(string tokenName, string value);
    }
}
