using System;
using SerDes;

namespace HardDrive
{
    [Serializable]
    public class BitmapSection : HardDriveSection
    {
        private const int EmptyArraySize = 224;
        private const int ArrayElemSize = 8;

        public bool[] OccupiedMask { get; private set; }

        public BitmapSection(int size, IHardDrive hardDrive, bool initFromDrive = false) : base(size, hardDrive,
            initFromDrive) { }

        public override byte[] ReadSection() =>
            HardDrive.Read(EmptyArraySize + ArrayElemSize * Size).BinaryCharsArrayToByteArray();

        public override void SaveSection() => HardDrive.Write(OccupiedMask.ToByteArray());

        protected override void InitData() => OccupiedMask = new bool[Size];
        protected override void InitFromData(byte[] data) => OccupiedMask = data.To<bool[]>();
    }
}