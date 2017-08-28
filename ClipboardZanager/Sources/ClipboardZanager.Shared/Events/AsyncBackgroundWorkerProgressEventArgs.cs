using System;

namespace ClipboardZanager.Shared.Events
{
    public sealed class AsyncBackgroundWorkerProgressEventArgs : EventArgs
    {
        public int Percent { get; }

        public object ProgressArgument { get; }

        public AsyncBackgroundWorkerProgressEventArgs(int percent, object progressArgument)
        {
            Percent = percent;
            ProgressArgument = progressArgument;
        }
    }
}
