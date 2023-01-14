using System;

namespace FileSystem.Exceptions
{
    public class NotInitializedDescriptorException : Exception
    {
        private const string DefaultMessage = "Not initialized descriptor";

        public NotInitializedDescriptorException(string descriptor) : base(
            $"{DefaultMessage}\nDescriptor {descriptor} not found") { }
    }
}