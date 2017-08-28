using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using ClipboardZanager.Shared.Events;

namespace ClipboardZanager.Shared.Core.Worker
{
    /// <summary>
    /// Provides a cancellable background task that can report a progress of its state.
    /// </summary>
    public abstract class AsyncBackgroundWorker
    {
        #region Fields

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private Exception _exception;
        private bool _started;

        #endregion

        #region Events

        /// <summary>
        /// Raised when the worker start.
        /// </summary>
        public event EventHandler WorkStarted;

        /// <summary>
        /// Raised when the worker stopped (because of a cancellation, an exception or finished normally).
        /// </summary>
        public event EventHandler<AsyncBackgroundWorkerEndedEventArgs> WorkEnded;

        /// <summary>
        /// Raised when the <see cref="DoWork"/> method report a progress.
        /// </summary>
        public event EventHandler<AsyncBackgroundWorkerProgressEventArgs> ProgressChanged;

        #endregion

        #region Methods

        /// <summary>
        /// Perform the work of the <see cref="AsyncBackgroundWorker"/>.
        /// </summary>
        protected abstract void DoWork();

        /// <summary>
        /// Starts execution of a background operation.
        /// </summary>
        public void Start()
        {
            if (_started)
            {
                throw new InvalidOperationException("The AsyncBackgroundWorker is not in a valid state to be started. It may have already been started, executed, or canceled.");
            }

            _started = true;
            WorkStarted?.Invoke(this, new EventArgs());

            var task = Task.Factory.StartNew(DoWorkInternal);
            task.ContinueWith(DoWorkEnded, TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        /// Requests cancellation of a pending background operation.
        /// </summary>
        public void Cancel()
        {
            if (!_started)
            {
                throw new InvalidOperationException("The AsyncBackgroundWorker is not in a valid state to be cancelled. It may have not been started yet.");
            }

            _cancellationTokenSource.Cancel();
        }

        /// <summary>
        /// Throws a <see cref="OperationCanceledException"/> if this token has had cancellation requested.
        /// </summary>
        protected void ThrowIfCanceled()
        {
            _cancellationTokenSource.Token.ThrowIfCancellationRequested();
        }

        /// <summary>
        /// Raises the <see cref="ProgressChanged"/> event.
        /// </summary>
        /// <param name="percent">The percent of progress</param>
        protected void ReportProgress(int percent)
        {
            ReportProgress(percent, null);
        }

        /// <summary>
        /// Raises the <see cref="ProgressChanged"/> event.
        /// </summary>
        /// <param name="percent">The percent of progress</param>
        /// <param name="progressArgument">The argument of the progress</param>
        protected void ReportProgress(int percent, object progressArgument)
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                ProgressChanged?.Invoke(this, new AsyncBackgroundWorkerProgressEventArgs(percent, progressArgument));
            }, DispatcherPriority.ApplicationIdle);
        }

        /// <summary>
        /// Start the execution of a background operation and manage its potential exception.
        /// </summary>
        private void DoWorkInternal()
        {
            try
            {
                DoWork();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                _exception = exception;
            }
        }

        /// <summary>
        /// Raises the <see cref="WorkEnded"/> event.
        /// </summary>
        /// <param name="task"></param>
        private void DoWorkEnded(Task task)
        {
            WorkEnded?.Invoke(this, new AsyncBackgroundWorkerEndedEventArgs(_cancellationTokenSource.Token.IsCancellationRequested, _exception));
        }

        #endregion
    }
}
