using System;

namespace HardDrive
{
    [Serializable]
    public class SingleNestedBlocksAddress : DataBlockContainer
    {
        public BlockAddress[] BlockAddresses { get; set; }
    }
}