using System;

namespace ClipboardZanager.Shared.Exceptions
{
    public sealed class NotAuthenticatedException : Exception
    {
        public NotAuthenticatedException()
            : base("User not authenticated.")
        {
        }
    }
}
