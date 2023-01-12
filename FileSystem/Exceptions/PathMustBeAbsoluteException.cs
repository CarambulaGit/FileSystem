using System;

namespace FileSystem.Exceptions
{
    public class PathMustBeAbsoluteException : Exception
    {
        private const string DefaultMessage = "Path must be absolute";

        public PathMustBeAbsoluteException(string path) : base($"{DefaultMessage}\nCan't split path = {path} for parts") { }
    }
}