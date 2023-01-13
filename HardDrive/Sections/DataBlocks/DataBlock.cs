using SerDes;

namespace HardDrive
{
    public class DataBlock
    {
        public const int BlockLengthInBytes = 4096;
        public const int BlockLength = BlockLengthInBytes * Constants.BitesInByte;

        public IHardDrive HardDrive { get; private set; }
        public int Offset { get; private set; }

        public DataBlock(IHardDrive hardDrive, int offset)
        {
            HardDrive = hardDrive;
            Offset = offset;
        }

        public char[] Read() => HardDrive.Read(BlockLength, Offset);
        // public void Write(byte[] toWrite) => HardDrive.Write(toWrite.ByteArrayToBinaryStr().PadRight(BlockLength, '0').ToByteArray(), Offset);
        public void Write(string binaryStrToWrite) => HardDrive.Write(binaryStrToWrite.PadRight(BlockLength, '0'), Offset);
    }
}