using System;

namespace FileSystem.Exceptions
{
    public abstract class CannotDeleteSavableException : Exception
    {
        protected CannotDeleteSavableException(string message) : base(message) { }
    }

    public class SavableNotExistsException : CannotDeleteSavableException
    {
        private const string DefaultMessage = "Savable not exists";

        public SavableNotExistsException(string path) : base($"{DefaultMessage}\nCan't find savable at path {path}") { }
    }

    public class DirectoryHasChildrenException : CannotDeleteSavableException
    {
        private const string DefaultMessage = "Directory has children";

        public DirectoryHasChildrenException() : base($"{DefaultMessage}") { }
    }

    public class CannotDeleteRootDirectory : CannotDeleteSavableException
    {
        private const string DefaultMessage = "Can't delete root directory";

        public CannotDeleteRootDirectory() : base(DefaultMessage) { }
    }
}