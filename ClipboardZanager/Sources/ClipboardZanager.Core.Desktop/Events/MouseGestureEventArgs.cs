using System;

namespace ClipboardZanager.Core.Desktop.Events
{
    /// <summary>
    /// Represents the arguments of a MouseGestureEvent
    /// </summary>
    internal sealed class MouseGestureEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the name of the gesture
        /// </summary>
        internal string Name { get; }

        /// <summary>
        /// Gets an array that describe the gesture
        /// </summary>
        internal int[,] GestureArray { get; }

        /// <summary>
        /// Gets the date time of the event
        /// </summary>
        internal long Time { get; }

        /// <summary>
        /// Initialize a new instance of the <see cref="MouseGestureEventArgs"/> class
        /// </summary>
        /// <param name="name">the name of the gesture</param>
        /// <param name="gestureArray">an array that describe the gesture</param>
        /// <param name="time">the date time of the event</param>
        internal MouseGestureEventArgs(string name, int[,] gestureArray, long time)
        {
            Name = name;
            GestureArray = gestureArray;
            Time = time;
        }
    }
}
