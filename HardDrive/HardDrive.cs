using System.IO;
using SerDes;

namespace HardDrive
{
    public class HardDrive : IHardDrive
    {
        public const string DataFileName = "data.txt";
        private readonly ISerDes _serDes;

        public HardDrive(ISerDes serDes)
        {
            _serDes = serDes;
        }

        public char[] Read(int toRead, int offset = 0) => 
            _serDes.Read(FileStream(), toRead, offset);

        public void Write(byte[] toWrite, int offset = 0) => 
            _serDes.Write(FileStream(), toWrite, offset);

        private FileStream FileStream() => File.Open(DataFileName, FileMode.OpenOrCreate);
    }
}