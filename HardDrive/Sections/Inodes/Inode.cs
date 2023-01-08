using System;
using System.Collections.Generic;
using SerDes;
using Utils;

namespace HardDrive
{
    [Serializable]
    public class Inode
    {
        public const int InodeLength = InodeByteLength * 8;
        public const int InodeByteLength = 1024;
        public int Id { get; set; }
        public List<string> FileNames { get; set; } = new List<string>();
        public FileType FileType { get; set; } = FileType.None;
        public int LinksCount { get; set; }
        public int FileSize => OccupiedDataBlocks.Length * InodeByteLength;
        public BlockAddress[] OccupiedDataBlocks { get; set; } = Array.Empty<BlockAddress>();

        public bool IsOccupied => LinksCount > 0;

        public string GetBinaryStr() => this.ToByteArray().ByteArrayToBinaryStr().PadRight(InodeLength, '0');

        public override bool Equals(object obj)
        {
            if (obj is not Inode item)
            {
                return false;
            }

            return Id.Equals(item.Id) &&
                   FileNames.ContentsMatch(item.FileNames) &&
                   FileType.Equals(item.FileType) &&
                   LinksCount.Equals(item.LinksCount) &&
                   OccupiedDataBlocks.ContentsMatch(item.OccupiedDataBlocks);
        }
    }
}