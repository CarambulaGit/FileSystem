using System;
using System.Text;
using SerDes;

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

        protected override void InitData()
        {
            Inodes = new Inode[Size];
            Inodes.FillWith(() => new Inode());
        }

        protected override void InitFromData(byte[] data)
        {
            var inodeByteSize = Inode.InodeByteLength;
            Inodes = new Inode[data.Length / inodeByteSize];
            for (int i = 0, b = 0; i < data.Length; i += inodeByteSize, b++)
            {
                Inodes[b] = data[i..(i + inodeByteSize)].To<Inode>();
            }
        }
    }
}