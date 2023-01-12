using System;

namespace FileSystem.Exceptions
{
    public abstract class InvalidSavableNameException : Exception
    {
        protected InvalidSavableNameException(string message) : base(message) { }
    }

    public class InvalidDirectoryNameException : InvalidSavableNameException
    {
        private const string DefaultMessage = "Invalid folder name";

        public InvalidDirectoryNameException() : base(DefaultMessage) { }
    }

    public class InvalidFileNameException : InvalidSavableNameException
    {
        private const string DefaultMessage = "Invalid file name";

        public InvalidFileNameException() : base(DefaultMessage) { }
    }

    public class InvalidSymlinkNameException : InvalidSavableNameException
    {
        private const string DefaultMessage = "Invalid symlink name";

        public InvalidSymlinkNameException() : base(DefaultMessage) { }
    }
}