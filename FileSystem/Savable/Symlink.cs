using System;
using HardDrive;
using SerDes;

namespace FileSystem.Savable
{
    public class Symlink : Savable<Symlink.SymlinkContent>
    {
        [Serializable]
        public struct SymlinkContent
        {
            public string Address { get; private set; }
        }

        public Symlink(Inode inode) : base(inode) { }

        public override SymlinkContent GetContent() => Content.To<SymlinkContent>();
    }
}