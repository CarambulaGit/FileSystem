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
            public string Address { get; set; }
        }

        public Symlink(Inode inode, bool fillContentWithDefaultValue = true) : base(inode)
        {
            if (!fillContentWithDefaultValue) return;
            var content = new SymlinkContent()
            {
                Address = string.Empty
            };

            Content = content.ToByteArray();
        }

        public override int LinksCountDefault() => DefaultLinksCount;

        public override SymlinkContent GetContent() => Content.To<SymlinkContent>();
    }
}