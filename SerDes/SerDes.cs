using System.IO;
using HardDrive;

namespace SerDes
{
    public class SerDes
    {
        private readonly IHardDrive _hardDrive;

        public SerDes(IHardDrive hardDrive)
        {
            _hardDrive = hardDrive;
        }

        public byte[] Read(int toRead, int offset = 0)
        {
            using BinaryReader b = new BinaryReader(_hardDrive.FileStream());
            b.BaseStream.Seek(offset, SeekOrigin.Begin);
            byte[] by = b.ReadBytes(toRead);
            return by;
        }

        public void Write(byte[] toWrite, int offset = 0)
        {
            using BinaryWriter b = new BinaryWriter(_hardDrive.FileStream());
            b.BaseStream.Seek(offset, SeekOrigin.Begin);
            b.Write(toWrite);
        }
    }
}