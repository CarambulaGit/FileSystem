using System;
using System.IO;
using FileSystem;
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
            _fileSystem.Initialize();
            _pathResolver = _services.GetRequiredService<IPathResolver>();
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
            var dir = _fileSystem.CreateDirectory(name, _fileSystem.RootName);
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
            var dir = _fileSystem.CreateDirectory(name, _fileSystem.RootName);
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
            var dir = _fileSystem.CreateDirectory(fileName, _fileSystem.RootName);
            var root = _fileSystem.ReadDirectory(
                _fileSystem.InodesSection.Inodes[FileSystem.FileSystem.RootFolderInodeId]);
            Assert.AreEqual(root.Inode, _fileSystem.GetInodeByPath(Path.AltDirectorySeparatorChar.ToString(), out _));
            Assert.AreEqual(dir.Inode, _fileSystem.GetInodeByPath($"{_fileSystem.RootDirectoryPath}{fileName}", out _));
            Assert.AreEqual(dir.Inode, _fileSystem.GetInodeByPath(fileName, out _));
        }

        [Test]
        public void CreateRegularFileTest()
        {
            var name = "Andrew loh";
            var oldNumOfChildren = _fileSystem.RootDirectory.GetContent().ChildrenInodeIds.Count;
            var file = _fileSystem.CreateFile(name, _fileSystem.RootName);
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
            var file = _fileSystem.CreateFile(name, _fileSystem.RootName);
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

        [Test]
        public void ChangeCurrentDirectoryTest1()
        {
            var fileName = "Andrew loh";
            var dir = _fileSystem.CreateDirectory(fileName, _fileSystem.RootName);
            _fileSystem.ChangeCurrentDirectory(fileName);
            Assert.IsTrue(_fileSystem.CurrentDirectory.Inode.Id == dir.Inode.Id);
            _fileSystem.ChangeCurrentDirectory(_fileSystem.RootDirectoryPath);
            Assert.IsTrue(_fileSystem.CurrentDirectory.Inode.Id == _fileSystem.RootDirectory.Inode.Id);
        }

        [Test]
        public void ChangeCurrentDirectoryTest2()
        {
            var fileName = "Andrew loh";
            var dir = _fileSystem.CreateDirectory(fileName, _fileSystem.RootName);
            _fileSystem.ChangeCurrentDirectory(fileName);
            Assert.IsTrue(_fileSystem.CurrentDirectory.Inode.Id == dir.Inode.Id);
            _fileSystem.ChangeCurrentDirectory(_pathResolver.ParentDirectory);
            Assert.IsTrue(_fileSystem.CurrentDirectory.Inode.Id == _fileSystem.RootDirectory.Inode.Id);
        }

        [Test]
        public void ChangeCurrentDirectoryTest3()
        {
            var fileName = "Andrew loh";
            var dir1 = _fileSystem.CreateDirectory(fileName, _fileSystem.RootName);
            _fileSystem.ChangeCurrentDirectory(fileName);
            Assert.IsTrue(_fileSystem.CurrentDirectory.Inode.Id == dir1.Inode.Id);
            var dir2 = _fileSystem.CreateDirectory(fileName, _pathResolver.CurrentDirectory);
            _fileSystem.ChangeCurrentDirectory(fileName);
            Assert.IsTrue(_fileSystem.CurrentDirectory.Inode.Id == dir2.Inode.Id);
            _fileSystem.ChangeCurrentDirectory(_fileSystem.RootDirectoryPath);
            Assert.IsTrue(_fileSystem.CurrentDirectory.Inode.Id == _fileSystem.RootDirectory.Inode.Id);
        }
    }
}