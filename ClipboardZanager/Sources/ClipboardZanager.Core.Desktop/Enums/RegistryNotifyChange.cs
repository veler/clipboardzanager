using System;

namespace ClipboardZanager.Core.Desktop.Enums
{
    [Flags]
    public enum RegistryNotifyChange : uint
    {
        NAME = 0x1,
        ATTRIBUTES = 0x2,
        LAST_SET = 0x4,
        SECURITY = 0x8
    }
}
