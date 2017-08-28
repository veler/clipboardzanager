using System;
using System.Runtime.InteropServices.ComTypes;
using System.Windows;
using ClipboardZanager.Core.Desktop.ComponentModel;

namespace ClipboardZanager.Core.Desktop.Clipboard
{
    /// <summary>
    /// Implementation of <see cref="IEnumFORMATETC"/> for <see cref="DataObject"/>.
    /// </summary>
    internal class FormatEnumerator : IEnumFORMATETC
    {
        #region Fields

        private readonly FORMATETC[] _formats;
        private int _current;

        #endregion

        #region Constructors

        /// <summary>  
        /// Initialize a new instance of the <see cref="FormatEnumerator"/> class.
        /// </summary>
        /// <param name="dataObject">The <see cref="System.Windows.IDataObject"/></param>
        internal FormatEnumerator(System.Windows.IDataObject dataObject)
        {
            var formats = dataObject.GetFormats();
            _formats = new FORMATETC[formats == null ? 0 : formats.Length];

            if (formats != null)
            {
                for (var i = 0; i < formats.Length; i++)
                {
                    var format = formats[i];
                    var temp = new FORMATETC();
                    temp.cfFormat = (short)DataFormats.GetDataFormat(format).Id;
                    temp.dwAspect = DVASPECT.DVASPECT_CONTENT;
                    temp.ptd = IntPtr.Zero;
                    temp.lindex = -1;
                    temp.tymed = TYMED.TYMED_HGLOBAL;

                    _formats[i] = temp;
                }
            }
        }

        /// <summary> 
        /// Initialize a new copy of a <see cref="FormatEnumerator"/> instance.
        /// </summary>
        /// <param name="formatEnumerator">The instance to copy</param>
        private FormatEnumerator(FormatEnumerator formatEnumerator)
        {
            _formats = formatEnumerator._formats;
            _current = formatEnumerator._current;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Retrieves a specified number of items in the enumeration sequence.
        /// </summary>
        /// <param name="celt">The number of <see cref="FORMATETC"/> references to return in rgelt.</param>
        /// <param name="rgelt">When this method returns, contains a reference to the enumerated <see cref="FORMATETC"/> references. This parameter is passed uninitialized.</param>
        /// <param name="pceltFetched">When this method returns, contains a reference to the actual number of references enumerated in rgelt. This parameter is passed uninitialized.</param>
        /// <returns><see cref="Consts.S_OK"/> if the pceltFetched parameter equals the celt parameter; otherwise, <see cref="Consts.S_FALSE"/>.</returns>
        public int Next(int celt, FORMATETC[] rgelt, int[] pceltFetched)
        {
            var fetched = 0;

            for (var i = 0; i < celt && _current < _formats.Length; i++)
            {
                rgelt[i] = _formats[_current];
                _current++;
                fetched++;
            }

            if (pceltFetched != null)
            {
                pceltFetched[0] = fetched;
            }

            return fetched == celt ? Consts.S_OK : Consts.S_FALSE;
        }

        /// <summary>
        /// Skips a specified number of items in the enumeration sequence.
        /// </summary>
        /// <param name="celt">The number of elements to skip in the enumeration.</param>
        /// <returns><see cref="Consts.S_OK"/> if the number of elements skipped equals the celt parameter; otherwise, <see cref="Consts.S_FALSE"/></returns>
        public int Skip(int celt)
        {
            // Make sure we don't overflow on the skip.
            _current = _current + Math.Min(celt, int.MaxValue - _current);

            return _current < _formats.Length ? Consts.S_OK : Consts.S_FALSE;
        }

        /// <summary>
        /// Resets the enumeration sequence to the beginning.
        /// </summary>
        /// <returns>An HRESULT with the value <see cref="Consts.S_OK"/>.</returns>
        public int Reset()
        {
            _current = 0;
            return Consts.S_OK;
        }

        /// <summary>
        /// Creates a new enumerator that contains the same enumeration state as the current enumerator.
        /// </summary>
        /// <param name="ppenum">When this method returns, contains a reference to the newly created enumerator.</param>
        public void Clone(out IEnumFORMATETC ppenum)
        {
            ppenum = new FormatEnumerator(this);
        }

        #endregion
    }
}
