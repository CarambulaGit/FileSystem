using System;
using SerDes;

namespace HardDrive
{
    [Serializable]
    public class DataBlocksSection : HardDriveSection
    {
        private int _bitmapSize;
        private int _inodesSize;
        private DataBlock[] _dataBlocks;

        public DataBlocksSection(int size, IHardDrive hardDrive, int bitmapSize, int inodesSize,
            bool initFromDrive = false) : base(size, hardDrive,
            initFromDrive)
        {
            _bitmapSize = bitmapSize;
            _inodesSize = inodesSize;
        }

        public override byte[] ReadSection() => Array.Empty<byte>();

        public override void SaveSection() { }

        public byte[] ReadBlock(int index)
        {
            CheckBlock(index);
            return _dataBlocks[index].Read();
        }

        public void WriteBlock(int index, byte[] toWrite)
        {
            CheckBlock(index);
            _dataBlocks[index].Write(toWrite);
        }

        public void WriteBlock(int index, string toWrite)
        {
            CheckBlock(index);
            _dataBlocks[index].Write(toWrite.ToByteArray());
        }

        protected override void InitData() => InitDataBlocks();
        protected override void InitFromData(byte[] data) => InitDataBlocks();

        private void InitDataBlocks()
        {
            _dataBlocks = new DataBlock[Size];
            var prevSectionsOffset = _bitmapSize + _inodesSize;
            for (var i = 0; i < _dataBlocks.Length; i++)
            {
                _dataBlocks[i] = new DataBlock(HardDrive, prevSectionsOffset + i * DataBlock.BlockSize);
            }
        }

        private void CheckBlock(int index)
        {
            if (_dataBlocks.Length <= index)
                throw new BlockIndexOutOfRangeException(_dataBlocks.Length, index);
        }
    }
}