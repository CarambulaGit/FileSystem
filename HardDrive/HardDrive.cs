using System.IO;
using SerDes;

namespace HardDrive
{
    public class HardDrive : IHardDrive
    {
        public const string DataFileName = "data.txt";
        private readonly ISerDes _serDes;
        private string _fileName;

        public HardDrive(ISerDes serDes, string fileName = DataFileName)
        {
            _fileName = fileName;
            _serDes = serDes;
        }

        public char[] Read(int toRead, int offset = 0) => _serDes.Read(FileStream(), toRead, offset);
        public void Write(byte[] toWrite, int offset = 0) => Write(toWrite.ByteArrayToBinaryStr(), offset);
        public void Write(string binaryStringToWrite, int offset = 0) => _serDes.Write(FileStream(), binaryStringToWrite, offset);
        private FileStream FileStream() => File.Open(_fileName, FileMode.OpenOrCreate);
    }
}