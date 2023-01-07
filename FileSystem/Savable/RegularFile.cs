using System;
using HardDrive;
using SerDes;

namespace FileSystem.Savable
{
    public class RegularFile : Savable<RegularFile.RegularFileContent>
    {
        [Serializable]
        public struct RegularFileContent
        {
            public string Text { get; private set; }
        }

        public RegularFile(Inode inode) : base(inode) { }

        public override RegularFileContent GetContent() => Content.To<RegularFileContent>();
    }
}