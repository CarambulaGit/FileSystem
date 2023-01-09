using System;
using FileSystem;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace PathResolverTests
{
    public class FileSystemTests
    {
        private IFileSystem _fileSystem;
        private IPathResolver _pathResolver;
        private IServiceProvider _services;

        [SetUp]
        public void Setup()
        {
            (int inodesAmount, int dataBlocksAmount, bool initFromDrive) fileSystemConfiguration = (20, 40, false);
            _services = Program.SetupDI(Array.Empty<string>(), fileSystemConfiguration);
            _fileSystem = _services.GetRequiredService<IFileSystem>();
            _pathResolver = _services.GetRequiredService<IPathResolver>();
            _fileSystem.Initialize();
        }

        [Test]
        public void CreateRootTest()
        {
            var root = _fileSystem.ReadDirectory(_fileSystem.InodesSection.Inodes[FileSystem.FileSystem.RootFolderInodeId]);
            Assert.AreEqual(_fileSystem.RootDirectory.GetContent(), root.GetContent());
        }

        [Test]
        public void CreateFolderTest()
        {
            var dir = _fileSystem.CreateDirectory("Andrew loh", _fileSystem.RootPath);
            var root = _fileSystem.ReadDirectory(_fileSystem.InodesSection.Inodes[FileSystem.FileSystem.RootFolderInodeId]);
            Assert.AreEqual(_fileSystem.RootDirectory.GetContent(), root.GetContent());
            Assert.IsTrue(_fileSystem.RootDirectory.GetContent().ChildrenInodeIds.Count == 3);
            Assert.AreEqual(dir.GetContent(), _fileSystem.ReadDirectory(_fileSystem.InodesSection.Inodes[1]).GetContent());
        }
    }
}