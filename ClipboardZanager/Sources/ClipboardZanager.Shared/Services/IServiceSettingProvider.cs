namespace ClipboardZanager.Shared.Services
{
    /// <summary>
    /// Provides a set of functions designed to provide application settings for a <see cref="IService"/>.
    /// </summary>
    public interface IServiceSettingProvider
    {
        /// <summary>
        /// Returns the specified setting.
        /// </summary>
        /// <typeparam name="T">The type of the setting's value</typeparam>
        /// <param name="settingName">The setting's name to get.</param>
        /// <returns>The setting corresponding to the given name.</returns>
        T GetSetting<T>(string settingName);

        /// <summary>
        /// Set the specified setting.
        /// </summary>
        /// <param name="settingName">The setting's name to set.</param>
        /// <param name="value">The setting.</param>
        void SetSetting(string settingName, object value);
    }
}
