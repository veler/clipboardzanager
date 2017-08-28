using System;
using ClipboardZanager.Core.Desktop.Services;

namespace ClipboardZanager.Core.Desktop.Events
{
    /// <summary>
    /// Represents the arguments of a <see cref="DataService.DataMigrationProgress"/>.
    /// </summary>
    internal sealed class DataMigrationProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets a value that defines whether the migration is completed.
        /// </summary>
        internal bool Completed { get; set; }

        /// <summary>
        /// Gets or sets a value that defines whether the migration failed.
        /// </summary>
        internal bool Failed { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates the progression in percent.
        /// </summary>
        internal int Percent { get; set; }

        /// <summary>
        /// Initialize a new instance of the <see cref="DataMigrationProgressEventArgs"/> class.
        /// </summary>
        /// <param name="percent">Indicates the progression in percent</param>
        /// <param name="completed">Defines whether the migration is completed.</param>
        /// <param name="failed">Defines whether the migration failed</param>
        internal DataMigrationProgressEventArgs(int percent, bool completed, bool failed)
        {
            Percent = percent;
            Completed = completed;
            Failed = failed;
        }
    }
}
