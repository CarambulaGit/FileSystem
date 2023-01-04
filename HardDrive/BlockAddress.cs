using System;

namespace HardDrive
{
    [Serializable]
    public class BlockAddress : DataBlockContainer
    {
        public int Address { get; private set; }
    }
}