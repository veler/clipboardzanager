using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using ClipboardZanager.Shared.Core;

namespace ClipboardZanager.Core.Desktop.Models
{
    /// <summary>
    /// Provide information about a Windows Store App package
    /// </summary>
    internal sealed class WindowsStoreApp
    {
        #region Properties

        /// <summary>
        /// Gets the family name of the package
        /// </summary>
        internal string FamilyName { get; }

        /// <summary>
        /// Gets the <see cref="Package"/>
        /// </summary>
        internal Package Package { get; }

        /// <summary>
        /// Gets the <see cref="AppListEntry"/>
        /// </summary>
        internal AppListEntry AppDetails { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="WindowsStoreApp"/> class
        /// </summary>
        /// <param name="package">the <see cref="Package"/></param>
        internal WindowsStoreApp(Package package)
        {
            Requires.NotNull(package, nameof(package));
            Package = package;
            FamilyName = package.Id.FamilyName;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initialize asynchronously the object. Before this method is called, <see cref="AppDetails"/> is not set.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        internal Task InitializeAsync()
        {
            var task = Package.GetAppListEntriesAsync().AsTask();
            task.Wait();
            var appEntries = task.Result;
            AppDetails = appEntries.FirstOrDefault();

            return Task.CompletedTask;
        }

        #endregion
    }
}
