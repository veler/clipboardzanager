using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using ClipboardZanager.Shared.Logs;

namespace ClipboardZanager.Core.Desktop.ComponentModel
{
    /// <summary>
    /// Provides a set of functions designed to help to manage threads.
    /// </summary>
    internal static class DispatcherHelper
    {
        #region Fields

        private static List<Dispatcher> _dispatchers;

        #endregion

        #region  Constructors

        /// <summary>
        /// Initialize the static instance of the <see cref="DispatcherHelper"/> class.
        /// </summary>
        static DispatcherHelper()
        {
            Initialize();
        }

        #endregion

        #region Handled Methods

        private static void Application_Exit(object sender, EventArgs e)
        {
            StopAll();
        }

        private static object ExitFrame(object frame)
        {
            ((DispatcherFrame)frame).Continue = false;
            return null;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Throw an exception if the current thread is not <see cref="ApartmentState.STA"/>
        /// </summary>
        /// <param name="callerName">(optional) The name of the caller member.</param>
        /// <param name="sourceFilePath">(optional) The path of the file who call this method.</param>
        /// <param name="sourceLineNumber">(optional) The line number of the fatal error.</param>
        internal static void ThrowIfNotStaThread([CallerMemberName] string callerName = null, [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
            {
                var exception = new ThreadStateException("STA thread required");
                if (Logger.InstanceLogSession != null)
                {
                    Logger.Instance.Fatal(exception, callerName, sourceFilePath, sourceLineNumber);
                }
                else
                {
                    throw exception;
                }
            }
        }


        /// <summary>
        /// Create a new Thread based on a WPF <see cref="Dispatcher"/>. The application will continue to run until the dispatcher has been close.
        /// </summary>
        /// <returns>A new <see cref="Dispatcher"/></returns>
        internal static Dispatcher CreateNewThread()
        {
            Initialize();

            Dispatcher dispatcher = null;
            var manualResetEvent = new ManualResetEvent(false);
            var thread = new Thread(() =>
            {
                dispatcher = Dispatcher.CurrentDispatcher;
                var synchronizationContext = new DispatcherSynchronizationContext(dispatcher);
                SynchronizationContext.SetSynchronizationContext(synchronizationContext);

                manualResetEvent.Set();
                Dispatcher.Run();
            });
            thread.Start();
            manualResetEvent.WaitOne();
            manualResetEvent.Dispose();

            _dispatchers.Add(dispatcher);
            return dispatcher;
        }

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        internal static void DoEvents()
        {
            var frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(ExitFrame), frame);
            Dispatcher.PushFrame(frame);
        }

        /// <summary>
        /// Initialize and confirm that the class is setted up.
        /// </summary>
        private static void Initialize()
        {
            if (_dispatchers == null)
            {
                _dispatchers = new List<Dispatcher>();

                if (CoreHelper.IsUnitTesting())
                {
                    // Unit tests
                    Dispatcher.CurrentDispatcher.ShutdownStarted += Application_Exit;
                }
                else
                {
                    // Real execution
                    Application.Current.Exit += Application_Exit;
                }
            }
        }

        /// <summary>
        /// Shutdown all the created dispatchers
        /// </summary>
        private static void StopAll()
        {
            // Shutdown all the dispatchers we have, so the application will exit properly.
            foreach (var dispatcher in _dispatchers)
            {
                if (!dispatcher.HasShutdownFinished && !dispatcher.HasShutdownStarted)
                {
                    dispatcher.InvokeShutdown();
                }
            }
        }

        #endregion
    }
}
