using System.IO;
using FileSystem.Savable;
using HardDrive;
using Directory = FileSystem.Savable.Directory;

namespace FileSystem
{
    public interface IFileSystem
    {
        string RootDirectoryPath { get; }
        string RootName { get; }
        string CurrentDirectoryPath { get; }
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
        RegularFile CreateFile(string name, string path);
        RegularFile ReadFile(Inode inode);
        void SaveFile(RegularFile file);
        void DeleteFile(RegularFile file);
        Inode GetInodeByPath(string path, out Inode parentInode);
        void DeleteFile(string path);
        void ChangeCurrentDirectory(string path);
        void DeleteDirectory(string path);
        RegularFile ReadFile(string path);
        Directory ReadDirectory(string path);
        void LinkFile(string pathToFile, string pathToCreatedLink);
    }
}