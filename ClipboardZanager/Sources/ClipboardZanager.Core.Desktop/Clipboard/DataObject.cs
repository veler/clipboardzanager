using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security;
using System.Windows;
using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Shared.Core;
using ClipboardZanager.Shared.Logs;
using IDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;

namespace ClipboardZanager.Core.Desktop.Clipboard
{
    /// <summary>
    /// Implements a basic data transfer mechanism.
    /// </summary>
    internal sealed class DataObject : System.Windows.IDataObject, IDataObject
    {
        #region Fields

        private readonly Dictionary<string, Stream> _data;

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="DataObject"/> class.
        /// </summary>
        public DataObject()
        {
            _data = new Dictionary<string, Stream>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a list of all formats that data stored in this instance is associated with or can be converted to.
        /// </summary>
        /// <returns>An array of the names that represents a list of all formats that are supported by the data stored in this object.</returns>
        [SecurityCritical]
        public string[] GetFormats()
        {
            return new List<string>(_data.Keys).ToArray();
        }

        /// <summary>
        /// Stores the specified data and its associated format in this instance.
        /// </summary>
        /// <param name="format">The format associated with the data. See <see cref="DataFormats"/> for predefined formats.</param>
        /// <param name="data">The data to store.</param>
        [SecurityCritical]
        public void SetData(string format, object data)
        {
            SetData(format, data as Stream);
        }

        /// <summary>
        /// Stores the specified data and its associated format in this instance.
        /// </summary>
        /// <param name="format">The format associated with the data. See <see cref="DataFormats"/> for predefined formats.</param>
        /// <param name="data">The data <see cref="Stream"/> to store.</param>
        internal void SetData(string format, Stream data)
        {
            SecurityHelper.DemandAllClipboardPermission();
            Requires.NotNullOrEmpty(format, nameof(format));
            Requires.NotNull(data, nameof(data));

            _data.Add(format, data);
        }

        /// <summary>
        /// Obtains data from a source data object. The GetData method, which is called by a data consumer, renders the data described in the specified <see cref="FORMATETC"/> structure and transfers it through the specified <see cref="STGMEDIUM"/> structure. The caller then assumes responsibility for releasing the <see cref="STGMEDIUM"/> structure.
        /// </summary>
        /// <param name="format">A pointer to a <see cref="FORMATETC"/> structure, passed by reference, that defines the format, medium, and target device to use when passing the data. It is possible to specify more than one medium by using the Boolean OR operator, allowing the method to choose the best medium among those specified.</param>
        /// <param name="medium">When this method returns, contains a pointer to the <see cref="STGMEDIUM"/> structure that indicates the storage medium containing the returned data through its tymed member, and the responsibility for releasing the medium through the value of its pUnkForRelease member. If pUnkForRelease is null, the receiver of the medium is responsible for releasing it; otherwise, pUnkForRelease points to the IUnknown interface on the appropriate object so its Release method can be called. The medium must be allocated and filled in by GetData. This parameter is passed uninitialized.</param>
        [SecurityCritical]
        void IDataObject.GetData(ref FORMATETC format, out STGMEDIUM medium)
        {
            medium = new STGMEDIUM();

            if ((format.tymed & TYMED.TYMED_HGLOBAL) == 0)
            {
                return;
            }

            medium.tymed = TYMED.TYMED_HGLOBAL;

            medium.unionmember = CoreHelper.Win32GlobalAlloc(Consts.GMEM_MOVEABLE | Consts.GMEM_DDESHARE | Consts.GMEM_ZEROINIT, (IntPtr)1);

            var hr = GetDataIntoOleStructs(ref format, ref medium, false /* doNotReallocate */);

            if (Requires.Failed(hr))
            {
                CoreHelper.Win32GlobalFree(new HandleRef(this, medium.unionmember));
            }
        }

        /// <summary>
        /// Obtains data from a source data object. This method, which is called by a data consumer, differs from the GetData method in that the caller must allocate and free the specified storage medium.
        /// </summary>
        /// <param name="format">A pointer to a <see cref="FORMATETC"/> structure, passed by reference, that defines the format, medium, and target device to use when passing the data. Only one medium can be specified in TYMED, and only the following TYMED values are valid: TYMED_ISTORAGE, TYMED_ISTREAM, TYMED_HGLOBAL, or TYMED_FILE.</param>
        /// <param name="medium">A <see cref="STGMEDIUM"/>, passed by reference, that defines the storage medium containing the data being transferred. The medium must be allocated by the caller and filled in by GetDataHere. The caller must also free the medium. The implementation of this method must always supply a value of null for the pUnkForRelease member of the <see cref="STGMEDIUM"/> structure that this parameter points to.</param>
        [SecurityCritical]
        void IDataObject.GetDataHere(ref FORMATETC format, ref STGMEDIUM medium)
        {
            // This method is spec'd to accepted only limited number of tymed
            // values, and it does not support multiple OR'd values.
            if (medium.tymed != TYMED.TYMED_ISTORAGE && medium.tymed != TYMED.TYMED_ISTREAM && medium.tymed != TYMED.TYMED_HGLOBAL && medium.tymed != TYMED.TYMED_FILE)
            {
                Marshal.ThrowExceptionForHR(Consts.DvETymed);
            }

            var hr = GetDataIntoOleStructs(ref format, ref medium, true /* doNotReallocate */);
            if (Requires.Failed(hr))
            {
                Marshal.ThrowExceptionForHR(hr);
            }
        }

        /// <summary>
        /// Determines whether the data object is capable of rendering the data described in the <see cref="FORMATETC"/> structure. Objects attempting a paste or drop operation can call this method before calling GetData to get an indication of whether the operation may be successful.
        /// </summary>
        /// <param name="format">A pointer to a <see cref="FORMATETC"/> structure, passed by reference, that defines the format, medium, and target device to use for the query.</param>
        /// <returns>This method supports the standard return values <see cref="Consts.S_FALSE"/>, <see cref="Consts.S_OK"/> and else.</returns>
        int IDataObject.QueryGetData(ref FORMATETC format)
        {
            if (format.dwAspect == DVASPECT.DVASPECT_CONTENT)
            {
                if (format.tymed == TYMED.TYMED_HGLOBAL)
                {
                    if (format.cfFormat == 0)
                    {
                        return Consts.S_FALSE;
                    }

                    return Consts.S_OK;
                }

                return Consts.DvETymed;
            }

            return Consts.DvEDvaspect;
        }

        /// <summary>
        /// This API supports the product infrastructure and is not intended to be used directly from your code. Creates an object for enumerating the <see cref="FORMATETC"/> structures for a data object. These structures are used in calls to GetData or SetData.
        /// </summary>
        /// <param name="direction">One of the <see cref="DATADIR"/> values that specifies the direction of the data.</param>
        /// <returns>This method supports the standard return values <see cref="Consts.S_FALSE"/>, <see cref="Consts.S_OK"/> and else.</returns>
        [SecurityCritical]
        IEnumFORMATETC IDataObject.EnumFormatEtc(DATADIR direction)
        {
            if (direction == DATADIR.DATADIR_GET)
            {
                return new FormatEnumerator(this);
            }

            throw new ExternalException(direction.ToString(), Consts.E_NOTIMPL);
        }

        /// <summary>
        /// Populates Ole datastructes from a [....] dataObject. This is the core
        /// of [....] to OLE conversion.
        /// </summary>
        [SecurityCritical]
        private int GetDataIntoOleStructs(ref FORMATETC formatetc, ref STGMEDIUM medium, bool doNotReallocate)
        {
            var format = DataFormats.GetDataFormat(formatetc.cfFormat).Name;

            var hr = SaveStreamToHandle(medium.unionmember, _data[format], doNotReallocate);

            if (hr == Consts.S_OK)
            {
                medium.tymed = TYMED.TYMED_HGLOBAL;
            }

            return hr;
        }

        /// <summary>
        /// Saves stream out to handle.
        /// </summary>
        /// <param name="handle">The handle where the data must be saved.</param>
        /// <param name="stream">The stream to save.</param>
        /// <param name="doNotReallocate">Defines wether the memory must be reallocated.</param>
        /// <returns>Returns <see cref="Consts.S_OK"/> if succeeded.</returns>
        [SecurityCritical]
        private int SaveStreamToHandle(IntPtr handle, Stream stream, bool doNotReallocate)
        {
            if (handle == IntPtr.Zero)
            {
                return Consts.E_INVALIDARG;
            }

            var dataSize = DataHelper.IntPtrToInt32((IntPtr)stream.Length);

            var hr = CoreHelper.EnsureMemoryCapacity(ref handle, new HandleRef(this, handle), dataSize, doNotReallocate);
            if (Requires.Failed(hr))
            {
                return hr;
            }

            var ptr = CoreHelper.Win32GlobalLock(new HandleRef(this, handle));

            try
            {
                var readLength = 0;
                var bufferSize = Consts.ClipboardDataBufferSize;
                var buffer = new byte[bufferSize];
                stream.Position = 0;

                while (readLength < dataSize)
                {
                    // resize the buffer if we are at the end of the data and that it remains less than the default buffer size to read.
                    if (dataSize - readLength < bufferSize)
                    {
                        bufferSize = dataSize - readLength;
                        buffer = new byte[bufferSize];
                    }

                    //Copy data to clipboard
                    var ptrUpdated = new IntPtr(ptr.ToInt64() + readLength);
                    stream.Read(buffer, 0, bufferSize);
                    Marshal.Copy(buffer, 0, ptrUpdated, bufferSize);

                    readLength += bufferSize;
                }
            }
            catch (Exception exception)
            {
                Logger.Instance.Error(exception);
            }
            finally
            {
                CoreHelper.Win32GlobalUnlock(new HandleRef(this, handle));
            }

            return Consts.S_OK;
        }

        object System.Windows.IDataObject.GetData(Type format)
        {
            throw new NotImplementedException();
        }

        object System.Windows.IDataObject.GetData(string format)
        {
            throw new NotImplementedException();
        }

        object System.Windows.IDataObject.GetData(string format, bool autoConvert)
        {
            throw new NotImplementedException();
        }

        bool System.Windows.IDataObject.GetDataPresent(Type format)
        {
            throw new NotImplementedException();
        }

        bool System.Windows.IDataObject.GetDataPresent(string format)
        {
            throw new NotImplementedException();
        }

        bool System.Windows.IDataObject.GetDataPresent(string format, bool autoConvert)
        {
            throw new NotImplementedException();
        }

        string[] System.Windows.IDataObject.GetFormats(bool autoConvert)
        {
            throw new NotImplementedException();
        }

        void System.Windows.IDataObject.SetData(object data)
        {
            throw new NotImplementedException();
        }

        void System.Windows.IDataObject.SetData(Type format, object data)
        {
            throw new NotImplementedException();
        }

        void IDataObject.SetData(ref FORMATETC formatIn, ref STGMEDIUM medium, bool release)
        {
            throw new NotImplementedException();
        }

        void System.Windows.IDataObject.SetData(string format, object data, bool autoConvert)
        {
            throw new NotImplementedException();
        }

        int IDataObject.DAdvise(ref FORMATETC pFormatetc, ADVF advf, IAdviseSink adviseSink, out int connection)
        {
            throw new NotImplementedException();
        }

        void IDataObject.DUnadvise(int connection)
        {
            throw new NotImplementedException();
        }

        int IDataObject.EnumDAdvise(out IEnumSTATDATA enumAdvise)
        {
            throw new NotImplementedException();
        }

        int IDataObject.GetCanonicalFormatEtc(ref FORMATETC formatIn, out FORMATETC formatOut)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
