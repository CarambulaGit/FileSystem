using System;

namespace FileSystem
{
    public class EmptyInodeCannotHaveParentException : Exception
    {
        private const string DefaultMessage = "Empty inode cannot have parent";

        public EmptyInodeCannotHaveParentException() : base(DefaultMessage) { }
    }
}