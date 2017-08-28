using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security;
using System.Security.Permissions;
using System.Windows;
using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Core.Desktop.Interop;
using ClipboardZanager.Shared.Core;
using ClipboardZanager.Shared.Logs;
using IDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;

namespace ClipboardZanager.Core.Desktop.Clipboard
{
    /// <summary>
    /// Provides a wrapper designed to read the clipboard.
    /// </summary>
    internal sealed class ClipboardReader : IDisposable
    {
        #region Fields

        private readonly IDataObject _oleDataObject;
        private STGMEDIUM _stgmedium;
        private IntPtr _dataHandle;
        private HandleRef _dataHandleRef;
        private int _dataSize;
        private int _readLength;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value that defines wether the current data is readable or not.
        /// </summary>
        internal bool IsReadable { get; private set; }

        #endregion

        #region Constructors

        /// <summary> 
        /// Initialize a new instance of the <see cref="ClipboardReader"/> class.
        /// </summary>
        /// <param name="dataObject">The data object that comes from the clipboard to read.</param>
        public ClipboardReader(IDataObject dataObject)
        {
            Requires.NotNull(dataObject, nameof(dataObject));
            SecurityHelper.DemandAllClipboardPermission();
            _oleDataObject = dataObject;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Dispose class.
        /// </summary>
        public void Dispose()
        {
            EndRead();
        }

        /// <summary>
        /// Handle and begin to read the specified data format.
        /// </summary>
        /// <param name="format">The data's format to read.</param>
        internal void BeginRead(string format)
        {
            Requires.NotNullOrEmpty(format, nameof(format));
            Requires.IsFalse(IsReadable);

            IsReadable = false;
            _readLength = 0;
            var formatetc = new FORMATETC
            {
                cfFormat = (short)DataFormats.GetDataFormat(format).Id,
                dwAspect = DVASPECT.DVASPECT_CONTENT,
                lindex = -1,
                tymed = TYMED.TYMED_HGLOBAL
            };

            Requires.IsFalse(Consts.S_OK != QueryGetDataInner(ref formatetc));

            try
            {
                GetDataInner(ref formatetc, out _stgmedium);
                if (_stgmedium.unionmember != IntPtr.Zero && _stgmedium.tymed == TYMED.TYMED_HGLOBAL)
                {
                    _dataHandleRef = new HandleRef(this, _stgmedium.unionmember);
                    _dataHandle = CoreHelper.Win32GlobalLock(_dataHandleRef);
                    _dataSize = DataHelper.IntPtrToInt32(CoreHelper.Win32GlobalSize(_dataHandleRef));
                    IsReadable = true;
                }
            }
            catch (Exception exception)
            {
                Logger.Instance.Error(exception);
                EndRead();
            }
        }

        /// <summary>
        /// Stop to read a data from the clipboard.
        /// </summary>
        internal void EndRead()
        {
            if (IsReadable)
            {
                CoreHelper.Win32GlobalUnlock(_dataHandleRef);
            }

            NativeMethods.ReleaseStgMedium(ref _stgmedium);

            _dataHandle = IntPtr.Zero;
            _dataSize = 0;
            _readLength = 0;
            IsReadable = false;
        }

        /// <summary>
        /// Determines whether the next block of bytes can be read or not.
        /// </summary>
        /// <returns>Return TRUE if the next block can be read.</returns>
        internal bool CanReadNextBlock()
        {
            return IsReadable && _readLength < _dataSize;
        }

        /// <summary>
        /// Read the next block of bytes from the current data in the clipboard.
        /// </summary>
        /// <returns>Return a block of bytes corresponding to a part of the data from the clipboard.</returns>
        internal byte[] ReadNextBlock()
        {
            Requires.IsTrue(CanReadNextBlock());

            var bufferSize = Consts.ClipboardDataBufferSize;
            var buffer = new byte[bufferSize];

            // resize the buffer if we are at the end of the data and that it remains less than the default buffer size to read.
            if (_dataSize - _readLength < bufferSize)
            {
                bufferSize = _dataSize - _readLength;
                buffer = new byte[bufferSize];
            }

            //Copy clipboard data to buffer
            var ptrUpdated = new IntPtr(_dataHandle.ToInt64() + _readLength);
            Marshal.Copy(ptrUpdated, buffer, 0, bufferSize);

            _readLength += bufferSize;

            return buffer;
        }

        /// <summary>
        /// Determines whether the data object is capable of rendering the data described in the <see cref="FORMATETC"/> structure. Objects attempting a paste or drop operation can call this method before calling GetData to get an indication of whether the operation may be successful.
        /// </summary>
        /// <param name="format">A pointer to a <see cref="FORMATETC"/> structure, passed by reference, that defines the format, medium, and target device to use for the query.</param>
        /// <returns>This method supports the standard return values <see cref="Consts.S_FALSE"/>, <see cref="Consts.S_OK"/> and else.</returns>
        [SecurityCritical]
        private int QueryGetDataInner(ref FORMATETC format)
        {
            var hr = Consts.S_FALSE;

            SecurityHelper.DemandUnmanagedCode();
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert(); // BlessedAssert

            try
            {
                hr = _oleDataObject.QueryGetData(ref format);
            }
            catch (Exception exception)
            {
                Logger.Instance.Error(exception);
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }

            return hr;
        }

        /// <summary>
        /// Obtains data from a source data object. The GetData method, which is called by a data consumer, renders the data described in the specified <see cref="FORMATETC"/> structure and transfers it through the specified <see cref="STGMEDIUM"/> structure. The caller then assumes responsibility for releasing the <see cref="STGMEDIUM"/> structure.
        /// </summary>
        /// <param name="format">A pointer to a <see cref="FORMATETC"/> structure, passed by reference, that defines the format, medium, and target device to use when passing the data. It is possible to specify more than one medium by using the Boolean OR operator, allowing the method to choose the best medium among those specified.</param>
        /// <param name="medium">When this method returns, contains a pointer to the <see cref="STGMEDIUM"/> structure that indicates the storage medium containing the returned data through its tymed member, and the responsibility for releasing the medium through the value of its pUnkForRelease member. If pUnkForRelease is null, the receiver of the medium is responsible for releasing it; otherwise, pUnkForRelease points to the IUnknown interface on the appropriate object so its Release method can be called. The medium must be allocated and filled in by GetData. This parameter is passed uninitialized.</param>
        [SecurityCritical]
        private void GetDataInner(ref FORMATETC format, out STGMEDIUM medium)
        {
            SecurityHelper.DemandUnmanagedCode();
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert(); // BlessedAssert
            try
            {
                _oleDataObject.GetData(ref format, out medium);
            }
            catch (Exception exception)
            {
                Logger.Instance.Error(exception);
                medium = default(STGMEDIUM);
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
        }

        #endregion
    }
}
