using System;

namespace HardDrive
{
    [Serializable]
    public class BlockAddress : DataBlockContainer
    {
        public int Address { get; private set; }
        
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