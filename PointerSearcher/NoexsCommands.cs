namespace PointerSearcher
{
    public enum NoexsCommands
    {
        Abort = 0x00,
        Status = 0x01,
        Poke8 = 0x02,
        Poke16 = 0x03,
        Poke32 = 0x04,
        Poke64 = 0x05,
        ReadMem = 0x06,
        WriteMem = 0x07,
        Resume = 0x08,
        Pause = 0x09,
        Attach = 0x0A,
        detach = 0x0B,
        QueryMemSingle = 0x0C,
        QueryMemMulti = 0x0D,
        CurrentPid = 0x0E,
        AttachedPid = 0x0F,
        ListPids = 0x10,
        GetTitleId = 0x11,
        Disconnect = 0x12,
        ReadMemMulti = 0x13,
        SetBreakpoint = 0x14,
        FreezeAddress = 0x15,
        SearchLocal = 0x16,
        FetchResult = 0x17,
        DetachDmnt = 0x18,
        DumpPtr = 0x19,
        AttachDmnt = 0x1A,
        GetBookmark = 0x1B,
        PutBookmark = 0x1C,
        DmntResume = 0x1D
    }
}
