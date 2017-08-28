using System;
using System.Runtime.InteropServices.ComTypes;
using System.Security;
using System.Threading;
using System.Windows.Threading;
using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Core.Desktop.Interop;
using ClipboardZanager.Shared.Core;
using ClipboardZanager.Shared.Logs;

namespace ClipboardZanager.Core.Desktop.Clipboard
{
    /// <summary>
    /// This class manages Ole services for DragDrop and Clipboard.
    /// The instance of OleServicesContext class is created per Thread...Dispatcher.
    /// </summary>
    internal sealed class OleServicesContext
    {
        #region Fields

        private static readonly LocalDataStoreSlot _threadDataSlot = Thread.AllocateDataSlot();

        #endregion

        #region Properties

        /// <summary>
        /// Get the ole services context associated with the current Thread.
        /// </summary>
        internal static OleServicesContext CurrentOleServicesContext
        {
            get
            {
                // Get the ole services context from the Thread data slot.
                var oleServicesContext = (OleServicesContext)Thread.GetData(_threadDataSlot);

                if (oleServicesContext == null)
                {
                    // Create OleSErvicesContext instance.
                    oleServicesContext = new OleServicesContext();

                    // Save the ole services context into the UIContext data slot.
                    Thread.SetData(_threadDataSlot, oleServicesContext);
                }

                return oleServicesContext;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="OleServicesContext"/> class.
        /// </summary>
        private OleServicesContext()
        {
            // We need to get the Dispatcher Thread in order to get OLE DragDrop and Clipboard services that
            // require STA.
            SetDispatcherThread();
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Set the clipboard content.
        /// </summary>
        /// <param name="dataObject">The data to set into the clipboard.</param>
        /// <returns>This function returns <see cref="Consts.S_OK"/> on success.</returns>
        [SecurityCritical]
        internal int OleSetClipboard(IDataObject dataObject)
        {
            DispatcherHelper.ThrowIfNotStaThread();

            return NativeMethods.OleSetClipboard(dataObject);
        }

        /// <summary>
        /// Flush the content of the clipboard.
        /// </summary>
        /// <returns>This function returns <see cref="Consts.S_OK"/> on success.</returns>
        [SecurityCritical]
        internal int OleFlushClipboard()
        {
            DispatcherHelper.ThrowIfNotStaThread();

            return NativeMethods.OleFlushClipboard();
        }

        /// <summary>
        /// SetDispatcherThread - Initialize OleServicesContext that will call Ole initialize for ole services(DragDrop and Clipboard)
        /// and add the disposed event handler of Dispatcher to clean up resources and uninitalize Ole.
        /// </summary>
        private void SetDispatcherThread()
        {
            DispatcherHelper.ThrowIfNotStaThread();

            // Initialize Ole services.
            // Balanced with OleUninitialize call in OnDispatcherShutdown.
            var hr = OleInitialize();

            Requires.VerifySucceeded(hr);

            // Add Dispatcher.Shutdown event handler. 
            // We will call ole Uninitialize and clean up the resource when UIContext is terminated.
            Dispatcher.CurrentDispatcher.ShutdownFinished += OnDispatcherShutdown;
        }

        /// <summary>
        /// This is a callback when Dispatcher is shut down.
        /// </summary>
        /// <remarks>
        /// This method must be called before shutting down the application
        /// on the dispatcher thread.  It must be called by the same
        /// thread running the dispatcher and the thread must have its
        /// ApartmentState property set to ApartmentState.STA.
        /// </remarks>
        private void OnDispatcherShutdown(object sender, EventArgs args)
        {
            DispatcherHelper.ThrowIfNotStaThread();

            // Uninitialize Ole services.
            // Balanced with OleInitialize call in SetDispatcherThread.
            OleUninitialize();
        }

        /// <summary>
        /// Initializes the COM library on the current apartment, identifies the concurrency model as single-thread apartment (STA), and enables additional functionality described in the Remarks section below. Applications must initialize the COM library before they can call COM library functions other than CoGetMalloc and memory allocation functions.
        /// </summary>
        /// <returns>This function returns <see cref="Consts.S_OK"/> on success.</returns>
        [SecurityCritical]
        private int OleInitialize()
        {
            var hr = NativeMethods.OleInitialize(IntPtr.Zero);
            Logger.Instance.Debug("Ole initialized.");

            return hr;
        }

        /// <summary>
        /// Closes the COM library
        /// </summary>
        /// <returns>This function returns <see cref="Consts.S_OK"/> on success.</returns>
        [SecurityCritical]
        private int OleUninitialize()
        {
            var hr = NativeMethods.OleUninitialize();
            Logger.Instance.Debug("Ole uninitialized.");

            return hr;
        }

        #endregion
    }
}
