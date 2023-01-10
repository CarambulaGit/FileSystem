using System;
using System.IO;
using System.Linq;
using FileSystem;
using HardDrive;
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
            var root = _fileSystem.ReadDirectory(
                _fileSystem.InodesSection.Inodes[FileSystem.FileSystem.RootFolderInodeId]);
            Assert.AreEqual(_fileSystem.RootDirectory.GetContent(), root.GetContent());
        }

        [Test]
        public void CreateFolderTest()
        {
            var name = "Andrew loh";
            var dir = _fileSystem.CreateDirectory(name, _fileSystem.RootPath);
            var root = _fileSystem.ReadDirectory(
                _fileSystem.InodesSection.Inodes[FileSystem.FileSystem.RootFolderInodeId]);
            Assert.AreEqual(_fileSystem.RootDirectory.GetContent(), root.GetContent());
            Assert.IsTrue(_fileSystem.RootDirectory.GetContent().ChildrenInodeIds.Count == 3);
            Assert.AreEqual(dir.GetContent(),
                _fileSystem.ReadDirectory(_fileSystem.InodesSection.Inodes[1]).GetContent());
        }

        [Test]
        public void DeleteFolderTest()
        {
            var name = "Andrew loh";
            var dir = _fileSystem.CreateDirectory(name, _fileSystem.RootPath);
            var numOfChildren = _fileSystem.RootDirectory.GetContent().ChildrenInodeIds.Count;
            var folderInode = _fileSystem.InodesSection.Inodes.First(inode => inode.FileNames.Contains(name));
            _fileSystem.DeleteDirectory(_fileSystem.ReadDirectory(folderInode));
            var root = _fileSystem.ReadDirectory(
                _fileSystem.InodesSection.Inodes[FileSystem.FileSystem.RootFolderInodeId]);
            Assert.AreEqual(_fileSystem.RootDirectory.GetContent(), root.GetContent());
            Assert.IsTrue(_fileSystem.RootDirectory.GetContent().ChildrenInodeIds.Count == numOfChildren - 1);
            Assert.IsTrue(!folderInode.IsOccupied && folderInode.FileNames.Count == 0 &&
                          folderInode.OccupiedDataBlocks.Length == 0 && folderInode.LinksCount == 0 &&
                          folderInode.FileType == FileType.None);
        }

        [Test]
        public void FindInodeTest()
        {
            var fileName = "Andrew loh";
            var dir = _fileSystem.CreateDirectory(fileName, _fileSystem.RootPath);
            var root = _fileSystem.ReadDirectory(
                _fileSystem.InodesSection.Inodes[FileSystem.FileSystem.RootFolderInodeId]);
            Assert.AreEqual(root.Inode, _fileSystem.GetInodeByPath(Path.AltDirectorySeparatorChar.ToString()));
            Assert.AreEqual(dir.Inode, _fileSystem.GetInodeByPath($"{_fileSystem.RootDirectoryPath}{fileName}"));
            Assert.AreEqual(dir.Inode, _fileSystem.GetInodeByPath(fileName));
        }
    }
}