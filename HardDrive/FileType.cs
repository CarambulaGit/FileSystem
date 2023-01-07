using System;

namespace HardDrive
{
    public enum FileType : byte
    {
        Directory,
        RegularFile,
        Symlink
    }
}