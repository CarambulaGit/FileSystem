using System;
using System.IO;
using System.Linq;
using FileSystem;
using FileSystem.Savable;
using HardDrive;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SerDes;

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
            var oldNumOfChildren = _fileSystem.RootDirectory.GetContent().ChildrenInodeIds.Count;
            var dir = _fileSystem.CreateDirectory(name, _fileSystem.RootPath);
            var root = _fileSystem.ReadDirectory(
                _fileSystem.InodesSection.Inodes[FileSystem.FileSystem.RootFolderInodeId]);
            Assert.AreEqual(_fileSystem.RootDirectory.GetContent(), root.GetContent());
            Assert.IsTrue(_fileSystem.RootDirectory.GetContent().ChildrenInodeIds.Count == oldNumOfChildren + 1);
            Assert.AreEqual(dir.GetContent(),
                _fileSystem.ReadDirectory(_fileSystem.InodesSection.Inodes[dir.Inode.Id]).GetContent());
        }

        [Test]
        public void DeleteFolderTest()
        {
            var name = "Andrew loh";
            var dir = _fileSystem.CreateDirectory(name, _fileSystem.RootPath);
            var numOfChildren = _fileSystem.RootDirectory.GetContent().ChildrenInodeIds.Count;
            var folderInode = dir.Inode;
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

        [Test]
        public void CreateRegularFileTest()
        {
            var name = "Andrew loh";
            var oldNumOfChildren = _fileSystem.RootDirectory.GetContent().ChildrenInodeIds.Count;
            var file = _fileSystem.CreateFile(name, _fileSystem.RootPath);
            var fileContent = file.GetContent();
            fileContent.Text = name;
            file.Content = fileContent.ToByteArray();
            _fileSystem.SaveFile(file);
            var root = _fileSystem.ReadDirectory(
                _fileSystem.InodesSection.Inodes[FileSystem.FileSystem.RootFolderInodeId]);
            Assert.AreEqual(_fileSystem.RootDirectory.GetContent(), root.GetContent());
            Assert.IsTrue(_fileSystem.RootDirectory.GetContent().ChildrenInodeIds.Count == oldNumOfChildren + 1);
            Assert.AreEqual(file.GetContent(),
                _fileSystem.ReadFile(_fileSystem.InodesSection.Inodes[file.Inode.Id]).GetContent());
        }

        [Test]
        public void DeleteRegularFileTest()
        {
            var name = "Andrew loh";
            var file = _fileSystem.CreateFile(name, _fileSystem.RootPath);
            var fileInode = file.Inode;
            var oldNumOfChildren = _fileSystem.RootDirectory.GetContent().ChildrenInodeIds.Count;
            _fileSystem.DeleteFile(_fileSystem.ReadFile(fileInode));
            var root = _fileSystem.ReadDirectory(
                _fileSystem.InodesSection.Inodes[FileSystem.FileSystem.RootFolderInodeId]);

            Assert.AreEqual(_fileSystem.RootDirectory.GetContent(), root.GetContent());
            Assert.IsTrue(_fileSystem.RootDirectory.GetContent().ChildrenInodeIds.Count == oldNumOfChildren - 1);
            Assert.IsTrue(!fileInode.IsOccupied && fileInode.FileNames.Count == 0 &&
                          fileInode.OccupiedDataBlocks.Length == 0 && fileInode.LinksCount == 0 &&
                          fileInode.FileType == FileType.None);
        }
    }
}