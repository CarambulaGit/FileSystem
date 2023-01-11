using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileSystem.Exceptions;
using FileSystem.Savable;
using HardDrive;
using SerDes;
using Utils;
using Directory = FileSystem.Savable.Directory;

namespace FileSystem
{
    public class FileSystem : IFileSystem
    {
        #region Fields

        public const int RootFolderInodeId = 0;
        private readonly IHardDrive _hardDrive;
        private readonly IPathResolver _pathResolver;
        private readonly SectionSplitter _sectionSplitter;
        private int _inodeAmount;
        private int _dataBlocksAmount;
        private bool _initFromDrive;

        #endregion

        #region Properties

        public string RootDirectoryPath => RootName + Path.AltDirectorySeparatorChar;
        public string RootName => "";
        public Directory CurrentDirectory => CWDData.directory;
        public string CurrentDirectoryPath => CWDData.path;
        public Directory RootDirectory { get; private set; }
        private (Directory directory, string path) CWDData { get; set; }

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
            CWDData = (RootDirectory, RootDirectoryPath);
        }

        private void SplitSections() => _sectionSplitter.SplitSections(_inodeAmount, _dataBlocksAmount);

        private void InitRootDirectory()
        {
            RootDirectory = !_initFromDrive
                ? CreateRootDirectory()
                : ReadDirectory(InodesSection.Inodes[RootFolderInodeId]);
        }

        private Directory CreateRootDirectory()
        {
            var freeInode = InodesSection.Inodes[RootFolderInodeId];
            var directory = new Directory(freeInode, freeInode.Id);
            freeInode.LinksCount = directory.LinksCountDefault();
            freeInode.FileNames.Add(RootName);
            freeInode.FileType = FileType.Directory;
            InodesSection.SaveInode(freeInode);
            SaveDirectory(directory);
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

            var parentFolder = GetParentOfNewSavable(name, path, out var freeInode);
            parentFolder.Inode.LinksCount++;
            InodesSection.SaveInode(parentFolder.Inode);
            var newFolder = new Directory(freeInode, parentFolder.Inode.Id);
            freeInode.LinksCount = newFolder.LinksCountDefault();
            freeInode.FileNames.Add(name);
            freeInode.FileType = FileType.Directory;
            SaveDirectory(newFolder);
            FolderChildChangeCallback(parentFolder);
            return newFolder;
        }

        public Directory ReadDirectory(string path)
        {
            if (!TryGetDirectoryInodeByPath(path, out var inode, out var dir, out var reason))
            {
                throw new InvalidSavablePathException(reason);
            }

            return dir;
        }

        public Directory ReadDirectory(Inode inode)
        {
            var desiredType = FileType.Directory;
            if (inode.FileType != desiredType)
            {
                throw new IncorrectInodeTypeException(inode, desiredType);
            }

            var blocksContent = GetDataBlocksContent(inode);
            var directory = new Directory(inode)
            {
                Content = blocksContent
            };
            return directory;
        }

        public void SaveDirectory(Directory directory) => SaveSavable(directory);

        #region Delete

        public void DeleteDirectory(string path)
        {
            var absolutePath = _pathResolver.Resolve(path);
            var dir = ReadDirectory(absolutePath);
            DeleteDirectory(dir);
        }

        public void DeleteDirectory(Directory directory)
        {
            var dirInode = directory.Inode;
            if (dirInode.Id == RootDirectory.Inode.Id)
            {
                throw new CannotDeleteRootDirectory();
            }

            var dirContent = directory.GetContent();
            if (dirContent.ChildrenInodeIds.Count > Directory.DirectoryContent.DefaultNumOfChildren)
            {
                throw new DirectoryHasChildrenException();
            }

            DeleteFolderFromParent(directory, dirContent, dirInode);

            DeleteSavable(dirInode);
        }

        private void DeleteFolderFromParent(Directory directory, Directory.DirectoryContent dirContent, Inode dirInode)
        {
            var parentDirNodeId = directory.GetParentDirectoryInodeId(dirContent);
            var parentInode = InodesSection.Inodes[parentDirNodeId];
            parentInode.LinksCount--;
            InodesSection.SaveInode(parentInode);

            DeleteFromParent(dirInode, parentInode);
        }

        #endregion

        private bool FolderNameValid(string name) => true; // todo

        #endregion

        #region File

        public RegularFile CreateFile(string name, string path)
        {
            if (!FileNameValid(name))
            {
                throw new InvalidFileNameException();
            }

            var parentFolder = GetParentOfNewSavable(name, path, out var freeInode);
            var file = new RegularFile(freeInode);
            freeInode.LinksCount = file.LinksCountDefault();
            freeInode.FileNames.Add(name);
            freeInode.FileType = FileType.RegularFile;
            SaveFile(file);
            FolderChildChangeCallback(parentFolder);
            return file;
        }

        public RegularFile ReadFile(string path)
        {
            var inode = GetInodeByPath(path, out _);
            return ReadFile(inode);
        }

        public RegularFile ReadFile(Inode inode)
        {
            var desiredType = FileType.RegularFile;
            if (inode.FileType != desiredType)
            {
                throw new IncorrectInodeTypeException(inode, desiredType);
            }

            var blocksContent = GetDataBlocksContent(inode);
            var file = new RegularFile(inode, false)
            {
                Content = blocksContent
            };
            return file;
        }

        public void SaveFile(RegularFile file) => SaveSavable(file);

        public void DeleteFile(RegularFile file)
        {
            var fileInode = file.Inode;
            var parentsInodes = GetParentsByInode(fileInode);
            parentsInodes.ForEach(parentInode => DeleteFromParent(fileInode, parentInode));
            DeleteSavable(fileInode);
        }

        public void DeleteFile(string path)
        {
            var absolutePath = _pathResolver.Resolve(path);
            var inode = GetInodeByPath(absolutePath, out var parentInode);
            DeleteFromParent(inode, parentInode);
            var splitPath = _pathResolver.SplitPath(absolutePath);
            inode.FileNames.Remove(splitPath.savableName);
            inode.LinksCount--;
            if (!inode.IsOccupied)
            {
                DeleteSavable(inode);
            }
            else
            {
                InodesSection.SaveInode(inode);
            }
        }

        public void LinkFile(string pathToFile, string pathToCreatedLink) { }

        private bool FileNameValid(string name) => true; // todo

        #endregion

        #region Symlink

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
            if (BitmapSection.FreeBlocksAmount == 0)
            {
                throw new NotEnoughDataBlocksException();
            }

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
            indexes = BitmapSection.GetFreeBlocksIndexes(amount);
            for (var i = 0; i < result.Length; i++)
            {
                result[i] = DataBlocksSection.GetDataBlock(indexes[i]);
            }

            return result;
        }

        #endregion

        #region SavableConstructors

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

        #region Other

        public Inode GetInodeByPath(string path, out Inode parentInode)
        {
            var absolutePath = _pathResolver.Resolve(path);
            if (absolutePath == RootDirectoryPath)
            {
                parentInode = null;
                return RootDirectory.Inode;
            }

            var splitPath = _pathResolver.SplitPath(absolutePath);
            var dir = ReadDirectory(splitPath.pathToSavable);

            var namesWithInodes = GetNamesWithInodes(dir);
            if (!namesWithInodes.TryGetValue(splitPath.savableName, out var inode))
            {
                throw new CannotFindSavableException(splitPath.pathToSavable, splitPath.savableName);
            }

            parentInode = dir.Inode;
            return inode;
        }

        public void ChangeCurrentDirectory(string path)
        {
            var absolutePath = _pathResolver.Resolve(path);
            if (CurrentDirectoryPath == absolutePath) return;
            var inode = GetInodeByPath(absolutePath, out _);
            if (inode.FileType == FileType.Symlink)
            {
                throw new NotImplementedException();
                // todo
            }
            else if (inode.FileType != FileType.Directory)
            {
                var splitPath = _pathResolver.SplitPath(absolutePath);
                throw new CannotChangeCurrentDirectoryException(splitPath.pathToSavable, splitPath.savableName);
            }

            var directory = ReadDirectory(inode);
            CWDData = (directory, path);
        }

        private void DeleteSavable(Inode savableInode)
        {
            BitmapSection.Release(savableInode.OccupiedDataBlocks.Select(block => block.Address).ToArray());
            savableInode.Clear();
            InodesSection.SaveInode(savableInode);
        }

        private void SaveSavable(BaseSavable savable)
        {
            var occupiedBlocks = savable.Inode.OccupiedDataBlocks;
            var splitData = SplitForDataBlocks(savable.Content.ByteArrayToBinaryStr()).ToArray();
            if (occupiedBlocks.Length > splitData.Length)
            {
                savable.Inode.OccupiedDataBlocks = occupiedBlocks[..splitData.Length];
            }

            if (occupiedBlocks.Length < splitData.Length)
            {
                var additionalBlocksAmount = splitData.Length - occupiedBlocks.Length;
                _ = GetFreeDataBlocks(additionalBlocksAmount, out var indexes);
                BitmapSection.SetOccupied(indexes);
                var blockAddresses = indexes.Select(index => new BlockAddress(index));
                savable.Inode.OccupiedDataBlocks = occupiedBlocks.Concat(blockAddresses).ToArray();
            }

            InodesSection.SaveInode(savable.Inode);
            occupiedBlocks = savable.Inode.OccupiedDataBlocks;
            for (var i = 0; i < splitData.Length; i++)
            {
                DataBlocksSection.WriteBlock(occupiedBlocks[i].Address, splitData[i]);
            }
        }

        private Directory GetParentOfNewSavable(string name, string path, out Inode freeInode)
        {
            var absolutePath = _pathResolver.Resolve(path);
            if (!PathValid(absolutePath, name, out var parentFolderInode, out var reason))
            {
                throw new InvalidSavablePathException(reason);
            }

            freeInode = GetFreeInode();
            var parentFolder = ReadDirectory(parentFolderInode);
            var dirContent = parentFolder.GetContent();
            dirContent.ChildrenInodeIds.Add(freeInode.Id);
            parentFolder.Content = dirContent.ToByteArray();
            SaveDirectory(parentFolder);
            return parentFolder;
        }

        private void DeleteFromParent(Inode savableInode, Inode parentInode)
        {
            var parentDir = ReadDirectory(parentInode);
            var parentDirContent = parentDir.GetContent();
            parentDirContent.ChildrenInodeIds.Remove(savableInode.Id);
            parentDir.Content = parentDirContent.ToByteArray();
            SaveDirectory(parentDir);
            FolderChildChangeCallback(parentDir);
        }

        private bool PathValid(string path, string name, out Inode parentInode, out string reason)
        {
            if (!TryGetDirectoryInodeByPath(path, out parentInode, out var currentDirectory, out reason)) return false;

            var namesWithInodes = GetNamesWithInodes(currentDirectory);
            if (namesWithInodes.ContainsKey(name))
            {
                reason = $"There is already savable with name = {name}, at path = {path}";
                return false;
            }

            return true;
        }

        private bool TryGetDirectoryInodeByPath(string path, out Inode resultInode, out Directory dir,
            out string reason)
        {
            reason = string.Empty;
            resultInode = null;
            var pathParts = path.Split(Path.AltDirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
            dir = RootDirectory;
            for (var i = 0; i < pathParts.Length; i++)
            {
                var namesWithInodes = GetNamesWithInodes(dir);
                if (!namesWithInodes.TryGetValue(pathParts[i], out var inode) ||
                    (inode.FileType != FileType.Symlink && inode.FileType != FileType.Directory))
                {
                    reason =
                        $"Can't find directory with name = {pathParts[i]}, at path = {string.Join(Path.AltDirectorySeparatorChar, pathParts[..i])}";
                    return false;
                }

                if (inode.FileType == FileType.Symlink)
                {
                    throw new NotImplementedException();
                }

                dir = ReadDirectory(inode);
            }

            resultInode = dir.Inode;
            return true;
        }

        private Dictionary<string, Inode> GetNamesWithInodes(Directory directory)
        {
            var childrenInodes = GetChildrenInodes(directory);
            var namesWithInodes = childrenInodes
                .SelectMany(inodes => inodes.FileNames, (inode, fileName) => (fileName, inode))
                .ToDictionary(tuple => tuple.fileName, tuple => tuple.inode);
            return namesWithInodes;
        }

        private List<int> GetChildrenIds(Directory directory)
        {
            var rawChildrenIds = directory.GetContent().ChildrenInodeIds;
            var startIndex = Directory.DirectoryContent.DefaultNumOfChildren;
            List<int> childrenIds = null;
            if (rawChildrenIds.Count > startIndex)
            {
                childrenIds = rawChildrenIds.GetRangeByStartIndex(startIndex);
            }

            return childrenIds ?? new List<int>();
        }

        private List<Inode> GetChildrenInodes(Directory directory)
        {
            var childrenIds = GetChildrenIds(directory);
            var childrenInodes = childrenIds.Select(childId => InodesSection.Inodes[childId]).ToList();
            return childrenInodes;
        }

        private List<Inode> GetParentsByInode(Inode inode)
        {
            if (!inode.IsOccupied)
            {
                throw new EmptyInodeCannotHaveParentException();
            }

            if (inode.Id == RootDirectory.Inode.Id)
            {
                throw new RootDirectoryDoesNotHaveParentException();
            }

            var numOfParents = inode.FileNames.Count;
            var result = new List<Inode>(numOfParents);
            var directoriesQuery = new Queue<Directory>();
            directoriesQuery.Enqueue(RootDirectory);
            while (directoriesQuery.Count > 0 && result.Count < numOfParents)
            {
                var curDir = directoriesQuery.Dequeue();
                var children = GetChildrenInodes(curDir);
                foreach (var child in children)
                {
                    if (child.Id == inode.Id)
                    {
                        result.Add(child);
                    }

                    if (child.FileType == FileType.Directory)
                    {
                        directoriesQuery.Enqueue(ReadDirectory(child));
                    }
                }
            }

            throw new CannotFindParentByInodeException(inode);
        }

        private byte[] GetDataBlocksContent(Inode inode)
        {
            var indexes = inode.OccupiedDataBlocks
                .Select(addressContainer => addressContainer.Address).ToArray();

            var blocksContents = new char[indexes.Length][];
            for (var i = 0; i < indexes.Length; i++)
            {
                blocksContents[i] = DataBlocksSection.ReadBlock(indexes[i]);
            }

            var blocksContent = blocksContents.SelectMany(bytes => bytes).ToArray().BinaryCharsArrayToByteArray();
            return blocksContent;
        }

        private string[] SplitForDataBlocks(string data)
        {
            var dataBlockLength = DataBlock.BlockLength;
            var result = new string[(int) Math.Ceiling((float) data.Length / dataBlockLength)];
            for (int i = 0, b = 0; i < data.Length; i += dataBlockLength, b++)
            {
                result[b] = i + dataBlockLength > data.Length
                    ? data[i..]
                    : data[i..(i + dataBlockLength)];
            }

            return result;
        }

        private void FolderChildChangeCallback(Directory folder)
        {
            if (folder.Inode.Id == RootFolderInodeId)
            {
                RootDirectory = folder;
            }
            else if (folder.Inode.Id == CurrentDirectory.Inode.Id)
            {
                CWDData = (folder, CurrentDirectoryPath);
            }
        }

        #endregion
    }
}