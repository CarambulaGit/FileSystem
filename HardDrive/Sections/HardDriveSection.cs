using System;

namespace HardDrive
{
    [Serializable]
    public abstract class HardDriveSection
    {
        public int Size { get; private set; }
        public IHardDrive HardDrive { get; private set; }

        protected HardDriveSection(int size, IHardDrive hardDrive, bool initFromDrive = false)
        {
            Size = size;
            HardDrive = hardDrive;
            if (initFromDrive)
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

        public abstract byte[] ReadSection();
        public abstract void SaveSection();

        protected abstract void InitData();
        protected abstract void InitFromData(byte[] data);
    }
}