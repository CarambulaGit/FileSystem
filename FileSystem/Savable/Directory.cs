using System;
using HardDrive;
using SerDes;

namespace FileSystem.Savable
{
    [Serializable]
    public class Directory : Savable<Directory.DirectoryContent>
    {
        [Serializable]
        public struct DirectoryContent
        {
            
        }

        public Directory(Inode inode) : base(inode) { }

        public override DirectoryContent GetContent() => Content.To<DirectoryContent>();
    }
}