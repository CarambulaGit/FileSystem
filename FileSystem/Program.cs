using System;
using HardDrive;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
            using IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) =>
                    services.AddScoped<IHardDrive, HardDrive.HardDrive>()
                        .AddTransient<SerDes.SerDes>())
                .Build();
        }
    }
}