using System;
using System.Windows.Input;
using ClipboardZanager.Core.Desktop.Enums;

namespace ClipboardZanager.Core.Desktop.Events
{
    /// <summary>
    /// Represents the arguments of an event <see cref="KeyboardHook.KeyboardAction"/>
    /// </summary>
    internal sealed class KeyboardHookEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the key.
        /// </summary>
        internal Key Key { get; }

        /// <summary>
        /// Gets the state of the key (pressed or released).
        /// </summary>
        internal KeyState State { get; }

        /// <summary>
        /// Gets a value that defines whether the letter is in uppercase or not.
        /// </summary>
        internal bool IsUppercase { get; }

        /// <summary>
        /// Gets the date time of the event.
        /// </summary>
        internal long Time { get; }

        /// <summary>
        /// Gets or sets a value that defines whether the key has been handled or not.
        /// </summary>
        internal bool Handled { get; set; }

        /// <summary>
        /// Initialize a new instance of the <see cref="KeyboardHookEventArgs"/> class.
        /// </summary>
        /// <param name="key">the key.</param>
        /// <param name="state">the state of the key (pressed or released).</param>
        /// <param name="isUppercase">defines whether the letter is in uppercase or not.</param>
        /// <param name="time">the date time of the event.</param>
        internal KeyboardHookEventArgs(Key key, KeyState state, bool isUppercase, long time)
        {
            Key = key;
            State = state;
            IsUppercase = isUppercase;
            Time = time;
        }
    }
}
