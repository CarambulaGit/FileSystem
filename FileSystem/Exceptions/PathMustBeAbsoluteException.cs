using System;

namespace FileSystem
{
    public class PathMustBeAbsoluteException : Exception
    {
        private const string DefaultMessage = "Path must be absolute";

        public PathMustBeAbsoluteException(string path) : base($"{DefaultMessage}\nCan't split path = {path} for parts") { }
    }
}