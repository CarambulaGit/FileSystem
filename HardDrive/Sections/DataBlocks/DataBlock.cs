using SerDes;

namespace HardDrive
{
    public class DataBlock
    {
        public const int BlockSize = 4096;

        public IHardDrive HardDrive { get; private set; }
        public int Offset { get; private set; }

        public DataBlock(IHardDrive hardDrive, int offset)
        {
            HardDrive = hardDrive;
            Offset = offset;
        }

        public byte[] Read() => HardDrive.Read(BlockSize, Offset).BinaryCharsArrayToByteArray();
        public void Write(byte[] toWrite) => HardDrive.Write(toWrite.ByteArrayToBinaryStr().ToByteArray(), Offset);
    }
}