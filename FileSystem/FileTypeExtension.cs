using System;
using HardDrive;

namespace FileSystem.Savable
{
    public static class FileTypeExtension
    {
        public static Type GetFileType(this FileType fileType) => fileType switch
        {
            FileType.Directory => typeof(Directory),
            FileType.RegularFile => typeof(RegularFile),
            FileType.Symlink => typeof(Symlink),
            _ => throw new UnexpectedFileTypeException(fileType)
        };
    }
}