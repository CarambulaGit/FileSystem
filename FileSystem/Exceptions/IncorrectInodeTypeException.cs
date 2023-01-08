using System;
using HardDrive;

namespace FileSystem.Exceptions
{
    public class IncorrectInodeTypeException : Exception
    {
        private const string DefaultMessage = "Incorrect inode type";

        public IncorrectInodeTypeException(Inode inode, FileType desiredType) : base(
            $"{DefaultMessage}\nDesired type = {desiredType}, given inode type = {inode.FileType}\nInode = {inode}") { }
    }
}