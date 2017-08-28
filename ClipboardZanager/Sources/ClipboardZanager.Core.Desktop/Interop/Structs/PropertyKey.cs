using System;
using System.Runtime.InteropServices;

namespace ClipboardZanager.Core.Desktop.Interop.Structs
{
    // PropertyKey Structure
    // Narrowed down from PropertyKey.cs of Windows API Code Pack 1.1
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct PropertyKey
    {
        #region Properties

        /// <summary>
        /// Gets the Unique GUID for property
        /// </summary>
        internal Guid FormatId { get; }

        /// <summary>
        /// Gets the Property identifier (PID)
        /// </summary>
        internal int PropertyId { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initialize a new instance of <see cref="PropVariant"/> class
        /// </summary>
        /// <param name="formatId">the Unique GUID for property</param>
        /// <param name="propertyId">the Property identifier (PID)</param>
        public PropertyKey(Guid formatId, int propertyId)
        {
            FormatId = formatId;
            PropertyId = propertyId;
        }

        /// <summary>
        /// Initialize a new instance of <see cref="PropVariant"/> class
        /// </summary>
        /// <param name="formatId">the Unique GUID for property</param>
        /// <param name="propertyId">the Property identifier (PID)</param>
        public PropertyKey(string formatId, int propertyId)
        {
            FormatId = new Guid(formatId);
            PropertyId = propertyId;
        }

        #endregion
    }
}
