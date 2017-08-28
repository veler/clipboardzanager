using System;
using ClipboardZanager.Core.Desktop.Enums;
using ClipboardZanager.Core.Desktop.Interop.Structs;

namespace ClipboardZanager.Core.Desktop.Events
{
    /// <summary>
    /// Represents the arguments of a <see cref="MouseHook.MouseAction"/>.
    /// </summary>
    internal sealed class MouseHookEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the coordonates of the mouse.
        /// </summary>
        internal Point Coords { get; }

        /// <summary>
        /// Gets the delta coordonates of the mouse.
        /// </summary>
        internal Point DeltaCoords { get; }

        /// <summary>
        /// Gets which button has been pressed.
        /// </summary>
        internal MouseAction Action { get; }

        /// <summary>
        /// Gets whether the wheel is going up or down.
        /// </summary>
        internal MouseWheelAction WheelAction { get; }

        /// <summary>
        /// Gets the date time of the event.
        /// </summary>
        internal long Time { get; }

        /// <summary>
        /// Gets or sets a value that defines whether the mouse has been handled or not.
        /// </summary>
        internal bool Handled { get; set; }

        /// <summary>
        /// Initialize a new instance of the <see cref="MouseHookEventArgs"/> class.
        /// </summary>
        /// <param name="coords">the coordonates of the mouse.</param>
        /// <param name="deltaCoords">the delta coordonates of the mouse.</param>
        /// <param name="mouseAction">which button has been pressed.</param>
        /// <param name="mouseWheelAction">whether the wheel is going up or down.</param>
        /// <param name="time">the date time of the event.</param>
        internal MouseHookEventArgs(Point coords, Point deltaCoords, MouseAction mouseAction, MouseWheelAction mouseWheelAction, long time)
        {
            Coords = coords;
            DeltaCoords = deltaCoords;
            Action = mouseAction;
            WheelAction = mouseWheelAction;
            Time = time;
        }
    }
}
