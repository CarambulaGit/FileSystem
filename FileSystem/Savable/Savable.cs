using System;
using HardDrive;

namespace FileSystem.Savable
{
    [Serializable]
    public abstract class Savable<T>
    {
        public Inode Inode { get; private set; }
        public byte[] Content { get; set; }

        public static TChild CreateInstance<TChild>(Type type, Inode inode) where TChild : Savable<T> =>
            (TChild) Activator.CreateInstance(type, inode);

        protected Savable(Inode inode)
        {
            Inode = inode;
        }

        public abstract T GetContent();
    }
}