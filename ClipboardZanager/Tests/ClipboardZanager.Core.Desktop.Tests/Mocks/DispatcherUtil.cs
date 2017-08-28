using System;
using System.Security.Permissions;
using System.Threading;
using System.Windows.Threading;

namespace ClipboardZanager.Core.Desktop.Tests.Mocks
{
    /// <summary>
    /// The dispatcher helper class.
    /// </summary>
    public static class DispatcherUtil
    {
        /// <summary>
        /// The stop execution callback.
        /// </summary>
        private static readonly EventHandler stop;

        /// <summary>
        /// Initializes static members of the <see cref="DispatcherUtil"/> class.
        /// </summary>
        static DispatcherUtil()
        {
            stop = (s, e) =>
            {
                ((DispatcherTimer)s).Stop();
                Dispatcher.ExitAllFrames();
            };
        }

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static void DoEvents()
        {
            var frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(ExitFrame), frame);
            Dispatcher.PushFrame(frame);
        }

        private static object ExitFrame(object frame)
        {
            ((DispatcherFrame)frame).Continue = false;
            return null;
        }

        /// <summary>
        /// Executes the on dispatcher thread.
        /// </summary>
        /// <param name="test">The test method.</param>
        /// <param name="milliSecondsToWait">The milliSeconds to wait.</param>
        public static void ExecuteOnDispatcherThread(Action test, int milliSecondsToWait = 5000)
        {
            Action dispatch = () =>
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal, test);
                StartTimer(milliSecondsToWait, Dispatcher.CurrentDispatcher);

                Dispatcher.Run();
            };

            ExecuteOnNewThread(dispatch);
        }

        /// <summary>
        /// Executes the on dispatcher thread.
        /// </summary>
        /// <typeparam name="T">The test result type.</typeparam>
        /// <param name="test">The test to execute.</param>
        /// <param name="milliSecondsToWait">The milliSeconds to wait.</param>
        /// <returns>The test result.</returns>
        public static T ExecuteOnDispatcherThread<T>(Func<T> test, int milliSecondsToWait = 5000)
        {
            var result = default(T);

            Action execute = () => result = test();

            ExecuteOnDispatcherThread(execute, milliSecondsToWait);

            return result;
        }

        /// <summary>
        /// Starts the timer.
        /// </summary>
        /// <param name="milliSecondsToWait">The milliSeconds to wait.</param>
        /// <param name="dispatcher">The dispatcher.</param>
        private static void StartTimer(int milliSecondsToWait, Dispatcher dispatcher)
        {
            var timeToWait = TimeSpan.FromMilliseconds(milliSecondsToWait);
            var timer = new DispatcherTimer(timeToWait, DispatcherPriority.ApplicationIdle, stop, dispatcher);
            timer.Start();
        }

        /// <summary>
        /// Executes the specfied action on a seperate thread.
        /// </summary>
        /// <param name="dispatcherAction">The dispatcher action.</param>
        private static void ExecuteOnNewThread(Action dispatcherAction)
        {
            var dispatchThread = new Thread(new ThreadStart(dispatcherAction));
            dispatchThread.SetApartmentState(ApartmentState.STA);
            dispatchThread.Start();
            dispatchThread.Join();
        }
    }
}
