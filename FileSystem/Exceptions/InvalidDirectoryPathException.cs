using System;

namespace FileSystem.Exceptions
{
    public class InvalidDirectoryPathException : Exception
    {
        private const string DefaultMessage = "Invalid directory path";

        public InvalidDirectoryPathException(string reason) : base($"{DefaultMessage}\n{reason}") { }
    }
}