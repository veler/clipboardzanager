namespace ClipboardZanager.Core.Desktop.Enums
{
    internal enum Stgm : long
    {
        StgmRead = 0x00000000L,
        StgmWrite = 0x00000001L,
        StgmReadwrite = 0x00000002L,
        StgmShareDenyNone = 0x00000040L,
        StgmShareDenyRead = 0x00000030L,
        StgmShareDenyWrite = 0x00000020L,
        StgmShareExclusive = 0x00000010L,
        StgmPriority = 0x00040000L,
        StgmCreate = 0x00001000L,
        StgmConvert = 0x00020000L,
        StgmFailifthere = 0x00000000L,
        StgmDirect = 0x00000000L,
        StgmTransacted = 0x00010000L,
        StgmNoscratch = 0x00100000L,
        StgmNosnapshot = 0x00200000L,
        StgmSimple = 0x08000000L,
        StgmDirectSwmr = 0x00400000L,
        StgmDeleteonrelease = 0x04000000L,
    }
}
