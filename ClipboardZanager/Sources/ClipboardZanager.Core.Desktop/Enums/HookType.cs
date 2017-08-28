namespace ClipboardZanager.Core.Desktop.Enums
{
    /// <summary>
    /// Represents a type of operating system hooking.
    /// </summary>
    internal enum HookType
    {
        Journalrecord = 0,
        Journalplayback = 1,
        Keyboard = 2,
        Getmessage = 3,
        Callwndproc = 4,
        Cbt = 5,
        Sysmsgfilter = 6,
        Mouse = 7,
        Hardware = 8,
        Debug = 9,
        Shell = 10,
        Foregroundidle = 11,
        Callwndprocret = 12,
        KeyboardLl = 13,
        MouseLl = 14
    }
}
