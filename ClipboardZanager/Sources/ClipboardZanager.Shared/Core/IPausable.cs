using System;

namespace ClipboardZanager.Shared.Core
{
    /// <summary>
    /// Provides a set of methods designed to pause something and resume it
    /// </summary>
    public interface IPausable : IDisposable
    {
        /// <summary>
        /// Put the process in pause
        /// </summary>
        void Pause();

        /// <summary>
        /// Resume the process
        /// </summary>
        void Resume();
    }
}
