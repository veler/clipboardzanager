namespace ClipboardZanager.Shared.Services
{
    /// <summary>
    /// Provides a set of functions and properties that represents a service.
    /// </summary>
    public interface IService
    {
        /// <summary>
        /// Initialize the service.
        /// </summary>
        /// <param name="settingProvider">An object that provides the access to the settings of the application.</param>
        void Initialize(IServiceSettingProvider settingProvider);

        /// <summary>
        /// Reset the state of the service. The service is considered as not initialized.
        /// </summary>
        void Reset();
    }
}
