using ClipboardZanager.Shared.Logs;
using System.Text;

namespace ClipboardZanager.Shared.Tests.Mocks
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
