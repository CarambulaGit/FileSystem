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
        Directory CreateDirectory(string path);
        Directory CreateDirectory(string name, string path);
        Directory ReadDirectory(string path);
        Directory ReadDirectory(Inode inode);
        void SaveDirectory(Directory directory);
        void DeleteDirectory(string path);
        void DeleteDirectory(Directory directory);
        RegularFile CreateFile(string path);
        RegularFile CreateFile(string name, string path);
        RegularFile ReadFile(string path);
        RegularFile ReadFile(Inode inode);
        void SaveFile(RegularFile file);
        void DeleteFile(string path);
        void DeleteFile(RegularFile file);
        void LinkFile(string pathToFile, string pathToCreatedLink);
        Inode GetInodeByPath(string path, out Inode parentInode);
        void ChangeCurrentDirectory(string path);
        void SaveDirectory(Directory directory, Directory.DirectoryContent content);
        void SaveFile(RegularFile file, RegularFile.RegularFileContent content);
        Symlink CreateSymlink(string path, string pathToLink);
        Symlink CreateSymlink(string name, string path, string pathToLink);
        Symlink ReadSymlink(string path);
        Symlink ReadSymlink(Inode inode);
        void SaveSymlink(Symlink symlink);
        void SaveSymlink(Symlink symlink, string pathToLink);
        void DeleteSymlink(Symlink symlink);
        void DeleteSymlink(string path);
        string GetSavableContentString(string path);
    }
}