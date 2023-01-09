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
            [NotNull] public List<int> ChildrenInodeIds { get; set; }

            public override bool Equals(object obj)
            {
                if (obj is not DirectoryContent item)
                {
                    return false;
                }

                return ChildrenInodeIds.ContentsMatchOrdered(item.ChildrenInodeIds);
            }
        }

        public Directory(Inode inode) : base(inode) { }

        public Directory(Inode inode, int parentDirectoryInodeId) : base(inode)
        {
            var content = new DirectoryContent()
            {
                ChildrenInodeIds = new List<int>()
                {
                    parentDirectoryInodeId,
                    inode.Id
                }
            };

            Content = content.ToByteArray();
        }

        public override int LinksCountDefault() => DefaultLinksCount;

        public override DirectoryContent GetContent() => Content.To<DirectoryContent>();

        public int GetParentDirectoryInodeId()
        {
            var content = GetContent();
            return content.ChildrenInodeIds[0];
        }
    }
}