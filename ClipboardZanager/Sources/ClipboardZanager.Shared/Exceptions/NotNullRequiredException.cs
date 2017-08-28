using System;

namespace ClipboardZanager.Shared.Exceptions
{
    public sealed class NotNullRequiredException : ArgumentNullException
    {
        public NotNullRequiredException(string parameterName)
            : base(parameterName, "The value must not be null")
        {
        }
    }
}
