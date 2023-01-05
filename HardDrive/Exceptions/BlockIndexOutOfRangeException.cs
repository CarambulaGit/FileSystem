using System;

namespace HardDrive
{
    [Serializable]
    public class BlockIndexOutOfRangeException : Exception
    {
        private const string DefaultMessage = "Data block index must be in range of DataBlocksSection";

        public BlockIndexOutOfRangeException(int length, int index) : base(
            $"{DefaultMessage}\nGiven index = {index}, DataBlocksSection length = {length}") { }
    }
}