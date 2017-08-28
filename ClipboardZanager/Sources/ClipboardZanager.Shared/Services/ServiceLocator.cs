using System.Collections.Generic;
using System.Linq;
using ClipboardZanager.Shared.Core;

namespace ClipboardZanager.Shared.Services
{
    /// <summary>
    /// Provides a set of functions designed to manage the services.
    /// </summary>
    public static class ServiceLocator
    {
        #region Fields

        private static readonly List<IService> _services = new List<IService>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the provider of settings.
        /// </summary>
        public static IServiceSettingProvider SettingProvider { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Get an instance of the specified service.
        /// </summary>
        /// <returns>The current instance of the service</returns>
        public static T GetService<T>() where T : IService, new()
        {
            Requires.NotNull(SettingProvider, nameof(SettingProvider));

            T service;
            var serviceFound = _services.OfType<T>().ToList();

            if (serviceFound.Any())
            {
                service = serviceFound.Single();
            }
            else
            {
                service = new T();
                Requires.NotNull(service, nameof(service));

                _services.Add(service);
                service.Initialize(SettingProvider);
            }

            return service;
        }

        /// <summary>
        /// Reset the state of all services. This method must be used in the unit test.
        /// </summary>
        public static void ResetAll()
        {
            foreach (var service in _services)
            {
                service.Reset();
            }
        }

        #endregion
    }
}
