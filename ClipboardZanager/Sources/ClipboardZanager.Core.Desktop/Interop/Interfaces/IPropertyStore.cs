using System.Runtime.InteropServices;
using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Core.Desktop.Interop.Classes;
using ClipboardZanager.Core.Desktop.Interop.Structs;

namespace ClipboardZanager.Core.Desktop.Interop.Interfaces
{
    /// <summary>
    /// Exposes methods for enumerating, getting, and setting property values.
    /// </summary>
    [ComImport, Guid(Consts.PropertyStore), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPropertyStore
    {
        /// <summary>
        /// Gets the number of properties attached to the file.
        /// </summary>
        /// <param name="cProps">When this method returns, contains the property count.</param>
        /// <returns>Returns 0 is success</returns>
        int GetCount([Out] out uint cProps);

        /// <summary>
        /// Gets a property key from an item's array of properties.
        /// </summary>
        /// <param name="iProp">The index of the property key in the array of <see cref="PropertyKey"/> structures. This is a zero-based index.</param>
        /// <param name="pkey">When this method returns, contains a <see cref="PropertyKey"/> structure that receives the unique identifier for a property.</param>
        /// <returns>Returns 0 is success</returns>
        int GetAt([In] uint iProp, out PropertyKey pkey);

        /// <summary>
        /// Gets data for a specific property.
        /// </summary>
        /// <param name="key">A reference to the <see cref="PropertyKey"/> structure retrieved through <see cref="GetAt(uint, out PropertyKey)"/>. This structure contains a unique identifier for the property in question.</param>
        /// <param name="pv">When this method returns, contains a <see cref="PropVariant"/> structure that contains the property data.</param>
        /// <returns>Returns 0 is success</returns>
        int GetValue([In] ref PropertyKey key, [Out] PropVariant pv);

        /// <summary>
        /// Sets a new property value, or replaces or removes an existing value.
        /// </summary>
        /// <param name="key">A reference to the <see cref="PropertyKey"/> structure retrieved through <see cref="GetAt(uint, out PropertyKey)"/>. This structure contains a unique identifier for the property in question.</param>
        /// <param name="pv">A reference to a <see cref="PropVariant"/> structure that contains the new property data.</param>
        /// <returns>Returns 0 is success</returns>
        int SetValue([In] ref PropertyKey key, [In] PropVariant pv);

        /// <summary>
        /// Saves a property change.
        /// </summary>
        /// <returns>Returns 0 is success</returns>
        int Commit();
    }
}
