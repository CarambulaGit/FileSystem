using System;
using SerDes;

namespace HardDrive
{
    [Serializable]
    public abstract class HardDriveSection
    {
        private bool _initFromDrive;
        public int Size { get; private set; }
        public IHardDrive HardDrive { get; private set; }
        protected ISerDes SerDes { get; private set; }

        protected HardDriveSection(int size, ISerDes serDes, IHardDrive hardDrive, bool initFromDrive = false)
        {
            SerDes = serDes;
            Size = size;
            HardDrive = hardDrive;
            _initFromDrive = initFromDrive;
        }

        protected void Initialize()
        {
            if (_initFromDrive)
            {
                var data = ReadSection();
                InitFromData(data);
            }
            else
            {
                InitData();
                SaveSection();
            }
        }

        public abstract int Length(); // size in bites
        public abstract byte[] ReadSection();
        public abstract void SaveSection();

        protected abstract void InitData();
        protected abstract void InitFromData(byte[] data);
    }
}