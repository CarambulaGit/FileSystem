using System;

namespace FileSystem.Exceptions
{
    public abstract class CannotOpenFileException : Exception
    {
        private const string DefaultMessage = "Can't open file";

        public CannotOpenFileException(string message) : base($"{DefaultMessage}\n{message}") { }
    }

    public class MaxNumOfOpenedFilesReachedException : CannotOpenFileException
    {
        private const string DefaultMessage =
            "Maximal number of opened files reached, close one of them to open new one";

        public MaxNumOfOpenedFilesReachedException() : base(DefaultMessage) { }
    }

    public class DescriptorAlreadyBusyException : CannotOpenFileException
    {
        private const string DefaultMessage = "Descriptor already busy";

        public DescriptorAlreadyBusyException() : base(DefaultMessage) { }
    }
}