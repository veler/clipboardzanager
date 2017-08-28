using System;

namespace ClipboardZanager.Shared.Exceptions
{
    public sealed class IsTrueRequiredException : Exception
    {
        public IsTrueRequiredException()
            : base("The value must be true")
        {
        }
    }
}
