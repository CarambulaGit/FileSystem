using System;
using System.Collections.Generic;
using SerDes;

namespace HardDrive
{
    [Serializable]
    public class Inode
    {
        public const int InodeLength = InodeByteLength * 8;
        public const int InodeByteLength = 512;
        public int Id { get; set; }
        public List<string> FileNames { get; set; } = new List<string>();
        public FileType FileType { get; set; }
        public int LinksCount { get; set; }
        public int FileSize { get; set; }
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
                   FileNames.Equals(item.FileNames) &&
                   FileType.Equals(item.FileType) &&
                   LinksCount.Equals(item.LinksCount) &&
                   FileSize.Equals(item.FileSize) &&
                   OccupiedDataBlocks.ContentsMatch(item.OccupiedDataBlocks);
        }
    }
}