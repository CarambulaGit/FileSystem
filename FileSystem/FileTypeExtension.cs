using System;
using FileSystem.Exceptions;
using FileSystem.Savable;
using HardDrive;

namespace FileSystem
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