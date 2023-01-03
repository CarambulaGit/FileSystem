using System.IO;

namespace HardDrive
{
    public class HardDrive : IHardDrive
    {
        public const string DataFileName = "data.txt";
        public FileStream FileStream() => File.Open(DataFileName, FileMode.OpenOrCreate);
    }
}