namespace ClipboardZanager.Core.Desktop.Enums
{
    internal enum DataEntryStatus
    {
        /// <summary>
        /// The data has been added locally and has not been synchronized yet.
        /// </summary>
        Added,

        /// <summary>
        /// The data has been synchronized with the cloud and did not changed since the last synchronization.
        /// </summary>
        DidNotChanged,

        /// <summary>
        /// The data has been removed locally and has not been synchronized yet.
        /// </summary>
        Deleted
    }
}
