using System.Text;
using ClipboardZanager.Shared.Logs;

namespace ClipboardZanager.Core.Desktop.Tests.Mocks
{
    internal class LogMock : ILogSession
    {
        private StringBuilder _logs;

        public void Persist(StringBuilder logs)
        {
            _logs.Append(logs);
        }

        public void SessionStarted()
        {
            _logs = new StringBuilder();
        }

        public void SessionStopped()
        {
            _logs.Clear();
        }

        public string GetFatalErrorAdditionalInfo()
        {
            return "Additional info...";
        }

        internal StringBuilder GetLogs()
        {
            return _logs;
        }
    }
}
