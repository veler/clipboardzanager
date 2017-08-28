using System;

namespace ClipboardZanager.Shared.Exceptions
{
    public sealed class IsFalseRequiredException : Exception
    {
        public IsFalseRequiredException()
            : base("The value must be false")
        {
        }
    }
}
