using HardDrive;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SerDes;

namespace FileSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            SetupDI(args);
        }

        private static void SetupDI(string[] args)
        {
            using var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) =>
                    services.AddTransient<IHardDrive, HardDrive.HardDrive>()
                        .AddTransient<ISerDes, SerDes.SerDes>()
                        .AddTransient<IFileSystem, FileSystem>()
                ).Build();
        }
    }
}