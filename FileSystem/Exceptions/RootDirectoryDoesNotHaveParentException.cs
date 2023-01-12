using System;

namespace FileSystem.Exceptions
{
    public class RootDirectoryDoesNotHaveParentException : Exception
    {
        private const string DefaultMessage = "Root directory doesn't have parent";

        public RootDirectoryDoesNotHaveParentException() : base(DefaultMessage) { }
    }
}