﻿using System;
using HardDrive;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SerDes;

namespace FileSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var inodeAmount = 20;
            var dataBlocksAmount = 40;
            var initFromDrive = false;
            var services = SetupDI(args, (inodeAmount, dataBlocksAmount, initFromDrive));
            var fileSystem = services.GetRequiredService<IFileSystem>();
            fileSystem.Initialize();
        }

        public static IServiceProvider SetupDI(string[] args,
            (int inodesAmount, int dataBlocksAmount, bool initFromDrive) fileSystemConfiguration)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddScoped<IHardDrive, HardDrive.HardDrive>()
                        .AddScoped<ISerDes, SerDes.SerDes>()
                        .AddScoped<IPathResolver, PathResolver>(serviceProvider => new PathResolver(
                            new Lazy<IFileSystem>(serviceProvider.GetRequiredService<IFileSystem>)))
                        .AddScoped<IFileSystem, FileSystem>(serviceProvider =>
                        {
                            var hardDrive = serviceProvider.GetRequiredService<IHardDrive>();
                            var pathResolver = serviceProvider.GetRequiredService<IPathResolver>();
                            return new FileSystem(hardDrive, pathResolver, fileSystemConfiguration.inodesAmount,
                                fileSystemConfiguration.dataBlocksAmount, fileSystemConfiguration.initFromDrive);
                        });
                }).Build();

            return host.Services;
        }
    }
}