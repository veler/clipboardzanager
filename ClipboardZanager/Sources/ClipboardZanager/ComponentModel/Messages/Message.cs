using GalaSoft.MvvmLight.Messaging;

namespace ClipboardZanager.ComponentModel.Messages
{
    /// <summary>
    /// Represents a message used to communicate between <see cref="ViewModelBase"/>
    /// </summary>
    internal sealed class Message : MessageBase
    {
        #region Properties

        /// <summary>
        /// Gets the optional values passed by the message.
        /// </summary>
        internal object[] Values { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="MessageIdentifier"/> class.
        /// </summary>
        /// <param name="values">the optional values passed by the message.</param>
        internal Message(params object[] values)
        {
            Values = values;
        }

        #endregion
    }
}
