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
        private int EmptyBoolArrayLength => SerDes.EmptyBoolArrayLength;
        private int ArrayElemLength => SerDes.BoolArrayElemLength;

        public bool[] OccupiedMask { get; private set; }

        public int FreeBlocksAmount => OccupiedMask.Count(elem => elem == false);

        public BitmapSection(int size, ISerDes serDes, IHardDrive hardDrive, bool initFromDrive = false) : base(size,
            serDes, hardDrive,
            initFromDrive)
        {
            Initialize();
        }

        public override int Length() => EmptyBoolArrayLength + ArrayElemLength * Size;

        public override byte[] ReadSection() =>
            HardDrive.Read(Length()).BinaryCharsArrayToByteArray();

        public override void SaveSection() => HardDrive.Write(OccupiedMask.ToByteArray());

        public int GetFreeBlockIndex() => OccupiedMask.IndexOf(elem => elem == false);

        public int[] GetFreeBlocksIndexes(int amount) =>
            OccupiedMask.FirstNIndexes(amount, elem => elem == false).ToArray();

        public void SetOccupied(params int[] indexes) => ChangeBitsState(indexes, true);

        public void Release(params int[] indexes) => ChangeBitsState(indexes, false);

        protected override void InitData() => OccupiedMask = new bool[Size];

        protected override void InitFromData(byte[] data) => OccupiedMask = data.To<bool[]>();

        private void ChangeBitsState(IEnumerable<int> indexes, bool newState)
        {
            foreach (var index in indexes)
            {
                OccupiedMask[index] = newState;
            }
        }
    }
}