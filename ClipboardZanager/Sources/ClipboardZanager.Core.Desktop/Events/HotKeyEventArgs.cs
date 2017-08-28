using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace ClipboardZanager.Core.Desktop.Events
{
    /// <summary>
    ///  Represents the argument of a HotKeyEvent
    /// </summary>
    internal sealed class HotKeyEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the name of the hot key combinaison
        /// </summary>
        internal string Name { get; }

        /// <summary>
        /// Gets the keys
        /// </summary>
        internal List<Key> Keys { get; }

        /// <summary>
        /// Gets the date time of the event
        /// </summary>
        internal long Time { get; }

        /// <summary>
        /// Gets or sets a value that defines whether the key has been handled or not.
        /// </summary>
        internal bool Handled { get; set; }

        /// <summary>
        /// Initialize a new instance of the <see cref="HotKeyEventArgs"/> class
        /// </summary>
        /// <param name="name">the name of the hot key combinaison</param>
        /// <param name="keys">the keys</param>
        /// <param name="time">the date time of the event</param>
        internal HotKeyEventArgs(string name, List<Key> keys, long time)
        {
            Name = name;
            Keys = keys;
            Time = time;
        }
    }
}
