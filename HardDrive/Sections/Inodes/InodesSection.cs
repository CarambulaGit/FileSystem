using System;
using System.Linq;
using System.Text;
using SerDes;
using Utils;

namespace HardDrive
{
    [Serializable]
    public class InodesSection : HardDriveSection
    {
        private int _bitmapSize;
        public Inode[] Inodes { get; private set; }

        public InodesSection(int size, IHardDrive hardDrive, int bitmapSize, bool initFromDrive = false) : base(size,
            hardDrive,
            initFromDrive)
        {
            _bitmapSize = bitmapSize;
        }

        public override int Length() => Inode.InodeLength * Size;

        public int FreeInodesAmount() => Inodes.Count(inode => !inode.IsOccupied);

        public override byte[] ReadSection() =>
            HardDrive.Read(Length(), _bitmapSize).BinaryCharsArrayToByteArray();

        public override void SaveSection()
        {
            var sb = new StringBuilder();
            foreach (var node in Inodes)
            {
                sb.Append(node.GetBinaryStr());
            }

            HardDrive.Write(sb.ToString(), _bitmapSize);
        }

        public void SaveInode(Inode inode) =>
            HardDrive.Write(inode.GetBinaryStr(), _bitmapSize + inode.Id * Inode.InodeLength);

        public Inode GetFreeInode() => Inodes.FirstOrDefault(elem => !elem.IsOccupied);

        protected override void InitData()
        {
            Inodes = new Inode[Size];
            Inodes.FillWith(i => new Inode {Id = i});
        }

        protected override void InitFromData(byte[] data)
        {
            var inodeByteLength = Inode.InodeByteLength;
            Inodes = new Inode[data.Length / inodeByteLength];
            for (int i = 0, b = 0; i < data.Length; i += inodeByteLength, b++)
            {
                Inodes[b] = data[i..(i + inodeByteLength)].To<Inode>();
            }
        }
    }
}