using System;

namespace FileSystem.Exceptions
{
    public class InvalidDirectoryNameException : Exception
    {
        private const string DefaultMessage = "Not enough data blocks";

        public InvalidDirectoryNameException() : base(DefaultMessage) { }
    }
}