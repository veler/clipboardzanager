using System.Diagnostics;
using System.Runtime.CompilerServices;
using ClipboardZanager.Shared.Core;

namespace ClipboardZanager.Shared.Logs
{
    /// <summary>
    /// Represents a special kind of log designed to get the execution duration of a block of code.
    /// </summary>
    public sealed class TimedLog : IPausable
    {
        #region Fields

        private readonly Logger _logger;
        private readonly Stopwatch _stopwatch;
        private readonly string _message;
        private readonly string _callerName;

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="TimedLog"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="Logger"/> associated to this new instance.</param>
        /// <param name="message">The message associated to the log.</param>
        /// <param name="callerName">(optional) The name of the caller member.</param>
        public TimedLog(Logger logger, string message, [CallerMemberName] string callerName = null)
        {
            Requires.NotNull(logger, nameof(logger));
            Requires.NotNullOrWhiteSpace(message, nameof(message));
            Requires.NotNullOrWhiteSpace(callerName, nameof(callerName));

            _logger = logger;
            _message = message;
            _callerName = callerName;

            _stopwatch = new Stopwatch();
            _stopwatch.Start();

            _logger.Information($"{_message} : stopwatch started !", _callerName);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Pause the stopwatch
        /// </summary>
        public void Pause()
        {
            _stopwatch.Stop();
        }

        /// <summary>
        /// Resume the execution of the stopwatch
        /// </summary>
        public void Resume()
        {
            _stopwatch.Start();
        }

        /// <summary>
        /// Stop the stopwatch and write in the log the duration.
        /// </summary>
        public void Dispose()
        {
            _stopwatch.Stop();
            _logger.Information(ToString(), _callerName);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{_message} : {_stopwatch.Elapsed.ToString(@"hh\:mm\:ss\:fff")}";
        }

        #endregion
    }
}
