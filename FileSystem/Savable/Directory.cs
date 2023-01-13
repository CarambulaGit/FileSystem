using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HardDrive;
using SerDes;
using Utils;

namespace FileSystem.Savable
{
    public class Directory : Savable<Directory.DirectoryContent>
    {
        private const int DefaultLinksCount = 2;

        [Serializable]
        public struct DirectoryContent
        {
            public const int DefaultNumOfChildren = 2;
            [NotNull] public List<(int id, string name)> ChildrenInodeData { get; set; }

            public override bool Equals(object obj)
            {
                if (obj is not DirectoryContent item)
                {
                    return false;
                }

                return ChildrenInodeData.ContentsMatchOrdered(item.ChildrenInodeData);
            }

            public override string ToString() => $"\n\t Children ids = {ChildrenInodeData.ToStr()}";
        }

        public Directory(Inode inode) : base(inode) { }

        public Directory(Inode inode, int parentDirectoryInodeId, string parentName) : base(inode)
        {
            var content = new DirectoryContent()
            {
                ChildrenInodeData = new List<(int, string)>()
                {
                    (parentDirectoryInodeId, parentName),
                    (inode.Id, inode.FileNames[0])
                }
            };

            Content = content.ToByteArray();
        }

        public override int LinksCountDefault() => DefaultLinksCount;

        public override DirectoryContent GetContent() => Content.To<DirectoryContent>();

        public int GetParentDirectoryInodeId() => GetParentDirectoryInodeId(GetContent());

        public int GetParentDirectoryInodeId(DirectoryContent directoryContent) => directoryContent.ChildrenInodeData[0].id;

        public override string ToString() => GetContent().ToString();
    }
}