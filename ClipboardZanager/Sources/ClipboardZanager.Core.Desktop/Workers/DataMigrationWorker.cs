using ClipboardZanager.Shared.Core.Worker;

namespace ClipboardZanager.Core.Desktop.Workers
{
    /// <summary>
    /// Provides a background operation that migrates the old data of the application.
    /// </summary>
    internal class DataMigrationWorker : AsyncBackgroundWorker
    {
        /// <inheritdoc/>
        protected override void DoWork()
        {
            ReportProgress(0);



            ReportProgress(100);
        }
    }
}
