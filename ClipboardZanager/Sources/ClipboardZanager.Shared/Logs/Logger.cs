using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using ClipboardZanager.Shared.Core;

namespace ClipboardZanager.Shared.Logs
{
    /// <summary>
    /// Provides a set of functions designed to log information about how the application run.
    /// </summary>
    public sealed class Logger : IDisposable
    {
        #region Fields

        private static ILogSession _instanceLog;
        private static Logger _instance;

        private readonly ILogSession _logSession;
        private readonly StringBuilder _logs;
        private bool _fatalOccured;
        private int _logsCountSinceLastFlush;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="ILogSession"/> for the default instance of the <see cref="Logger"/>.
        /// </summary>
        public static ILogSession InstanceLogSession
        {
            get
            {
                return _instanceLog;
            }
            set
            {
                if (_instance != null)
                {
                    throw new InvalidOperationException($"Cannot change the value once '{nameof(_instance)}' has been initialized.");
                }

                _instanceLog = value;
            }
        }

        /// <summary>
        /// Gets the default instance of the <see cref="Logger"/>.
        /// </summary>
        public static Logger Instance
        {
            get
            {
                if (_instanceLog == null)
                {
                    throw new InvalidOperationException($"The property '{nameof(InstanceLogSession)}' should be set before accessing this property.");
                }

                if (_instance == null)
                {
                    _instance = new Logger(InstanceLogSession);
                }

                return _instance;
            }
        }

        /// <summary>
        /// Gets or sets the number of logs the program must keep in memory before an automatic flush.
        /// </summary>
        public int FlushInterval { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="Logger"/> class.
        /// </summary>
        /// <param name="logSession">The <see cref="ILogSession"/> used to persist the logs.</param>
        public Logger(ILogSession logSession)
        {
            Requires.NotNull(logSession, nameof(logSession));

            FlushInterval = 200;

            _logs = new StringBuilder();
            _logSession = logSession;
            _logSession.SessionStarted();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Times a block of code. The stopwatch will stop when the returned <see cref="TimedLog"/> will be disposed. It is recommanded to use this method with a Using block.
        /// </summary>
        /// <param name="message">The message associated to the log.</param>
        /// <param name="callerName">(optional) The name of the caller member.</param>
        /// <returns>Returns a <see cref="TimedLog"/> that times the execution.</returns>
        public TimedLog Stopwatch(string message, [CallerMemberName] string callerName = null)
        {
            return new TimedLog(this, message, callerName);
        }

        /// <summary>
        /// Add an <see cref="LogType.Information"/> log.
        /// </summary>
        /// <param name="message">The message associated to the log.</param>
        /// <param name="callerName">(optional) The name of the caller member.</param>
        public void Information(string message, [CallerMemberName] string callerName = null)
        {
            Log(LogType.Information, message, callerName);
        }

        /// <summary>
        /// Add an <see cref="LogType.Debug"/> log. It will be performed only if the debugger is attached, not in production mode.
        /// </summary>
        /// <param name="message">The message associated to the log.</param>
        /// <param name="callerName">(optional) The name of the caller member.</param>
        public void Debug(string message, [CallerMemberName] string callerName = null)
        {
            if (Debugger.IsAttached)
            {
                Log(LogType.Debug, message, callerName);
            }
        }

        /// <summary>
        /// Add an <see cref="LogType.Warning"/> log.
        /// </summary>
        /// <param name="message">The message associated to the log.</param>
        /// <param name="callerName">(optional) The name of the caller member.</param>
        public void Warning(string message, [CallerMemberName] string callerName = null)
        {
            Log(LogType.Warning, message, callerName);
        }

        /// <summary>
        /// Add an <see cref="LogType.Error"/> log.
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/> associated to the error.</param>
        /// <param name="callerName">(optional) The name of the caller member.</param>
        public void Error(Exception exception, [CallerMemberName] string callerName = null)
        {
            Log(LogType.Error, exception.Message, callerName);
        }

        /// <summary>
        /// Add a <see cref="LogType.Fatal"/> log, flush the logs, stop the session and throw the given exception.
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/> associated to the fatal error.</param>
        /// <param name="callerName">(optional) The name of the caller member.</param>
        /// <param name="sourceFilePath">(optional) The path of the file who call this method.</param>
        /// <param name="sourceLineNumber">(optional) The line number of the fatal error.</param>
        public void Fatal(Exception exception, [CallerMemberName] string callerName = null, [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            Fatal(string.Empty, exception, callerName, sourceFilePath, sourceLineNumber);
        }

        /// <summary>
        /// Add a <see cref="LogType.Fatal"/> log, flush the logs, stop the session and throw the given exception.
        /// </summary>
        /// <param name="message">The message associated to the log.</param>
        /// <param name="exception">The <see cref="Exception"/> associated to the fatal error.</param>
        /// <param name="callerName">(optional) The name of the caller member.</param>
        /// <param name="sourceFilePath">(optional) The path of the file who call this method.</param>
        /// <param name="sourceLineNumber">(optional) The line number of the fatal error.</param>
        public void Fatal(string message, Exception exception, [CallerMemberName] string callerName = null, [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            Log(LogType.Fatal, message, $"{callerName} in {Path.GetFileName(sourceFilePath)}, line {sourceLineNumber}");
            Log(LogType.Fatal, _logSession.GetFatalErrorAdditionalInfo(), "Additional Information");
            Flush();
            _logSession.SessionStopped();
            _fatalOccured = true;

            if (exception != null)
            {
                throw exception;
            }
        }

        /// <summary>
        /// Persists the logs and clear the buffer.
        /// </summary>
        public void Flush()
        {
            _logSession.Persist(_logs);
            _logs.Clear();
            _logsCountSinceLastFlush = 0;
        }

        /// <summary>
        /// Persist the logs and stop the session.
        /// </summary>
        public void Dispose()
        {
            if (!_fatalOccured)
            {
                Flush();
                _logSession.SessionStopped();
            }
        }

        /// <summary>
        /// Add a log to the buffer in memory.
        /// </summary>
        /// <param name="type">The type of log.</param>
        /// <param name="message">The message associated to the log.</param>
        /// <param name="callerName">(optional) The name of the caller member.</param>
        private void Log(LogType type, string message, string callerName)
        {
            if (_fatalOccured)
            {
                throw new OperationCanceledException("Unable to log something when the logger did a fatal.");
            }

            var fullMessage = $"[{type}] [{callerName}] [{DateTime.Now.ToString("dd'/'MM'/'yyyy HH:mm:ss")}] {message}";
            _logs.AppendLine(fullMessage);

            if (Debugger.IsAttached)
            {
                System.Diagnostics.Debug.WriteLine(fullMessage);
            }

            _logsCountSinceLastFlush++;
            if (_logsCountSinceLastFlush > FlushInterval)
            {
                Flush();
            }
        }

        #endregion
    }
}
