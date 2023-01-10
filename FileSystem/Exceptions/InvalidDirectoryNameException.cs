using System;

namespace FileSystem.Exceptions
{
    public class InvalidDirectoryNameException : Exception
    {
        private const string DefaultMessage = "Invalid folder name";

        public InvalidDirectoryNameException() : base(DefaultMessage) { }
    }
}