using System;

namespace HardDrive
{
    public enum FileType : byte
    {
        None,
        Directory,
        RegularFile,
        Symlink
    }
}