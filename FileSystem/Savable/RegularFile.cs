using System;
using HardDrive;
using SerDes;

namespace FileSystem.Savable
{
    public class RegularFile : Savable<RegularFile.RegularFileContent>
    {
        private const int DefaultLinksCount = 1;

        [Serializable]
        public struct RegularFileContent
        {
            public string Text { get; private set; }
        }

        public RegularFile(Inode inode) : base(inode) { }

        public override int LinksCountDefault() => DefaultLinksCount;

        public override RegularFileContent GetContent() => Content.To<RegularFileContent>();
    }
}