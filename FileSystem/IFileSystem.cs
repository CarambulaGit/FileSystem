using System.IO;
using FileSystem.Savable;
using HardDrive;
using Directory = FileSystem.Savable.Directory;

namespace FileSystem
{
    public interface IFileSystem
    {
        string RootDirectoryPath { get; }
        string RootPath { get; }
        string RootName { get; }
        Directory CurrentDirectory { get; }
        Directory RootDirectory { get; }
        BitmapSection BitmapSection { get; }
        InodesSection InodesSection { get; }
        DataBlocksSection DataBlocksSection { get; }
        void Initialize();
        Directory CreateDirectory(string name, string path);
        Directory ReadDirectory(Inode inode);
        void SaveDirectory(Directory directory);
        void DeleteDirectory(Directory directory);
        RegularFile CreateFile(string name);
        RegularFile ReadFile(Inode inode);
        void SaveFile();
        void DeleteFile();
        Inode GetInodeByPath(string path);
    }
}