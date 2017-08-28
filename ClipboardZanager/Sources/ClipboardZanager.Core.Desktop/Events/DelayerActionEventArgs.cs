namespace ClipboardZanager.Core.Desktop.Events
{
    /// <summary>
    /// Represents the arguments of an event <see cref="Delayer.Action"/>
    /// </summary>
    internal sealed class DelayerActionEventArgs<T> where T : class
    {
        #region Properties

        /// <summary>
        /// Gets the data linked to the event.
        /// </summary>
        internal T Data { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="DelayerActionEventArgs"/> class
        /// </summary>
        internal DelayerActionEventArgs()
            : this(null)
        {
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="DelayerActionEventArgs"/> class
        /// </summary>
        /// <param name="data">The data linked to the event.</param>
        internal DelayerActionEventArgs(T data)
        {
            Data = data;
        }

        #endregion
    }
}
