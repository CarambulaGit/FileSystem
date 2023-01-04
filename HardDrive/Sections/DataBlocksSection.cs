using System;

namespace HardDrive
{
    [Serializable]
    public class DataBlocksSection : HardDriveSection
    {
        public DataBlocksSection(int size) : base(size) { }
    }
}