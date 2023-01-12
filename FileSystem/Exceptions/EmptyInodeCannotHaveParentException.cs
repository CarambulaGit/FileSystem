using System;

namespace FileSystem.Exceptions
{
    public class EmptyInodeCannotHaveParentException : Exception
    {
        private const string DefaultMessage = "Empty inode cannot have parent";

        public EmptyInodeCannotHaveParentException() : base(DefaultMessage) { }
    }
}