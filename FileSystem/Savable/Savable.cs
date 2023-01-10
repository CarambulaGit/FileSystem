using System;
using HardDrive;

namespace FileSystem.Savable
{
    public abstract class Savable<T> : BaseSavable
    {
        public static TChild CreateInstance<TChild>(Type type, Inode inode) where TChild : Savable<T> =>
            (TChild) Activator.CreateInstance(type, inode);

        protected Savable(Inode inode) : base(inode) { }

        public abstract T GetContent();
    }
}