using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HardDrive;
using SerDes;

namespace FileSystem.Savable
{
    public class Directory : Savable<Directory.DirectoryContent>
    {
        [Serializable]
        public struct DirectoryContent
        {
            [NotNull] public List<int> ChildrenInodeIds { get; set; }
        }

        public Directory(Inode inode) : base(inode) { }

        public override DirectoryContent GetContent() => Content.To<DirectoryContent>();

        public int GetParentDirectoryInodeId()
        {
            var content = GetContent();
            return content.ChildrenInodeIds[0];
        }
    }
}