using System;
using System.Linq;
using FileSystem;
using FileSystem.Savable;
using HardDrive;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SerDes;
using Utils;

namespace PathResolverTests
{
    public class FileSystemTests
    {
        private IFileSystem _fileSystem;
        private IPathResolver _pathResolver;
        private IServiceProvider _services;
        private ISerDes _serDes;

        [SetUp]
        public void Setup()
        {
            (int inodesAmount, int dataBlocksAmount, bool initFromDrive) fileSystemConfiguration = (20, 40, false);
            _services = Program.SetupDI(Array.Empty<string>(), fileSystemConfiguration);
            _fileSystem = _services.GetRequiredService<IFileSystem>();
            _pathResolver = _services.GetRequiredService<IPathResolver>();
            _serDes = _services.GetRequiredService<ISerDes>();
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
            var oldNumOfChildren = _fileSystem.RootDirectory.GetContent().ChildrenInodeData.Count;
            var dir = _fileSystem.CreateDirectory(name, _fileSystem.RootDirectoryPath);
            var root = _fileSystem.ReadDirectory(
                _fileSystem.InodesSection.Inodes[FileSystem.FileSystem.RootFolderInodeId]);
            Assert.AreEqual(_fileSystem.RootDirectory.GetContent(), root.GetContent());
            Assert.IsTrue(_fileSystem.RootDirectory.GetContent().ChildrenInodeData.Count == oldNumOfChildren + 1);
            Assert.AreEqual(dir.GetContent(),
                _fileSystem.ReadDirectory(_fileSystem.InodesSection.Inodes[dir.Inode.Id]).GetContent());
        }

        [Test]
        public void DeleteFolderTest()
        {
            var name = "Andrew loh";
            var dir = _fileSystem.CreateDirectory(name, _fileSystem.RootDirectoryPath);
            var numOfChildren = _fileSystem.RootDirectory.GetContent().ChildrenInodeData.Count;
            var folderInode = dir.Inode;
            _fileSystem.DeleteDirectory(_fileSystem.ReadDirectory(folderInode));
            var root = _fileSystem.ReadDirectory(
                _fileSystem.InodesSection.Inodes[FileSystem.FileSystem.RootFolderInodeId]);
            Assert.AreEqual(_fileSystem.RootDirectory.GetContent(), root.GetContent());
            Assert.IsTrue(_fileSystem.RootDirectory.GetContent().ChildrenInodeData.Count == numOfChildren - 1);
            Assert.IsTrue(!folderInode.IsOccupied && folderInode.FileNames.Count == 0 &&
                          folderInode.OccupiedDataBlocks.Length == 0 && folderInode.LinksCount == 0 &&
                          folderInode.FileType == FileType.None);
        }

        [Test]
        public void FindInodeTest()
        {
            var fileName = "Andrew loh";
            var dir = _fileSystem.CreateDirectory(fileName, _fileSystem.RootDirectoryPath);
            var root = _fileSystem.ReadDirectory(
                _fileSystem.InodesSection.Inodes[FileSystem.FileSystem.RootFolderInodeId]);
            Assert.AreEqual(root.Inode, _fileSystem.GetInodeByPath(_fileSystem.RootDirectoryPath, out _));
            Assert.AreEqual(dir.Inode, _fileSystem.GetInodeByPath($"{_fileSystem.RootDirectoryPath}{fileName}", out _));
            Assert.AreEqual(dir.Inode, _fileSystem.GetInodeByPath(fileName, out _));
        }

        [Test]
        public void CreateRegularFileTest()
        {
            var name = "Andrew loh";
            var oldNumOfChildren = _fileSystem.RootDirectory.GetContent().ChildrenInodeData.Count;
            var file = _fileSystem.CreateFile(name, _fileSystem.RootDirectoryPath);
            var fileContent = file.GetContent();
            fileContent.Text = name;
            file.Content = fileContent.ToByteArray();
            _fileSystem.SaveFile(file);
            var root = _fileSystem.ReadDirectory(
                _fileSystem.InodesSection.Inodes[FileSystem.FileSystem.RootFolderInodeId]);
            Assert.AreEqual(_fileSystem.RootDirectory.GetContent(), root.GetContent());
            Assert.IsTrue(_fileSystem.RootDirectory.GetContent().ChildrenInodeData.Count == oldNumOfChildren + 1);
            Assert.AreEqual(file.GetContent(),
                _fileSystem.ReadFile(_fileSystem.InodesSection.Inodes[file.Inode.Id]).GetContent());
        }

        [Test]
        public void DeleteRegularFileTest()
        {
            var name = "Andrew loh";
            var file = _fileSystem.CreateFile(name, _fileSystem.RootDirectoryPath);
            var fileInode = file.Inode;
            var oldNumOfChildren = _fileSystem.RootDirectory.GetContent().ChildrenInodeData.Count;
            _fileSystem.DeleteFile(name);
            var root = _fileSystem.ReadDirectory(
                _fileSystem.InodesSection.Inodes[FileSystem.FileSystem.RootFolderInodeId]);

            Assert.AreEqual(_fileSystem.RootDirectory.GetContent(), root.GetContent());
            Assert.IsTrue(_fileSystem.RootDirectory.GetContent().ChildrenInodeData.Count == oldNumOfChildren - 1);
            Assert.IsTrue(!fileInode.IsOccupied && fileInode.FileNames.Count == 0 &&
                          fileInode.OccupiedDataBlocks.Length == 0 && fileInode.LinksCount == 0 &&
                          fileInode.FileType == FileType.None);
        }

        [Test]
        public void ChangeCurrentDirectoryTest1()
        {
            var fileName = "Andrew loh";
            var dir = _fileSystem.CreateDirectory(fileName, _fileSystem.RootDirectoryPath);
            _fileSystem.ChangeCurrentDirectory(fileName);
            Assert.IsTrue(_fileSystem.CurrentDirectory.Inode.Id == dir.Inode.Id);
            _fileSystem.ChangeCurrentDirectory(_fileSystem.RootDirectoryPath);
            Assert.IsTrue(_fileSystem.CurrentDirectory.Inode.Id == _fileSystem.RootDirectory.Inode.Id);
        }

        [Test]
        public void ChangeCurrentDirectoryTest2()
        {
            var fileName = "Andrew loh";
            var dir = _fileSystem.CreateDirectory(fileName, _fileSystem.RootDirectoryPath);
            _fileSystem.ChangeCurrentDirectory(fileName);
            Assert.IsTrue(_fileSystem.CurrentDirectory.Inode.Id == dir.Inode.Id);
            _fileSystem.ChangeCurrentDirectory(_pathResolver.ParentDirectory);
            Assert.IsTrue(_fileSystem.CurrentDirectory.Inode.Id == _fileSystem.RootDirectory.Inode.Id);
        }

        [Test]
        public void ChangeCurrentDirectoryTest3()
        {
            var fileName = "Andrew loh";
            var dir1 = _fileSystem.CreateDirectory(fileName, _fileSystem.RootDirectoryPath);
            _fileSystem.ChangeCurrentDirectory(fileName);
            Assert.IsTrue(_fileSystem.CurrentDirectory.Inode.Id == dir1.Inode.Id);
            var dir2 = _fileSystem.CreateDirectory(fileName, _pathResolver.CurrentDirectory);
            _fileSystem.ChangeCurrentDirectory(fileName);
            Assert.IsTrue(_fileSystem.CurrentDirectory.Inode.Id == dir2.Inode.Id);
            _fileSystem.ChangeCurrentDirectory(_fileSystem.RootDirectoryPath);
            Assert.IsTrue(_fileSystem.CurrentDirectory.Inode.Id == _fileSystem.RootDirectory.Inode.Id);
        }

        [Test]
        public void LinkFileTest()
        {
            var dirName = "Andrew loh";
            var dir = _fileSystem.CreateDirectory(dirName, _fileSystem.RootDirectoryPath);
            var fileName = "file";
            var file = _fileSystem.CreateFile(fileName, _fileSystem.RootDirectoryPath);
            var fileContent = file.GetContent();
            fileContent.Text = dirName;
            file.Content = fileContent.ToByteArray();
            _fileSystem.SaveFile(file);
            var root = _fileSystem.ReadDirectory(
                _fileSystem.InodesSection.Inodes[FileSystem.FileSystem.RootFolderInodeId]);
            var hardlinkName = "hardlink";
            _fileSystem.LinkFile(fileName,
                _pathResolver.Combine(_fileSystem.RootName, dirName, hardlinkName));
            Assert.IsTrue(file.Inode.FileNames.Count == 2);
            Assert.IsTrue(file.Inode.LinksCount == 2);
            Assert.IsTrue(file.Inode.FileNames.Contains(fileName));
            Assert.IsTrue(file.Inode.FileNames.Contains(hardlinkName));
            dir = _fileSystem.ReadDirectory(dir.Inode);
            Assert.IsTrue(dir.GetContent().ChildrenInodeData.Contains((file.Inode.Id, hardlinkName)));
        }

        [Test]
        public void UnlinkFileTest()
        {
            var dirName = "Andrew loh";
            var dir = _fileSystem.CreateDirectory(dirName, _fileSystem.RootDirectoryPath);
            var fileName = "file";
            var file = _fileSystem.CreateFile(fileName, _fileSystem.RootDirectoryPath);
            var fileContent = file.GetContent();
            fileContent.Text = dirName;
            file.Content = fileContent.ToByteArray();
            _fileSystem.SaveFile(file);
            var root = _fileSystem.ReadDirectory(
                _fileSystem.InodesSection.Inodes[FileSystem.FileSystem.RootFolderInodeId]);
            var hardlinkName = "hardlink";
            var pathToCreatedLink = _pathResolver.Combine(_fileSystem.RootName, dirName, hardlinkName);
            _fileSystem.LinkFile(fileName, pathToCreatedLink);
            _fileSystem.DeleteFile(fileName);
            Assert.IsTrue(file.Inode.FileNames.Count == 1);
            Assert.IsTrue(file.Inode.LinksCount == 1);
            Assert.IsTrue(file.Inode.FileNames.Contains(hardlinkName));
            var hardlinkFile = _fileSystem.ReadFile(pathToCreatedLink);
            Assert.IsTrue(hardlinkFile.Inode == file.Inode);
        }

        [Test]
        public void SymlinkTest1()
        {
            var dir1Name = "dir1";
            var dir2Name = "dir2";
            var dirSymlinkName = "dir2Link";
            var fileName = "file1";
            var fileSymlinkName = "file1Link";
            var dir2Path = _pathResolver.Combine(_fileSystem.RootName, dir1Name);
            var dir1 = _fileSystem.CreateDirectory(dir1Name, _fileSystem.RootDirectoryPath);
            var dir2 = _fileSystem.CreateDirectory(dir2Name, dir2Path);
            var dir3 = _fileSystem.CreateSymlink(dirSymlinkName, _pathResolver.Combine(dir2Path, dir2Name));
            var file1 = _fileSystem.CreateFile(fileName, dir2Path);
            var file2 = _fileSystem.CreateSymlink(fileSymlinkName, _pathResolver.Combine(dir2Path, fileName));
            var file1Str = _fileSystem.GetSavableContentString(_pathResolver.Combine(dir2Path, fileName));
            var file2Str = _fileSystem.GetSavableContentString(fileSymlinkName);
            Assert.AreEqual(file1Str, file2Str);
            Assert.AreEqual(dir2.ToString(), _fileSystem.GetSavableContentString(dirSymlinkName));
        }

        [Test]
        public void SymlinkTest2()
        {
            var dir1Name = "dir1";
            var dir2Name = "dir2";
            var dirSymlink1Name = "dir3Link2";
            var dirSymlink2Name = "dir4Link3";
            var fileName = "file1";
            var fileSymlinkName = "file1Link";
            var dir2Path = _pathResolver.Combine(_fileSystem.RootName, dir1Name);
            var dir4Path = _pathResolver.Combine(dir2Path, dir2Name, dirSymlink2Name);
            var dir1 = _fileSystem.CreateDirectory(dir1Name, _fileSystem.RootDirectoryPath);
            var dir2 = _fileSystem.CreateDirectory(dir2Name, dir2Path);
            var dir3 = _fileSystem.CreateSymlink(dirSymlink1Name, _pathResolver.Combine(dir2Path, dir2Name));
            var dir4 = _fileSystem.CreateSymlink(dir4Path, dirSymlink1Name);
            var file1 = _fileSystem.CreateFile(fileName, dir2Path);
            var file2 = _fileSystem.CreateSymlink(fileSymlinkName, _pathResolver.Combine(dir2Path, fileName));
            _fileSystem.ChangeCurrentDirectory(dir4Path);
            Assert.IsTrue(_fileSystem.CurrentDirectory.Inode.Id == dir2.Inode.Id);
        }

        [Test]
        public void WritingBytesIntoSerializedStringTest()
        {
            var str = new RegularFile.RegularFileContent() {Text = "Andrew loh"};
            var byteArray = str.ToByteArray();
            var strBytes = byteArray.ToList();
            var toAdd = new byte[] {0, 34, 55, 66};
            var strLengthIndex = _serDes.RegularFileStringLengthIndex;
            strBytes[strLengthIndex] = (byte) (str.Text.Length + toAdd.Length);
            strBytes.InsertRange(strLengthIndex + 1, toAdd);
            Console.WriteLine(strBytes.ToArray().To<RegularFile.RegularFileContent>());
        }

        [Test]
        public void OpeningFileTest1()
        {
            var name = "Andrew loh";
            var file = _fileSystem.CreateFile(name, _fileSystem.RootDirectoryPath);
            var descriptor = "fd";
            _fileSystem.OpenFile(name, descriptor);
            var dataToWrite = new byte[] {54, 75, 65, 63, 88, 72};
            _fileSystem.SaveFile(descriptor, dataToWrite);
            _fileSystem.SeekFile(descriptor, 0);
            var result = _fileSystem.ReadFile(descriptor, dataToWrite.Length);
            _fileSystem.CloseFile(descriptor);
            Assert.IsTrue(result.ContentsMatchOrdered(dataToWrite));
        }

        [Test]
        public void OpeningFileTest2()
        {
            var name = "Andrew loh";
            var file = _fileSystem.CreateFile(name, _fileSystem.RootDirectoryPath);
            var descriptor = "fd";
            _fileSystem.OpenFile(name, descriptor);
            var result = _fileSystem.ReadFile(descriptor, 10);
            _fileSystem.CloseFile(descriptor);
            Assert.IsTrue(result.Length == 0);
        }

        [Test]
        public void TruncateFileTest()
        {
            var name = "Andrew loh";
            var file = _fileSystem.CreateFile(name, _fileSystem.RootDirectoryPath);
            var descriptor = "fd";
            _fileSystem.OpenFile(name, descriptor);
            var dataToWrite = new byte[] {54, 75, 65, 63, 88, 72};
            _fileSystem.SaveFile(descriptor, dataToWrite);
            _fileSystem.SeekFile(descriptor, 0);
            _fileSystem.CloseFile(descriptor);
            _fileSystem.TruncateFile(name, file.Inode.FileSize + 10);
            var fileBig = _fileSystem.ReadFile(name);
            _fileSystem.TruncateFile(name, file.Inode.FileSize - 13);
            var fileSmall = _fileSystem.ReadFile(name);
        }

        [Test]
        public void GetInodeDataTest()
        {
            var dir1Name = "dir1";
            var dir1 = _fileSystem.CreateDirectory(dir1Name, _fileSystem.RootDirectoryPath);
            var data = _fileSystem.GetInodeData(dir1Name);
            Console.WriteLine(data);
        }

        [Test]
        public void GetCWDDataTest()
        {
            var dir1Name = "dir1";
            var dir2Name = "dir2";
            var dirSymlink1Name = "dir3Link2";
            var dirSymlink2Name = "dir4Link3";
            var fileName = "file1";
            var fileSymlinkName = "file1Link";
            var dir2Path = _pathResolver.Combine(_fileSystem.RootName, dir1Name);
            var dir4Path = _pathResolver.Combine(dir2Path, dir2Name, dirSymlink2Name);
            var dir1 = _fileSystem.CreateDirectory(dir1Name, _fileSystem.RootDirectoryPath);
            var dir2 = _fileSystem.CreateDirectory(dir2Name, dir2Path);
            var dir3 = _fileSystem.CreateSymlink(dirSymlink1Name, _pathResolver.Combine(dir2Path, dir2Name));
            var dir4 = _fileSystem.CreateSymlink(dir4Path, dirSymlink1Name);
            var file1 = _fileSystem.CreateFile(fileName, dir2Path);
            var file2 = _fileSystem.CreateSymlink(fileSymlinkName, _pathResolver.Combine(dir2Path, fileName));
            var data1 = _fileSystem.GetCWDData();
            _fileSystem.ChangeCurrentDirectory(dir1Name);
            var data2 = _fileSystem.GetCWDData();
            _fileSystem.ChangeCurrentDirectory(dir2Name);
            var data3 = _fileSystem.GetCWDData();
            Console.WriteLine(data1);
            Console.WriteLine(data2);
            Console.WriteLine(data3);
        }
    }
}