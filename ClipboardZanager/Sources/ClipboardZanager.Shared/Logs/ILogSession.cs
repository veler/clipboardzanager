using System.Text;

namespace ClipboardZanager.Shared.Logs
{
    /// <summary>
    /// Provides a set of functions designed to perform action when a log session starts, stop or wants to save the logs.
    /// </summary>
    public interface ILogSession
    {
        /// <summary>
        /// Called when a new instance of a <see cref="Logger"/> in created.
        /// </summary>
        void SessionStarted();

        /// <summary>
        /// Called when an instance of <see cref="Logger"/> is disposed or if a <see cref="LogType.Fatal"/> log is added, after flushing it.
        /// </summary>
        void SessionStopped();

        /// <summary>
        /// Must persist the data from the log
        /// </summary>
        void Persist(StringBuilder logs);

        /// <summary>
        /// Returns a set of additional information like the operating system or the executable version when a fatal error occur.
        /// </summary>
        /// <returns>Some additional information.</returns>
        string GetFatalErrorAdditionalInfo();
    }
}
