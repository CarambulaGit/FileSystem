using System.IO;

namespace SerDes
{
    public class SerDes : ISerDes
    {
        public char[] Read(FileStream fileStream, int toRead, int offset = 0)
        {
            using var b = new StreamReader(fileStream);
            b.BaseStream.Seek(offset, SeekOrigin.Begin);
            var by = new char[toRead]; 
            b.Read(by);
            return by;
        }

        public void Write(FileStream fileStream, byte[] toWrite, long offset = 0)
        {
            using var b = new StreamWriter(fileStream);
            b.BaseStream.Seek(offset, SeekOrigin.Begin);
            b.Write(toWrite);
        }

        public void Write(FileStream fileStream, string toWrite, long offset = 0)
        {
            // File.WriteAllBytes(); 
            using var b = new StreamWriter(fileStream);
            b.BaseStream.Seek(offset, SeekOrigin.Begin);
            b.Write(toWrite);
        }
    }
}