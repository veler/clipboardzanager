using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Shared.Logs;

namespace ClipboardZanager.Core.Desktop.IO
{
    /// <summary>
    /// Provides a set of functions designed to perform action when a log session starts, stop or wants to save the logs.
    /// </summary>
    internal sealed class LogSession : ILogSession
    {
        #region Fields

        private StreamWriter _stream;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the path to the file where the logs are saved.
        /// </summary>
        internal string LogFilePath { get; private set; }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public void SessionStarted()
        {
            var appFolder = CoreHelper.GetAppDataFolder();
            var file1 = Path.Combine(appFolder, "logs1");
            var file2 = Path.Combine(appFolder, "logs2");
            var file1LastWriteTime = new DateTime(1970, 1, 1);
            DateTime file2LastWriteTime;

            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }

            if (File.Exists(file1))
            {
                file1LastWriteTime = File.GetLastWriteTime(file1);
            }

            if (File.Exists(file2))
            {
                file2LastWriteTime = File.GetLastWriteTime(file2);
            }
            else
            {
                File.WriteAllText(file2, string.Empty);
                file2LastWriteTime = DateTime.Now;
            }

            if (file1LastWriteTime >= file2LastWriteTime)
            {
                LogFilePath = file2;
            }
            else
            {
                LogFilePath = file1;
            }

            try
            {
                if (File.Exists(LogFilePath))
                {
                    File.Delete(LogFilePath);
                }
            }
            catch
            {
            }

            _stream = File.AppendText(LogFilePath);
        }

        /// <inheritdoc/>
        public void SessionStopped()
        {
            _stream.Dispose();
        }

        /// <inheritdoc/>
        public void Persist(StringBuilder logs)
        {
            _stream.Write(logs.ToString());
            _stream.Flush();
        }

        /// <inheritdoc/>
        public string GetFatalErrorAdditionalInfo()
        {
            var result = new StringBuilder();
            result.AppendLine($"Assembly version : {CoreHelper.GetApplicationVersion()}");
            result.AppendLine($"Memory used : {Process.GetCurrentProcess().PrivateMemorySize64 / 1024} KB");
            result.AppendLine($"Operating system : {Environment.OSVersion.VersionString}");
            return result.ToString();
        }

        /// <summary>
        /// Returns the path to the log file that corresponds to the previous session.
        /// </summary>
        /// <returns>A <see cref="string"/> that corresponds to the previous session file.</returns>
        internal string GetPreviousSessionLogFilePath()
        {
            if (LogFilePath.EndsWith("logs1"))
            {
                return LogFilePath.Replace("logs1", "logs2");
            }

            return LogFilePath.Replace("logs2", "logs1");
        }

        #endregion
    }
}
