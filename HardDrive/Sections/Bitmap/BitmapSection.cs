using System;
using SerDes;

namespace HardDrive
{
    [Serializable]
    public class BitmapSection : HardDriveSection
    {
        private const int EmptyArrayLength = 224;
        private const int ArrayElemLength = 8;

        public bool[] OccupiedMask { get; private set; }

        public BitmapSection(int size, IHardDrive hardDrive, bool initFromDrive = false) : base(size, hardDrive,
            initFromDrive) { }

        public override int Length() => EmptyArrayLength + ArrayElemLength * Size;

        public override byte[] ReadSection() =>
            HardDrive.Read(Length()).BinaryCharsArrayToByteArray();

        public override void SaveSection() => HardDrive.Write(OccupiedMask.ToByteArray());

        protected override void InitData() => OccupiedMask = new bool[Size];
        protected override void InitFromData(byte[] data) => OccupiedMask = data.To<bool[]>();
    }
}