using System;
using SerDes;

namespace HardDrive
{
    [Serializable]
    public class Inode
    {
        public const int InodeSize = InodeByteSize * 8;
        public const int InodeByteSize = 512;
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public FileType FileType { get; set; }
        public int LinksCount { get; set; }
        public int FileSize { get; set; }
        public BlockAddress[] OccupiedDataBlocks { get; set; } = Array.Empty<BlockAddress>();

        public bool IsOccupied => LinksCount > 0;

        public string GetBinaryStr() => this.ToByteArray().ByteArrayToBinaryStr().PadRight(InodeSize, '0');

        public override bool Equals(object obj)
        {
            if (obj is not Inode item)
            {
                return false;
            }

            return Id.Equals(item.Id) &&
                   FileName.Equals(item.FileName) &&
                   FileType.Equals(item.FileType) &&
                   LinksCount.Equals(item.LinksCount) &&
                   FileSize.Equals(item.FileSize) &&
                   OccupiedDataBlocks.ContentsMatch(item.OccupiedDataBlocks);
        }
    }
}