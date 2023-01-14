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
        void LinkFile(string pathToFile, string pathToCreatedLink);
        Inode GetInodeByPath(string path, out Inode parentInode);
        void ChangeCurrentDirectory(string path);
        void SaveDirectory(Directory directory, Directory.DirectoryContent content);
        void SaveFile(RegularFile file, RegularFile.RegularFileContent content);
        Symlink CreateSymlink(string pathForLink, string pathToSavable);
        Symlink CreateSymlink(string name, string path, string pathToSavable);
        Symlink ReadSymlink(string path);
        Symlink ReadSymlink(Inode inode);
        void SaveSymlink(Symlink symlink);
        void SaveSymlink(Symlink symlink, string pathToLink);
        void DeleteSymlink(string path);
        string GetSavableContentString(string path);
        byte[] ReadFile(string descriptor, int numOfBytesToRead);
        void SaveFile(string descriptor, byte[] dataToWrite);
        void OpenFile(string path, string descriptor);
        void OpenFile(RegularFile file, string descriptor);
        void CloseFile(string descriptor);
        void SeekFile(string descriptor, int offsetInBytes);
        void TruncateFile(string path, int size);
        string GetInodeData(string path);
        string GetCWDData();
    }
}