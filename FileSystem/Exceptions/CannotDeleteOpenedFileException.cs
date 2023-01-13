using System;
using FileSystem.Savable;

namespace FileSystem.Exceptions
{
    public class CannotDeleteOpenedFileException : Exception
    {
        private const string DefaultMessage = "Opened file can't be deleted";
        public CannotDeleteOpenedFileException(RegularFile file) : base($"{DefaultMessage}\nFile = {file}") { }
    }
}