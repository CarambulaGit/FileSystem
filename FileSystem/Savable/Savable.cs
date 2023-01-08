using System;
using System.Runtime.Serialization;
using HardDrive;

namespace FileSystem.Savable
{
    public abstract class Savable<T> : ISerializable
    {
        public Inode Inode { get; private set; }
        public byte[] Content { get; set; }

        public static TChild CreateInstance<TChild>(Type type, Inode inode) where TChild : Savable<T> =>
            (TChild) Activator.CreateInstance(type, inode);

        protected Savable(Inode inode)
        {
            Inode = inode;
        }

        public abstract int LinksCountDefault();

        public abstract T GetContent();

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Content), Content);
        }
    }
}