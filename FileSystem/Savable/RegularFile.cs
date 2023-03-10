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
            public string Text { get; set; }

            public override string ToString() => Text;
        }

        public RegularFile(Inode inode, bool fillContentWithDefaultValue = true) : base(inode)
        {
            if (!fillContentWithDefaultValue) return;
            var content = new RegularFileContent()
            {
                Text = string.Empty
            };

            Content = content.ToByteArray();
        }

        public override int LinksCountDefault() => DefaultLinksCount;

        public override RegularFileContent GetContent() => Content.To<RegularFileContent>();

        public override bool Equals(object obj)
        {
            if (obj is not RegularFile item)
            {
                return false;
            }

            return Inode.Id == item.Inode.Id;
        }

        public override string ToString() => GetContent().ToString();
    }
}