using System;

namespace FileSystem.Exceptions
{
    public class CannotChangeCurrentDirectoryException : Exception
    {
        private const string DefaultMessage = "Cannot change current directory";

        public CannotChangeCurrentDirectoryException(string path, string name) : base(
            $"{DefaultMessage}\nCan't find folder with name = {name}, at path = {path}") { }
    }
}