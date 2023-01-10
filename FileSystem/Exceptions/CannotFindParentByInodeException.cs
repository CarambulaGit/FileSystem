using System;
using HardDrive;

namespace FileSystem
{
    public class CannotFindParentByInodeException : Exception
    {
        private const string DefaultMessage = "Can't find parent for inode";

        public CannotFindParentByInodeException(Inode inode) : base($"{DefaultMessage} = {inode}") { }
    }
}