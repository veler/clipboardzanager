using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Shared.Core;

namespace ClipboardZanager.Core.Desktop.Clipboard
{
    /// <summary>
    /// Provides a wrapper designed to write into the clipboard.
    /// </summary>
    internal sealed class ClipboardWriter : IDisposable
    {
        #region Fields

        private readonly List<Stream> _streams;
        private DataObject _dataObject;

        #endregion

        #region Constructors

        /// <summary> 
        /// Initialize a new instance of the <see cref="ClipboardWriter"/> class.
        /// </summary>
        public ClipboardWriter()
        {
            SecurityHelper.DemandAllClipboardPermission();
            _dataObject = new DataObject();
            _streams = new List<Stream>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Dispose class.
        /// </summary>
        public void Dispose()
        {
            _dataObject = null;

            foreach (var stream in _streams)
            {
                stream.Dispose();
            }
        }

        /// <summary>
        /// Add a data to the clipboard writer.
        /// </summary>
        /// <param name="format">The data's format name.</param>
        /// <param name="dataStream">The stream that contains the data.</param>
        /// <param name="otherStreams">All the other streams that must be disposed once the write has been flushed.</param>
        internal void AddData(string format, Stream dataStream, params Stream[] otherStreams)
        {
            Requires.NotNull(dataStream, nameof(dataStream));

            _dataObject.SetData(format, dataStream);

            _streams.Add(dataStream);
            _streams.AddRange(otherStreams);
        }

        /// <summary>
        /// Persists the data to the clipboard.
        /// </summary>
        internal void Flush()
        {
            Requires.NotNull(_dataObject, nameof(_dataObject));

            var i = Consts.OleRetryCount;

            // Retry OLE operations several times as mitigation for clipboard locking issues in TS sessions.
            while (true)
            {
                // Clear the system clipboard by calling OleSetClipboard with null parameter.
                var hr = OleServicesContext.CurrentOleServicesContext.OleSetClipboard(_dataObject);

                if (Requires.Succeeded(hr))
                {
                    break;
                }

                if (--i == 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }

                Thread.Sleep(Consts.OleRetryDelay);
            }

            // OleSetClipboard and OleFlushClipboard both modify the clipboard
            // and cause notifications to be sent to clipboard listeners. We sleep a bit here to
            // mitigate issues with clipboard listeners (like TS) corrupting the clipboard contents
            // as a result of these two calls being back to back.
            Thread.Sleep(Consts.OleFlushDelay);

            i = Consts.OleRetryCount;

            // Retry OLE operations several times as mitigation for clipboard locking issues in TS sessions.
            while (true)
            {
                var hr = OleServicesContext.CurrentOleServicesContext.OleFlushClipboard();

                if (Requires.Succeeded(hr))
                {
                    break;
                }

                if (--i == 0)
                {
                    Marshal.ThrowExceptionForHR(hr, new IntPtr(-1));
                }

                Thread.Sleep(Consts.OleRetryDelay);
            }

            Thread.Sleep(Consts.OleRetryDelay);
        }

        #endregion
    }
}
