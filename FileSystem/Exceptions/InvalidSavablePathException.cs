using System;

namespace FileSystem.Exceptions
{
    public class InvalidSavablePathException : Exception
    {
        private const string DefaultMessage = "Invalid savable path";

        public InvalidSavablePathException(string reason) : base($"{DefaultMessage}\n{reason}") { }
    }
}