using SerDes;

namespace HardDrive
{
    public class DataBlock
    {
        public const int BlockLength = 4096;

        public IHardDrive HardDrive { get; private set; }
        public int Offset { get; private set; }

        public DataBlock(IHardDrive hardDrive, int offset)
        {
            HardDrive = hardDrive;
            Offset = offset;
        }

        public byte[] Read() => HardDrive.Read(BlockLength, Offset).BinaryCharsArrayToByteArray();
        public void Write(byte[] toWrite) => HardDrive.Write(toWrite.ByteArrayToBinaryStr().PadRight(BlockLength, '0').ToByteArray(), Offset);
    }
}