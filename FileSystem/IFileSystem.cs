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
        void ReadDirectory();
        void SaveDirectory(Directory directory);
        void DeleteDirectory();
        RegularFile CreateFile(string name);
        void ReadFile();
        void SaveFile();
        void DeleteFile();
    }
}