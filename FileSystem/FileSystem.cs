using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FileSystem.Exceptions;
using FileSystem.Savable;
using HardDrive;
using Microsoft.Extensions.Primitives;
using SerDes;
using Utils;
using Directory = FileSystem.Savable.Directory;

namespace FileSystem
{
    public class FileSystem : IFileSystem
    {
        #region Fields

        public const int RootFolderInodeId = 0;
        public const int MaxNumOfOpenedFiles = 5;
        private readonly IHardDrive _hardDrive;
        private readonly IPathResolver _pathResolver;
        private readonly SectionSplitter _sectionSplitter;
        private readonly Dictionary<string, (int offset, RegularFile file)> _openedFiles = new();
        private int _inodeAmount;
        private int _dataBlocksAmount;
        private bool _initFromDrive;
        private ISerDes _serDes;

        #endregion

        #region Properties

        public string RootDirectoryPath => RootName + _pathResolver.Separator;
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

        public FileSystem(ISerDes serDes, IHardDrive hardDrive, IPathResolver pathResolver, int inodeAmount,
            int dataBlocksAmount,
            bool initFromDrive = false)
        {
            _pathResolver = pathResolver;
            _serDes = serDes;
            _hardDrive = hardDrive;
            _inodeAmount = inodeAmount;
            _dataBlocksAmount = dataBlocksAmount;
            _initFromDrive = initFromDrive;
            _sectionSplitter = new SectionSplitter(_serDes, _hardDrive, _initFromDrive);
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
            freeInode.FileNames.Add(RootName);
            freeInode.FileType = FileType.Directory;
            var directory = new Directory(freeInode, freeInode.Id, RootName);
            freeInode.LinksCount = directory.LinksCountDefault();
            InodesSection.SaveInode(freeInode);
            SaveDirectory(directory);
            return directory;
        }

        #endregion

        #region Directory

        #region Directory/Create

        public Directory CreateDirectory(string path)
        {
            var absolutePath = _pathResolver.Resolve(path);
            var splitPath = _pathResolver.SplitPath(absolutePath);
            return CreateDirectory(splitPath.savableName, splitPath.pathToSavable);
        }

        public Directory CreateDirectory(string name, string path)
        {
            if (!FolderNameValid(name))
            {
                throw new InvalidDirectoryNameException();
            }

            var parentFolder = GetParentOfNewSavable(name, path);
            var freeInode = GetFreeInode();
            freeInode.FileNames.Add(name);
            freeInode.FileType = FileType.Directory;
            var newFolder = new Directory(freeInode, parentFolder.Inode.Id, parentFolder.Inode.FileNames[0]);
            freeInode.LinksCount = newFolder.LinksCountDefault();
            SaveDirectory(newFolder);
            AddChildToDirectory(freeInode, name, parentFolder);
            FolderChildChangeCallback(parentFolder);
            return newFolder;
        }

        #endregion

        #region Directory/Read

        public Directory ReadDirectory(string path)
        {
            if (!TryGetDirectoryInodeByPath(path, out var dir, out var reason))
            {
                throw new InvalidSavablePathException(reason);
            }

            return dir;
        }

        public Directory ReadDirectory(Inode inode)
        {
            CheckForType(inode, FileType.Directory);
            var blocksContent = GetDataBlocksContent(inode);
            var directory = new Directory(inode)
            {
                Content = blocksContent[..inode.FileSize]
            };
            return directory;
        }

        #endregion

        #region Directory/Save

        public void SaveDirectory(Directory directory, Directory.DirectoryContent content)
        {
            directory.Content = content.ToByteArray();
            SaveDirectory(directory);
        }

        public void SaveDirectory(Directory directory) => SaveSavable(directory);

        #endregion

        #region Directory/Delete

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
            if (dirContent.ChildrenInodeData.Count > Directory.DirectoryContent.DefaultNumOfChildren)
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

            DeleteFromParent(dirInode, dirInode.FileNames[0], parentInode);
        }

        #endregion

        private bool FolderNameValid(string name) => true; // todo

        private void AddChildToDirectory(Inode childInode, string name, Directory directory)
        {
            var dirContent = directory.GetContent();
            dirContent.ChildrenInodeData.Add((childInode.Id, name));
            directory.Content = dirContent.ToByteArray();
            SaveDirectory(directory);
            if (childInode.FileType != FileType.Directory) return;
            directory.Inode.LinksCount++;
            InodesSection.SaveInode(directory.Inode);
        }

        #endregion

        #region File

        #region File/Create

        public RegularFile CreateFile(string path)
        {
            var absolutePath = _pathResolver.Resolve(path);
            var splitPath = _pathResolver.SplitPath(absolutePath);
            return CreateFile(splitPath.savableName, splitPath.pathToSavable);
        }

        public RegularFile CreateFile(string name, string path)
        {
            if (!FileNameValid(name))
            {
                throw new InvalidFileNameException();
            }

            var parentFolder = GetParentOfNewSavable(name, path);
            var freeInode = GetFreeInode();
            var file = new RegularFile(freeInode);
            freeInode.LinksCount = file.LinksCountDefault();
            freeInode.FileNames.Add(name);
            freeInode.FileType = FileType.RegularFile;
            SaveFile(file);
            AddChildToDirectory(freeInode, name, parentFolder);
            FolderChildChangeCallback(parentFolder);
            return file;
        }

        #endregion

        #region File/Read

        public RegularFile ReadFile(string path)
        {
            var absolutePath = _pathResolver.Resolve(path);
            var inode = GetInodeByPath(absolutePath, out _);
            if (inode.FileType == FileType.Symlink)
            {
                var symlink = ReadSymlink(inode);
                return ReadFile(symlink.GetContent().Address);
            }

            return ReadFile(inode);
        }

        public RegularFile ReadFile(Inode inode)
        {
            CheckForType(inode, FileType.RegularFile);
            var blocksContent = GetDataBlocksContent(inode);
            var file = new RegularFile(inode, false)
            {
                Content = blocksContent[..inode.FileSize]
            };
            return file;
        }

        public byte[] ReadFile(string descriptor, int numOfBytesToRead)
        {
            // todo test (empty file)
            CheckIfDescriptorInitialized(descriptor);
            var openedFile = _openedFiles[descriptor];
            var file = openedFile.file;
            var offset = GetRealOffset(openedFile.offset);
            var readTo = Math.Min(offset + numOfBytesToRead, file.Inode.FileSize - 1);
            _openedFiles[descriptor] = (readTo, file);
            return file.Content[offset..readTo];
        }

        #endregion

        #region File/Save

        public void SaveFile(RegularFile file, RegularFile.RegularFileContent content)
        {
            file.Content = content.ToByteArray();
            SaveFile(file);
        }

        public void SaveFile(RegularFile file) => SaveSavable(file);

        public void SaveFile(string descriptor, byte[] dataToWrite)
        {
            CheckIfDescriptorInitialized(descriptor);
            var openedFile = _openedFiles[descriptor];
            SaveFile(openedFile.file, dataToWrite, openedFile.offset);
        }

        private void SaveFile(RegularFile file, byte[] dataToWrite, int offset = 0)
        {
            var content = file.Content.ToList();
            var strLengthIndex = _serDes.RegularFileStringLengthIndex;
            content[strLengthIndex] = (byte) (content[strLengthIndex] + dataToWrite.Length);
            content.InsertRange(GetRealOffset(offset), dataToWrite);
            file.Content = content.ToArray();
            SaveFile(file);
        }

        #endregion

        #region File/Delete

        public void DeleteFile(string path)
        {
            var file = ReadFile(path);
            CheckIfFileCanBeDeleted(file);
            DeleteSavable(path);
        }

        #endregion

        #region File/Link

        public void LinkFile(string pathToFile, string pathToCreatedLink)
        {
            var file = ReadFile(pathToFile);
            var absolutePathToCreatedLink = _pathResolver.Resolve(pathToCreatedLink);
            var splitPathToLink = _pathResolver.SplitPath(absolutePathToCreatedLink);
            CheckPath(splitPathToLink.savableName, splitPathToLink.pathToSavable, out var parentFolderInode);
            var linkParentDir = ReadDirectory(parentFolderInode);
            var fileInode = file.Inode;
            AddChildToDirectory(fileInode, splitPathToLink.savableName, linkParentDir);
            fileInode.FileNames.Add(splitPathToLink.savableName);
            fileInode.LinksCount++;
            InodesSection.SaveInode(fileInode);
        }

        #endregion

        #region File/Open

        public void OpenFile(string path, string descriptor)
        {
            CheckBeforeOpenFile(descriptor);
            var absolutePath = _pathResolver.Resolve(path);
            var inode = GetInodeByPath(absolutePath, out _);
            CheckForType(inode, FileType.RegularFile);
            var file = ReadFile(inode);
            UnsafeOpenFile(file, descriptor);
        }

        public void OpenFile(RegularFile file, string descriptor)
        {
            CheckBeforeOpenFile(descriptor);
            UnsafeOpenFile(file, descriptor);
        }

        private void UnsafeOpenFile(RegularFile file, string descriptor) => _openedFiles.Add(descriptor, (0, file));

        private void CheckBeforeOpenFile(string descriptor)
        {
            if (_openedFiles.Count >= MaxNumOfOpenedFiles)
            {
                throw new MaxNumOfOpenedFilesReachedException();
            }

            if (_openedFiles.ContainsKey(descriptor))
            {
                throw new DescriptorAlreadyBusyException();
            }
        }

        #endregion

        #region File/Close

        public void CloseFile(string descriptor)
        {
            CheckIfDescriptorInitialized(descriptor);
            _openedFiles.Remove(descriptor);
        }

        #endregion

        #region File/Seek

        public void SeekFile(string descriptor, int offsetInBytes)
        {
            CheckIfDescriptorInitialized(descriptor);
            var openedFile = _openedFiles[descriptor];
            _openedFiles[descriptor] = (offsetInBytes, openedFile.file);
        }

        #endregion

        #region File/Truncate

        public void TruncateFile(string path, int size)
        {
            var file = ReadFile(path);
            var delta = file.Inode.FileSize - size;
            if (delta > 0)
            {
                var endIndex = file.Inode.FileSize - 1;
                var minSize = file.Inode.FileSize - file.Content[_serDes.RegularFileStringLengthIndex];
                if (size < minSize)
                    throw new CannotTruncateFileToGivenSizeException(size, minSize);

                var content = file.Content.ToList();
                var strLengthIndex = _serDes.RegularFileStringLengthIndex;
                content[strLengthIndex] = (byte) (content[strLengthIndex] - delta);
                content.RemoveRange(endIndex - delta, delta);
                file.Content = content.ToArray();
                SaveFile(file);
            }
            else
            {
                var dataToWrite = new byte[-delta].FillWith(() => (byte) '0');
                SaveFile(file, dataToWrite, file.Content[_serDes.RegularFileStringLengthIndex]);
            }
        }

        #endregion

        private int GetRealOffset(int rawOffset) => _serDes.RegularFileStringLengthIndex + rawOffset + 1;

        private void CheckIfFileCanBeDeleted(RegularFile file)
        {
            var fileOpened = _openedFiles.Values
                .Select(openedFile => openedFile.file)
                .FirstOrDefault(openedFile => openedFile.Inode.Id == file.Inode.Id) != null;
            if (fileOpened)
            {
                throw new CannotDeleteOpenedFileException(file);
            }
        }

        private void CheckIfDescriptorInitialized(string descriptor)
        {
            if (!_openedFiles.ContainsKey(descriptor))
            {
                throw new NotInitializedDescriptorException(descriptor);
            }
        }

        private bool FileNameValid(string name) => true; // todo

        #endregion

        #region Symlink

        #region Symlink/Create

        public Symlink CreateSymlink(string pathForLink, string pathToSavable)
        {
            var absolutePath = _pathResolver.Resolve(pathForLink);
            var splitPath = _pathResolver.SplitPath(absolutePath);
            return CreateSymlink(splitPath.savableName, splitPath.pathToSavable, _pathResolver.Resolve(pathToSavable));
        }

        public Symlink CreateSymlink(string name, string path, string pathToSavable)
        {
            if (!SymlinkNameValid(name))
            {
                throw new InvalidSymlinkNameException();
            }

            var parentFolder = GetParentOfNewSavable(name, path);
            var freeInode = GetFreeInode();
            var symlink = new Symlink(freeInode);
            freeInode.LinksCount = symlink.LinksCountDefault();
            freeInode.FileNames.Add(name);
            freeInode.FileType = FileType.Symlink;
            SaveSymlink(symlink, pathToSavable);
            AddChildToDirectory(freeInode, name, parentFolder);
            FolderChildChangeCallback(parentFolder);
            return symlink;
        }

        #endregion

        #region Symlink/Read

        public Symlink ReadSymlink(string path)
        {
            var inode = GetInodeByPath(path, out _);
            return ReadSymlink(inode);
        }

        public Symlink ReadSymlink(Inode inode)
        {
            CheckForType(inode, FileType.Symlink);
            var blocksContent = GetDataBlocksContent(inode);
            var symlink = new Symlink(inode, false)
            {
                Content = blocksContent[..inode.FileSize]
            };
            return symlink;
        }

        #endregion

        #region Symlink/Save

        public void SaveSymlink(Symlink symlink) => SaveSavable(symlink);

        public void SaveSymlink(Symlink symlink, string pathToLink)
        {
            symlink.Content = new Symlink.SymlinkContent {Address = pathToLink}.ToByteArray();
            SaveSymlink(symlink);
        }

        #endregion

        #region Symlink/Delete

        public void DeleteSymlink(string path) => DeleteSavable(path);

        #endregion

        private bool SymlinkNameValid(string name) => true;

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
            void ThrowCannotResolveSymlinkAddress(Symlink.SymlinkContent symlinkContent) =>
                throw new CannotResolveAddressFromSymlinkException(symlinkContent.Address);

            var absolutePath = _pathResolver.Resolve(path);
            if (CurrentDirectoryPath == absolutePath) return;
            var inode = GetInodeByPath(absolutePath, out _);
            if (inode.FileType == FileType.Symlink)
            {
                var symlink = ReadSymlink(inode);
                var symlinkContent = symlink.GetContent();
                try
                {
                    ChangeCurrentDirectory(symlinkContent.Address);
                    return;
                }
                catch (InvalidSavablePathException e)
                {
                    ThrowCannotResolveSymlinkAddress(symlinkContent);
                }
                catch (CannotFindSavableException e)
                {
                    ThrowCannotResolveSymlinkAddress(symlinkContent);
                }
            }
            else if (inode.FileType != FileType.Directory)
            {
                var splitPath = _pathResolver.SplitPath(absolutePath);
                throw new CannotChangeCurrentDirectoryException(splitPath.pathToSavable, splitPath.savableName);
            }

            var directory = ReadDirectory(inode);
            CWDData = (directory, absolutePath);
        }

        public string GetSavableContentString(string path)
        {
            var inode = GetInodeByPath(path, out _);
            switch (inode.FileType)
            {
                case FileType.Directory:
                    var dir = ReadDirectory(inode);
                    return dir.ToString();
                case FileType.RegularFile:
                    var file = ReadFile(inode);
                    return file.ToString();
                case FileType.Symlink:
                    var symlink = ReadSymlink(inode);
                    return GetSavableContentString(symlink.GetContent().Address);
                case FileType.None:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public string GetInodeData(string path)
        {
            var inode = GetInodeByPath(path, out _);
            return inode.ToString();
        }

        public string GetCWDData()
        {
            var cwdContent = CurrentDirectory.GetContent();
            var sb = new StringBuilder();
            sb.Append($"Directory {CurrentDirectory.Inode.FileNames[0]} data:\n");
            for (var i = 0; i < cwdContent.ChildrenInodeData.Count; i++)
            {
                var savableData = cwdContent.ChildrenInodeData[i];
                var inode = InodesSection.Inodes[savableData.id];
                var name = i switch
                {
                    0 => _pathResolver.ParentDirectory,
                    1 => _pathResolver.CurrentDirectory,
                    _ => savableData.name
                };
                sb.Append($"\t{inode.ToShortStr(name)}\n");
            }

            return sb.ToString();
        }

        private void CheckForType(Inode inode, FileType desiredType)
        {
            if (inode.FileType != desiredType)
            {
                throw new IncorrectInodeTypeException(inode, desiredType);
            }
        }

        /// <summary>
        /// Delete single ref 
        /// </summary>
        /// <param name="path">Path to regular file or symlink</param>
        private void DeleteSavable(string path)
        {
            var absolutePath = _pathResolver.Resolve(path);
            var splitPath = _pathResolver.SplitPath(absolutePath);
            var parentDir = ReadDirectory(splitPath.pathToSavable);
            var namesWithInodes = GetNamesWithInodes(parentDir);
            if (!namesWithInodes.TryGetValue(splitPath.savableName, out var inode))
            {
                throw new CannotFindSavableException(splitPath.pathToSavable, splitPath.savableName);
            }

            DeleteFromParent(inode, splitPath.savableName, parentDir.Inode);
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

            savable.Inode.FileSize = savable.Content.Length;
            InodesSection.SaveInode(savable.Inode);
            occupiedBlocks = savable.Inode.OccupiedDataBlocks;
            for (var i = 0; i < splitData.Length; i++)
            {
                DataBlocksSection.WriteBlock(occupiedBlocks[i].Address, splitData[i]);
            }
        }

        private Directory GetParentOfNewSavable(string name, string path)
        {
            var absolutePath = _pathResolver.Resolve(path);
            CheckPath(name, absolutePath, out var parentFolderInode);
            var parentFolder = ReadDirectory(parentFolderInode);
            return parentFolder;
        }

        private void CheckPath(string name, string path, out Inode parentFolderInode)
        {
            if (!PathValid(path, name, out parentFolderInode, out var reason))
            {
                throw new InvalidSavablePathException(reason);
            }
        }

        private void DeleteFromParent(Inode savableInode, string name, Inode parentInode)
        {
            var parentDir = ReadDirectory(parentInode);
            var parentDirContent = parentDir.GetContent();
            parentDirContent.ChildrenInodeData.Remove((savableInode.Id, name));
            parentDir.Content = parentDirContent.ToByteArray();
            SaveDirectory(parentDir);
            FolderChildChangeCallback(parentDir);
        }

        private bool PathValid(string path, string name, out Inode inode, out string reason)
        {
            inode = null;
            var absolutePath = _pathResolver.Resolve(path);
            if (!TryGetDirectoryInodeByPath(absolutePath, out var dir, out reason)) return false;
            inode = dir.Inode;
            var namesWithInodes = GetNamesWithInodes(dir);
            if (namesWithInodes.ContainsKey(name))
            {
                reason = $"There is already savable with name = {name}, at path = {absolutePath}";
                return false;
            }

            return true;
        }

        private bool TryGetDirectoryInodeByPath(string path, out Directory dir, out string reason)
        {
            reason = string.Empty;
            var pathParts = path.Split(_pathResolver.Separator, StringSplitOptions.RemoveEmptyEntries);
            dir = RootDirectory;
            for (var i = 0; i < pathParts.Length; i++)
            {
                var namesWithInodes = GetNamesWithInodes(dir);
                if (!namesWithInodes.TryGetValue(pathParts[i], out var savableData) ||
                    (savableData.FileType != FileType.Symlink && savableData.FileType != FileType.Directory))
                {
                    reason =
                        $"Can't find directory with name = {pathParts[i]}, at path = {_pathResolver.Combine(pathParts[..i])}";
                    return false;
                }

                if (savableData.FileType == FileType.Symlink)
                {
                    var symlink = ReadSymlink(savableData);
                    var symlinkContent = symlink.GetContent();
                    if (!TryGetDirectoryInodeByPath(symlinkContent.Address, out dir, out reason))
                    {
                        reason += $"\nCan't resolve address from symlink\nAddress = {symlinkContent.Address}";
                        return false;
                    }
                }
                else
                {
                    dir = ReadDirectory(savableData);
                }
            }

            return true;
        }

        private Dictionary<string, Inode> GetNamesWithInodes(Directory directory)
        {
            var childrenInodes = GetChildrenInodesData(directory);
            var namesWithInodes = childrenInodes.ToDictionary(tuple => tuple.name, tuple => tuple.inode);
            return namesWithInodes;
        }

        private List<(int id, string name)> GetChildrenData(Directory directory)
        {
            var rawChildrenIds = directory.GetContent().ChildrenInodeData;
            var startIndex = Directory.DirectoryContent.DefaultNumOfChildren;
            List<(int, string)> childrenIds = null;
            if (rawChildrenIds.Count > startIndex)
            {
                childrenIds = rawChildrenIds.GetRangeByStartIndex(startIndex);
            }

            return childrenIds ?? new List<(int, string)>();
        }

        private List<(Inode inode, string name)> GetChildrenInodesData(Directory directory)
        {
            var childrenData = GetChildrenData(directory);
            var childrenInodes = childrenData
                .Select(childData => (InodesSection.Inodes[childData.id], childData.name)).ToList();
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
                var childrenData = GetChildrenInodesData(curDir);
                foreach (var childData in childrenData)
                {
                    if (childData.inode.Id == inode.Id)
                    {
                        result.Add(curDir.Inode);
                    }

                    if (childData.inode.FileType == FileType.Directory)
                    {
                        directoriesQuery.Enqueue(ReadDirectory(childData.inode));
                    }
                }
            }

            return result.Count > 0 ? result : throw new CannotFindParentByInodeException(inode);
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

            if (folder.Inode.Id == CurrentDirectory.Inode.Id)
            {
                CWDData = (folder, CurrentDirectoryPath);
            }
        }

        #endregion
    }
}