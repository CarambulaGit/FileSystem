using System;

namespace FileSystem.Exceptions
{
    public class InvalidFileNameException : Exception
    {
        private const string DefaultMessage = "Invalid file name";

        public InvalidFileNameException() : base(DefaultMessage) { }
    }
}