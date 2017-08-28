using System;
using System.Security.Cryptography;
using System.Text;
using ClipboardZanager.Shared.Core;

namespace ClipboardZanager.ComponentModel.Messages
{
    /// <summary>
    /// Represents an identifier used to identify the sender, target and reason of a message sent to a <see cref="ViewModelBase"/>
    /// </summary>
    internal sealed class MessageIdentifier
    {
        #region Fields

        private readonly string _hash;

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="MessageIdentifier"/> class.
        /// </summary>
        /// <param name="sender">the type of the sender</param>
        /// <param name="target">the type of the target</param>
        /// <param name="reason">the reason why the message is sent</param>
        internal MessageIdentifier(Type sender, Type target, string reason)
        {
            Requires.NotNull(sender, nameof(sender));
            Requires.NotNull(target, nameof(target));
            Requires.NotNullOrWhiteSpace(reason, nameof(reason));
            _hash = GenerateHash($"{sender.FullName}{target.FullName}{reason}");
        }

        #endregion

        #region Methods

        public override bool Equals(object obj)
        {
            var messageIdentifier = obj as MessageIdentifier;
            Requires.NotNull(messageIdentifier, nameof(messageIdentifier));

            return string.Compare(ToString(), messageIdentifier?.ToString(), StringComparison.OrdinalIgnoreCase) == 0;
        }

        public override string ToString()
        {
            return _hash;
        }

        /// <summary>
        /// Generate a MD5 hash from a string
        /// </summary>
        /// <param name="input">the string hash</param>
        /// <returns>the has of the string</returns>
        private string GenerateHash(string input)
        {
            using (var md5Hash = MD5.Create())
            {
                var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sBuilder = new StringBuilder();

                foreach (var t in data)
                {
                    sBuilder.Append(t.ToString("x2"));
                }

                return sBuilder.ToString();
            }
        }

        #endregion
    }
}
