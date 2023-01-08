using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileSystem.Exceptions;
using FileSystem.Savable;
using HardDrive;
using SerDes;
using Directory = FileSystem.Savable.Directory;

namespace FileSystem
{
    public class FileSystem : IFileSystem
    {
        #region Fields

        private const int RootFolderInodeId = 0;
        private readonly IHardDrive _hardDrive;
        private readonly IPathResolver _pathResolver;
        private readonly SectionSplitter _sectionSplitter;
        private int _inodeAmount;
        private int _dataBlocksAmount;
        private bool _initFromDrive;

        #endregion

        #region Properties

        public string RootDirectoryPath => RootPath + Path.AltDirectorySeparatorChar;
        public string RootPath => "";
        public string RootName => "";
        public Directory CurrentDirectory { get; private set; }
        public Directory RootDirectory { get; private set; }

        public BitmapSection BitmapSection => _sectionSplitter.BitmapSection;
        public InodesSection InodesSection => _sectionSplitter.InodesSection;
        public DataBlocksSection DataBlocksSection => _sectionSplitter.DataBlocksSection;

        #endregion

        #region Initialize

        public FileSystem(IHardDrive hardDrive, IPathResolver pathResolver, int inodeAmount, int dataBlocksAmount,
            bool initFromDrive = false)
        {
            _pathResolver = pathResolver;
            _hardDrive = hardDrive;
            _inodeAmount = inodeAmount;
            _dataBlocksAmount = dataBlocksAmount;
            _initFromDrive = initFromDrive;
            _sectionSplitter = new SectionSplitter(_hardDrive, _initFromDrive);
        }

        public void Initialize()
        {
            SplitSections();
            InitRootDirectory();
            CurrentDirectory = RootDirectory;
        }

        private void SplitSections() => _sectionSplitter.SplitSections(_inodeAmount, _dataBlocksAmount);

        private void InitRootDirectory()
        {
            RootDirectory = !_initFromDrive
                ? CreateRootDirectory()
                : ConstructDirectoryFromInode(InodesSection.Inodes[RootFolderInodeId]);
        }

        private Directory CreateRootDirectory()
        {
            var freeInode = InodesSection.Inodes[RootFolderInodeId];
            var directory = new Directory(freeInode, freeInode.Id);
            var splitData = SplitForDataBlocks(directory.Content).ToArray();
            _ = GetFreeDataBlocks(splitData.Length, out var indexes);
            var blockAddresses = indexes.Select(index => new BlockAddress(index)).ToArray();
            freeInode.OccupiedDataBlocks = blockAddresses;
            freeInode.LinksCount = directory.LinksCountDefault();
            freeInode.FileNames.Add(RootName);
            InodesSection.SaveInode(freeInode);

            for (var i = 0; i < splitData.Length; i++)
            {
                DataBlocksSection.WriteBlock(blockAddresses[i].Address, splitData[i]);
            }

            return directory;
        }

        #endregion

        #region Directory

        public Directory CreateDirectory(string name, string path)
        {
            if (!FolderNameValid(name))
            {
                throw new InvalidDirectoryNameException();
            }

            var absolutePath = _pathResolver.Resolve(path);
            if (!PathValid(absolutePath, name, out var parentFolderInode, out var reason))
            {
                throw new InvalidDirectoryPathException(reason);
            }

            var freeInode = GetFreeInode();
            var parentFolder = new Directory(parentFolderInode);
            parentFolder.Content = parentFolder.GetContent().ChildrenInodeIds.Append(freeInode.Id).ToByteArray();
            SaveDirectory(parentFolder);
            var newFolder = new Directory(freeInode, parentFolderInode.Id);
            freeInode.LinksCount = newFolder.LinksCountDefault();
            freeInode.FileNames.Add(name);
            return newFolder;
        }

        public void ReadDirectory()
        {
            // todo
        }

        public void SaveDirectory(Directory directory)
        {
            var occupiedBlocks = directory.Inode.OccupiedDataBlocks;
            var splitData = SplitForDataBlocks(directory.Content).ToArray();
            if (occupiedBlocks.Length > splitData.Length)
            {
                directory.Inode.OccupiedDataBlocks = occupiedBlocks[..splitData.Length];
                InodesSection.SaveInode(directory.Inode);
                occupiedBlocks = directory.Inode.OccupiedDataBlocks;
            }

            if (occupiedBlocks.Length < splitData.Length)
            {
                var additionalBlocksAmount = splitData.Length - occupiedBlocks.Length;
                _ = GetFreeDataBlocks(additionalBlocksAmount, out var indexes);
                var blockAddresses = indexes.Select(index => new BlockAddress(index));
                directory.Inode.OccupiedDataBlocks = occupiedBlocks.Concat(blockAddresses).ToArray();
                InodesSection.SaveInode(directory.Inode);
                occupiedBlocks = directory.Inode.OccupiedDataBlocks;
            }

            for (var i = 0; i < splitData.Length; i++)
            {
                DataBlocksSection.WriteBlock(occupiedBlocks[i].Address, splitData[i]);
            }
        }

        public void DeleteDirectory()
        {
            // todo
        }

        private bool FolderNameValid(string name) => true; // todo

        #endregion

        #region File

        public RegularFile CreateFile(string name)
        {
            throw new NotImplementedException();
            // todo
        }

        public void ReadFile()
        {
            // todo
        }

        public void SaveFile()
        {
            // todo
        }

        public void DeleteFile()
        {
            // todo
        }

        private bool FileNameValid(string name) => true; // todo

        #endregion

        #region GetFree

        private Inode GetFreeInode()
        {
            if (InodesSection.FreeInodesAmount() <= 0)
            {
                throw new NotEnoughInodesException();
            }

            return InodesSection.GetFreeInode();
        }

        private DataBlock GetFreeDataBlock(out int index)
        {
            index = BitmapSection.GetFreeBlockIndex();
            return DataBlocksSection.GetDataBlock(index);
        }

        private DataBlock[] GetFreeDataBlocks(int amount, out int[] indexes)
        {
            if (amount > BitmapSection.FreeBlocksAmount)
            {
                throw new NotEnoughDataBlocksException();
            }

            var result = new DataBlock[amount];
            indexes = new int[amount];
            for (var i = 0; i < result.Length; i++)
            {
                result[i] = GetFreeDataBlock(out indexes[i]);
            }

            return result;
        }

        #endregion

        #region SavableConstructors

        private Directory ConstructDirectoryFromInode(Inode inode)
        {
            var desiredType = FileType.Directory;
            if (inode.FileType != desiredType)
            {
                throw new IncorrectInodeTypeException(inode, desiredType);
            }

            return new Directory(inode);
        }

        private RegularFile ConstructRegularFileFromInode(Inode inode)
        {
            var desiredType = FileType.RegularFile;
            if (inode.FileType != desiredType)
            {
                throw new IncorrectInodeTypeException(inode, desiredType);
            }

            return new RegularFile(inode);
        }

        private Symlink ConstructSymlinkFromInode(Inode inode)
        {
            var desiredType = FileType.Symlink;
            if (inode.FileType != desiredType)
            {
                throw new IncorrectInodeTypeException(inode, desiredType);
            }

            return new Symlink(inode);
        }

        #endregion

        private bool PathValid(string path, string name, out Inode parentInode, out string reason)
        {
            reason = string.Empty;
            parentInode = null;
            var pathParts = path.Split(Path.AltDirectorySeparatorChar);
            var currentDirectory = RootDirectory;
            Dictionary<string, Inode> namesWithInodes;
            for (int i = 0; i < pathParts.Length; i++)
            {
                namesWithInodes = GetNamesWithInodes(currentDirectory);
                if (!namesWithInodes.TryGetValue(pathParts[i], out var inode))
                {
                    reason =
                        $"Can't find folder with name = {pathParts[i]}, at path = {string.Join(Path.AltDirectorySeparatorChar, pathParts[..i])}";
                    return false;
                }

                currentDirectory = new Directory(inode);
            }

            namesWithInodes = GetNamesWithInodes(currentDirectory);
            if (namesWithInodes.ContainsKey(name))
            {
                reason = $"There is already savable with name = {name}, at path = {path}";
                return false;
            }

            parentInode = currentDirectory.Inode;
            return true;
        }

        private Dictionary<string, Inode> GetNamesWithInodes(Directory directory)
        {
            var childrenIds = directory.GetContent().ChildrenInodeIds;
            var childrenInodes = childrenIds.Select(childId => InodesSection.Inodes[childId]).ToList();
            var namesWithInodes =
                childrenInodes.SelectMany(inodes => inodes.FileNames, (inode, fileName) => (fileName, inode))
                    .ToDictionary(tuple => tuple.fileName, tuple => tuple.inode);
            return namesWithInodes;
        }

        private IEnumerable<byte[]> SplitForDataBlocks(byte[] data)
        {
            var dataBlockLength = DataBlock.BlockLength;
            var result = new byte[(int) Math.Ceiling((float) data.Length / dataBlockLength)][];
            for (int i = 0, b = 0; i < data.Length; i += dataBlockLength, b++)
            {
                result[b] = i + dataBlockLength > data.Length
                    ? data[i..]
                    : data[i..(i + dataBlockLength)];
            }

            return result;
        }
    }
}