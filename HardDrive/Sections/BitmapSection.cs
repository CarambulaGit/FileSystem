using System;

namespace HardDrive
{
    [Serializable]
    public class BitmapSection : HardDriveSection
    {
        public bool[] OccupiedMask { get; private set; }

        public BitmapSection(int size) : base(size)
        {
            OccupiedMask = new bool[size];
        }
    }
}