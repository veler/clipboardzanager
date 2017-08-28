using System;
using System.Windows;

namespace ClipboardZanager.Core.Desktop.Events
{
    /// <summary>
    /// Represents the arguments of an event <see cref="ClipboardHook.ClipboardChanged"/>
    /// </summary>
    internal sealed class ClipboardHookEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the data of the clipboard
        /// </summary>
        internal DataObject DataObject { get; }

        /// <summary>
        /// Gets a value that defines whether the data has been cut of just copy.
        /// </summary>
        internal bool IsCut { get; }

        /// <summary>
        /// Gets the date time of the event
        /// </summary>
        internal long Time { get; }

        /// <summary>
        /// Initialize a new instance of the <see cref="ClipboardHookEventArgs"/> class
        /// </summary>
        /// <param name="dataObject">the data of the clipboard</param>
        /// <param name="isCut">defines whether the data has been cut of just copy</param>
        /// <param name="time">the date time of the event</param>
        internal ClipboardHookEventArgs(DataObject dataObject, bool isCut, long time)
        {
            DataObject = dataObject;
            IsCut = isCut;
            Time = time;
        }
    }
}
