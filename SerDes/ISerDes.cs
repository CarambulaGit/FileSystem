using System.IO;

namespace SerDes
{
    public interface ISerDes
    {
        char[] Read(FileStream fileStream, int toRead, int offset = 0);
        void Write(FileStream fileStream, byte[] toWrite, long offset = 0);
        void Write(FileStream fileStream, string toWrite, long offset = 0);
    }
}