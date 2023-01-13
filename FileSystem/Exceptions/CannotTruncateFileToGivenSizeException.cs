using System;

namespace FileSystem.Exceptions
{
    public class CannotTruncateFileToGivenSizeException : Exception
    {
        private const string DefaultMessage = "Can't truncate file to given size";

        public CannotTruncateFileToGivenSizeException(int size, int minSize) : base(
            $"{DefaultMessage}\nGiven size = {size}, minimal size = {minSize}") { }
    }
}