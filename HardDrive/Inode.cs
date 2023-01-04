using System;

namespace HardDrive
{
    [Serializable]
    public class Inode
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public FileType FileType { get; set; }
        public int LinksCount { get; set; }
        public int FileSize { get; set; }
        public BlockAddress[] OccupiedDataBlocks { get; set; }
    }
}