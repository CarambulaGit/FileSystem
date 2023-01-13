using SerDes;

namespace HardDrive
{
    public class SectionSplitter
    {
        private readonly IHardDrive _hardDrive;
        private bool _initFromDrive;
        private ISerDes _serDes;
        public BitmapSection BitmapSection { get; private set; }
        public InodesSection InodesSection { get; private set; }
        public DataBlocksSection DataBlocksSection { get; private set; }

        public SectionSplitter(ISerDes serDes, IHardDrive hardDrive, bool initFromDrive = false)
        {
            _initFromDrive = initFromDrive;
            _serDes = serDes;
            _hardDrive = hardDrive;
        }

        public void SplitSections(int inodeAmount, int dataBlocksAmount)
        {
            BitmapSection = InitializeBitmap(dataBlocksAmount);
            InodesSection = InitializeInodes(inodeAmount, BitmapSection.Length());
            DataBlocksSection = InitializeHardDrive(dataBlocksAmount, BitmapSection.Length(), InodesSection.Length());
        }

        private BitmapSection InitializeBitmap(int dataBlocksAmount) =>
            new BitmapSection(dataBlocksAmount, _serDes, _hardDrive, _initFromDrive);

        private InodesSection InitializeInodes(int inodeAmount, int bitmapSize) =>
            new InodesSection(inodeAmount, _serDes, _hardDrive, bitmapSize, _initFromDrive);

        private DataBlocksSection InitializeHardDrive(int dataBlocksAmount, int bitmapSize, int inodesSize) =>
            new DataBlocksSection(dataBlocksAmount, _serDes, _hardDrive, bitmapSize, inodesSize, _initFromDrive);
    }
}