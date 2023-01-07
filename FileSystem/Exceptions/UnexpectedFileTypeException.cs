using System;
using HardDrive;

namespace FileSystem.Savable
{
    public class UnexpectedFileTypeException : Exception
    {
        private const string DefaultMessage = "Unexpected file type";

        public UnexpectedFileTypeException(FileType fileType) : base(
            $"{DefaultMessage}\nGiven type = {fileType.ToString()}") { }

    }
}