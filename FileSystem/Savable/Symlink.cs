using System;
using HardDrive;
using SerDes;

namespace FileSystem.Savable
{
    public class Symlink : Savable<Symlink.SymlinkContent>
    {
        private const int DefaultLinksCount = 1;

        [Serializable]
        public struct SymlinkContent
        {
            public string Address { get; private set; }
        }

        public Symlink(Inode inode) : base(inode) { }

        public override int LinksCountDefault() => DefaultLinksCount;

        public override SymlinkContent GetContent() => Content.To<SymlinkContent>();
    }
}