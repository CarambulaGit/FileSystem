using System;
using System.Runtime.Serialization;
using HardDrive;

namespace FileSystem.Savable
{
    public abstract class BaseSavable : ISerializable
    {
        public Inode Inode { get; private set; }
        public byte[] Content { get; set; } = Array.Empty<byte>();

        protected BaseSavable(Inode inode)
        {
            Inode = inode;
        }

        public abstract int LinksCountDefault();

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Content), Content);
        }
    }
}