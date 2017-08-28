using System;

namespace ClipboardZanager.Shared.Events
{
    public sealed class AsyncBackgroundWorkerEndedEventArgs : EventArgs
    {
        public bool IsCanceled { get; }

        public Exception Exception { get; }

        public AsyncBackgroundWorkerEndedEventArgs(bool isCanceled, Exception exception)
        {
            IsCanceled = isCanceled;
            Exception = exception;
        }
    }
}
