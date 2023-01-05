using System.IO;

namespace SerDes
{
    public class SerDes : ISerDes
    {
        public char[] Read(FileStream fileStream, int toRead, int offset = 0)
        {
            using StreamReader b = new StreamReader(fileStream);
            // b.BaseStream.Seek(offset, SeekOrigin.Begin);
            char[] by = new char[toRead]; 
            b.Read(by, offset, toRead);
            return by;
        }

        public void Write(FileStream fileStream, byte[] toWrite, long offset = 0)
        {
            using StreamWriter b = new StreamWriter(fileStream);
            b.BaseStream.Seek(offset, SeekOrigin.Begin);
            b.Write(toWrite);
        }

        public void Write(FileStream fileStream, string toWrite, long offset = 0)
        {
            // File.WriteAllBytes(); 
            using StreamWriter b = new StreamWriter(fileStream);
            b.BaseStream.Seek(offset, SeekOrigin.Begin);
            b.Write(toWrite);
        }
    }
}