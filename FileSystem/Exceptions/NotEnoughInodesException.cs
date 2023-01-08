using System;

namespace FileSystem.Exceptions
{
    public class NotEnoughInodesException : Exception
    {
        private const string DefaultMessage = "Not enough inodes";

        public NotEnoughInodesException() : base(DefaultMessage) { }
    }
}