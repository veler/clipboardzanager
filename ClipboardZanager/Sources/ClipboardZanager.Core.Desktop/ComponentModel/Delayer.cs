using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using ClipboardZanager.Core.Desktop.Events;

namespace ClipboardZanager.Core.Desktop.ComponentModel
{
    /// <summary>
    /// Delay an action.
    /// </summary>
    internal class Delayer<T> where T : class
    {
        #region Fields

        private readonly bool _runAsync;
        private readonly DispatcherTimer _timer;

        private T _data;

        #endregion

        #region Events

        /// <summary>
        /// Raised when the delay passed and that the action must be performed.
        /// </summary>
        internal event Delegates.DelayerActionEventHandler<T> Action;

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="Delayer"/> class.
        /// </summary>
        /// <param name="timeSpan">The interval of time before the action will be executed.</param>
        /// <param name="runActionAsync">Determines whether the action must be ran asynchronously.</param>
        internal Delayer(TimeSpan timeSpan, bool runActionAsync = false)
        {
            _timer = new DispatcherTimer();
            _timer.Interval = timeSpan;
            _timer.Tick += Timer_Tick;
            _runAsync = runActionAsync;
        }

        #endregion

        #region Handled Methods

        private void Timer_Tick(object sender, object e)
        {
            _timer.Stop();

            if (!_runAsync)
            {
                Action?.Invoke(this, new DelayerActionEventArgs<T>(_data));
            }
            else
            {
                Task.Run(() =>
                {
                    Action?.Invoke(this, new DelayerActionEventArgs<T>(_data));
                });
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Call the action after a delay.
        /// </summary>
        internal void ResetAndTick()
        {
            ResetAndTick(null);
        }

        /// <summary>
        /// Call the action after a delay.
        /// </summary>
        /// <param name="data">The data to pass to the action.</param>
        internal void ResetAndTick(T data)
        {
            _data = data;
            _timer.Stop();
            _timer.Start();
        }

        /// <summary>
        /// Manually stop the delayer.
        /// </summary>
        internal void Stop()
        {
            _timer.Stop();
        }

        #endregion
    }
}
