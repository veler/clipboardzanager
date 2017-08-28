using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Media;

namespace ClipboardZanager.Core.Desktop.Interop
{
    /// <summary>
    /// Provide the system color set
    /// </summary>
    internal class AccentColorSet
    {
        #region Fields

        private static AccentColorSet[] _allSets;
        private static AccentColorSet _activeSet;

        private uint _colorSet;

        #endregion

        #region Properties

        internal static AccentColorSet[] AllSets
        {
            get
            {
                if (_allSets != null)
                {
                    return _allSets;
                }

                var colorSetCount = NativeMethods.GetImmersiveColorSetCount();

                var colorSets = new List<AccentColorSet>();
                for (uint i = 0; i < colorSetCount; i++)
                {
                    colorSets.Add(new AccentColorSet(i, false));
                }

                AllSets = colorSets.ToArray();

                return _allSets;
            }
            private set
            {
                _allSets = value;
            }
        }

        internal static AccentColorSet ActiveSet
        {
            get
            {
                var activeSet = NativeMethods.GetImmersiveUserColorSetPreference(false, false);
                ActiveSet = AllSets[Math.Min(activeSet, AllSets.Length - 1)];
                return _activeSet;
            }
            private set
            {
                if (_activeSet != null)
                {
                    _activeSet.Active = false;
                }

                value.Active = true;
                _activeSet = value;
            }
        }

        internal bool Active { get; private set; }

        internal Color this[string colorName]
        {
            get
            {
                var name = IntPtr.Zero;
                uint colorType;

                try
                {
                    name = Marshal.StringToHGlobalUni("Immersive" + colorName);
                    colorType = NativeMethods.GetImmersiveColorTypeFromName(name);
                    if (colorType == 0xFFFFFFFF)
                    {
                        throw new InvalidOperationException();
                    }
                }
                finally
                {
                    if (name != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(name);
                        name = IntPtr.Zero;
                    }
                }

                return this[colorType];
            }
        }

        internal Color this[uint colorType]
        {
            get
            {
                var nativeColor = NativeMethods.GetImmersiveColorFromColorSetEx(_colorSet, colorType, false, 0);
                //if (nativeColor == 0)
                //    throw new InvalidOperationException();
                return Color.FromArgb(
                    (byte)((0xFF000000 & nativeColor) >> 24),
                    (byte)((0x000000FF & nativeColor) >> 0),
                    (byte)((0x0000FF00 & nativeColor) >> 8),
                    (byte)((0x00FF0000 & nativeColor) >> 16)
                    );
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="AccentColorSet"/> class
        /// </summary>
        /// <param name="colorSet"></param>
        /// <param name="active"></param>
        internal AccentColorSet(uint colorSet, bool active)
        {
            _colorSet = colorSet;
            Active = active;
        }

        #endregion

        #region Methods

        /// <summary>
        /// HACK: GetAllColorNames collects the available color names by brute forcing the OS function.
        ///  Since there is currently no known way to retrieve all possible color names,
        ///  the method below just tries all indices from 0 to 0xFFF ignoring errors.
        /// </summary>
        /// <returns>The list of color names</returns>
        public List<string> GetAllColorNames()
        {
            var allColorNames = new List<string>();
            for (uint i = 0; i < 0xFFF; i++)
            {
                var typeNamePtr = NativeMethods.GetImmersiveColorNamedTypeByIndex(i);
                if (typeNamePtr != IntPtr.Zero)
                {
                    var typeName = (IntPtr)Marshal.PtrToStructure(typeNamePtr, typeof(IntPtr));
                    allColorNames.Add(Marshal.PtrToStringUni(typeName));
                }
            }

            return allColorNames;
        }

        #endregion
    }
}
