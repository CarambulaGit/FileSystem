using System;

namespace FileSystem.Exceptions
{
    public class NotEnoughDataBlocksException : Exception
    {
        private const string DefaultMessage = "Not enough data blocks";

        public NotEnoughDataBlocksException() : base(DefaultMessage) { }
    }
}