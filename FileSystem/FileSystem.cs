using FileSystem.Savable;
using HardDrive;

namespace FileSystem
{
    public class FileSystem : IFileSystem
    {
        private IHardDrive _hardDrive;
        private SectionSplitter _sectionSplitter;
        private int _inodeAmount;
        private int _dataBlocksAmount;

        public Directory CurrentDirectory { get; private set; }

        private BitmapSection BitmapSection => _sectionSplitter.BitmapSection;
        private InodesSection InodesSection => _sectionSplitter.InodesSection;
        private DataBlocksSection DataBlocksSection => _sectionSplitter.DataBlocksSection;

        public FileSystem(IHardDrive hardDrive, int inodeAmount, int dataBlocksAmount, bool initFromDrive = false)
        {
            _hardDrive = hardDrive;
            _inodeAmount = inodeAmount;
            _dataBlocksAmount = dataBlocksAmount;
            _sectionSplitter = new SectionSplitter(_hardDrive, initFromDrive);
            if (!initFromDrive)
            {
                // todo create root directory ?
            }
        }

        public void SplitSections() => _sectionSplitter.SplitSections(_inodeAmount, _dataBlocksAmount);

        public void CreateDirectory(string name)
        {
            // todo
        }
    }

    public interface IFileSystem { }
}