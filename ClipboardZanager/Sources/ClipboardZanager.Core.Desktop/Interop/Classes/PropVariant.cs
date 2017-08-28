using System;
using System.Runtime.InteropServices;
using ClipboardZanager.Shared.Core;

namespace ClipboardZanager.Core.Desktop.Interop.Classes
{
    /// <summary>
    /// PropVariant Class (only for string value)
    /// Narrowed down from PropVariant.cs of Windows API Code Pack 1.1
    /// Originally from http://blogs.msdn.com/b/adamroot/archive/2008/04/11/interop-with-propvariants-in-net.aspx
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    internal sealed class PropVariant : IDisposable
    {
        #region Fields

        [FieldOffset(0)]
        private ushort _valueType;

        [FieldOffset(8)]
        private IntPtr _ptr;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the value type
        /// </summary>
        internal VarEnum VarType
        {
            get { return (VarEnum)_valueType; }
            set { _valueType = (ushort)value; }
        }

        /// <summary>
        /// Gets a value that defines if the value is null or empty or not.
        /// </summary>
        internal bool IsNullOrEmpty
        {
            get
            {
                return _valueType == (ushort)VarEnum.VT_EMPTY || _valueType == (ushort)VarEnum.VT_NULL;
            }
        }

        /// <summary>
        /// Gets the value
        /// </summary>
        internal string Value
        {
            get
            {
                return Marshal.PtrToStringUni(_ptr);
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new instance of <see cref="PropVariant"/> class
        /// </summary>
        internal PropVariant()
        {
        }

        /// <summary>
        /// Initialize a new instance of <see cref="PropVariant"/> class
        /// </summary>
        /// <param name="value">The value of the <see cref="PropVariant"/></param>
        internal PropVariant(string value)
        {
            Requires.NotNull(value, nameof(value));

            _valueType = (ushort)VarEnum.VT_LPWSTR;
            _ptr = Marshal.StringToCoTaskMemUni(value);
        }

        #endregion

        #region Destructors

        /// <summary>
        /// Explicit destructor of the class
        /// </summary>
        ~PropVariant()
        {
            Dispose();
        }

        /// <summary>
        /// Dispose the resources
        /// </summary>
        public void Dispose()
        {
            NativeMethods.PropVariantClear(this);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
