using System;
using SerDes;

namespace HardDrive
{
    [Serializable]
    public class DataBlocksSection : HardDriveSection
    {
        private int _bitmapLength;
        private int _inodesLength;
        private DataBlock[] _dataBlocks;

        public DataBlocksSection(int size, IHardDrive hardDrive, int bitmapLength, int inodesLength,
            bool initFromDrive = false) : base(size, hardDrive, initFromDrive)
        {
            _bitmapLength = bitmapLength;
            _inodesLength = inodesLength;
            Initialize();
        }

        public override int Length() => Size * DataBlock.BlockLength;

        public override byte[] ReadSection() => Array.Empty<byte>();

        public override void SaveSection() { }

        public DataBlock GetDataBlock(int index) => _dataBlocks[index];

        public DataBlock[] GetDataBlocks(params int[] indexes)
        {
            var result = new DataBlock[indexes.Length];
            for (var i = 0; i < result.Length; i++)
            {
                result[i] = _dataBlocks[indexes[i]];
            }

            return result;
        }

        public char[] ReadBlock(int index)
        {
            CheckBlock(index);
            return _dataBlocks[index].Read();
        }

        // public void WriteBlock(int index, byte[] toWrite)
        // {
        //     CheckBlock(index);
        //     _dataBlocks[index].Write(toWrite);
        // }

        public void WriteBlock(int index, string binaryStrToWrite)
        {
            CheckBlock(index);
            _dataBlocks[index].Write(binaryStrToWrite);
        }

        // public void WriteBlock(int index, string toWrite) => WriteBlock(index, toWrite.ToByteArray());

        protected override void InitData() => InitDataBlocks();

        protected override void InitFromData(byte[] data) => InitDataBlocks();

        private void InitDataBlocks()
        {
            _dataBlocks = new DataBlock[Size];
            var prevSectionsOffset = _bitmapLength + _inodesLength;
            for (var i = 0; i < _dataBlocks.Length; i++)
            {
                _dataBlocks[i] = new DataBlock(HardDrive, prevSectionsOffset + i * DataBlock.BlockLength);
            }
        }

        private void CheckBlock(int index)
        {
            if (_dataBlocks.Length <= index)
                throw new BlockIndexOutOfRangeException(_dataBlocks.Length, index);
        }
    }
}