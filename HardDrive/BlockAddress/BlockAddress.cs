using System;

namespace HardDrive
{
    [Serializable]
    public class BlockAddress : DataBlockContainer
    {
        public int Address { get; private set; }

        public BlockAddress(int address)
        {
            Address = address;
        }

        public override bool Equals(object obj)
        {
            if (obj is not BlockAddress item)
            {
                return false;
            }

            return Address.Equals(item.Address);
        }
    }
}