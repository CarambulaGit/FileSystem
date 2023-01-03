using System.IO;

namespace HardDrive
{
    public interface IHardDrive
    {
        FileStream FileStream();
    }
}