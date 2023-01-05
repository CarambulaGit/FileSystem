using System.IO;

namespace HardDrive
{
    public interface IHardDrive
    {
        char[] Read(int toRead, int offset = 0);
        void Write(byte[] toWrite, int offset = 0);
        void Write(string binaryStringToWrite, int offset = 0);
    }
}