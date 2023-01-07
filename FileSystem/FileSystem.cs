using System;
using FileSystem.Savable;
using HardDrive;

namespace FileSystem
{
    public class FileSystem : IFileSystem
    {
        public const string RootDirectoryPath = "/";
        private IHardDrive _hardDrive;
        private SectionSplitter _sectionSplitter;
        private int _inodeAmount;
        private int _dataBlocksAmount;
        private bool _initFromDrive;
        private IPathResolver _pathResolver;

        public Directory CurrentDirectory { get; private set; }
        public Directory RootDirectory { get; private set; }

        public BitmapSection BitmapSection => _sectionSplitter.BitmapSection;
        public InodesSection InodesSection => _sectionSplitter.InodesSection;
        public DataBlocksSection DataBlocksSection => _sectionSplitter.DataBlocksSection;

        public FileSystem(IHardDrive hardDrive, IPathResolver pathResolver, int inodeAmount, int dataBlocksAmount, bool initFromDrive = false)
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
            if (!_initFromDrive) InitRootDirectory();
            CurrentDirectory = RootDirectory;
        }

        #region Directory

        public Directory CreateDirectory(string name, string path)
        {
            throw new NotImplementedException();
            // todo
        }

        public void ReadDirectory()
        {
            // todo
        }

        public void SaveDirectory()
        {
            // todo
        }

        public void DeleteDirectory()
        {
            // todo
        }

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

        #endregion
        
        private void SplitSections() => _sectionSplitter.SplitSections(_inodeAmount, _dataBlocksAmount);

        private void InitRootDirectory()
        {
            CreateDirectory("", _pathResolver.Resolve(CurrentDirectory));
        }

        private Inode GetFreeNode()
        {
            throw new NotImplementedException();
            // todo
        }

        private DataBlock GetFreeDataBlock()
        {
            throw new NotImplementedException();
            // todo
        }
    }

    public interface IFileSystem { }
}