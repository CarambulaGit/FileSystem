using System;

namespace FileSystem
{
    public class CannotFindSavableException : Exception
    {
        private const string DefaultMessage = "Can't find savable";

        public CannotFindSavableException(string pathToSavable, string savableName) : base(
            $"{DefaultMessage} with name = {savableName}, at path = {pathToSavable}") { }
    }
}