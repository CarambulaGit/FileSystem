using System;

namespace HardDrive
{
    [Serializable]
    public abstract class HardDriveSection
    {
        public int Size { get; set; }

        protected HardDriveSection(int size)
        {
            Size = size;
        }
    }
}