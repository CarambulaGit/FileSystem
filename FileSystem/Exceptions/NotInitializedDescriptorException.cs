using System;

namespace FileSystem.Exceptions
{
    public class NotInitializedDescriptorException : Exception
    {
        public NotInitializedDescriptorException(string descriptor) : base(descriptor) { }
    }
}