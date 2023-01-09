using System;
using System.Collections.Generic;
using System.Linq;
using SerDes;
using Utils;

namespace HardDrive
{
    [Serializable]
    public class BitmapSection : HardDriveSection
    {
        private const int EmptyArrayLength = 224;
        private const int ArrayElemLength = 8;

        public bool[] OccupiedMask { get; private set; }

        public int FreeBlocksAmount => OccupiedMask.Count(elem => elem == false);

        public BitmapSection(int size, IHardDrive hardDrive, bool initFromDrive = false) : base(size, hardDrive,
            initFromDrive)
        {
            Initialize();
        }

        public override int Length() => EmptyArrayLength + ArrayElemLength * Size;

        public override byte[] ReadSection() =>
            HardDrive.Read(Length()).BinaryCharsArrayToByteArray();

        public override void SaveSection() => HardDrive.Write(OccupiedMask.ToByteArray());

        public int GetFreeBlockIndex() => OccupiedMask.IndexOf(elem => elem == false);

        public int[] GetFreeBlocksIndexes(int amount) =>
            OccupiedMask.FirstNIndexes(amount, elem => elem == false).ToArray();

        public void SetOccupied(IEnumerable<int> indexes)
        {
            foreach (var index in indexes)
            {
                OccupiedMask[index] = true;
            }
        }

        protected override void InitData() => OccupiedMask = new bool[Size];

        protected override void InitFromData(byte[] data) => OccupiedMask = data.To<bool[]>();
    }
}