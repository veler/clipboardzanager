using System;

namespace ClipboardZanager.Shared.Exceptions
{
    public sealed class NotNullOrWhiteSpaceRequiredException : ArgumentException
    {
        public NotNullOrWhiteSpaceRequiredException(string parameterName)
            : base(parameterName, "The value must not be null or white space")
        {
        }
    }
}
