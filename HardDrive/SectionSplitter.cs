namespace HardDrive
{
    public class SectionSplitter
    {
        private readonly IHardDrive _hardDrive;
        private bool _initFromDrive;
        public BitmapSection BitmapSection { get; private set; }
        public InodesSection InodesSection { get; private set; }
        public DataBlocksSection DataBlocksSection { get; private set; }

        public SectionSplitter(IHardDrive hardDrive, bool initFromDrive = false)
        {
            _initFromDrive = initFromDrive;
            _hardDrive = hardDrive;
        }

        public void SplitSections(int inodeAmount, int dataBlocksAmount)
        {
            BitmapSection = InitializeBitmap(dataBlocksAmount);
            InodesSection = InitializeInodes(inodeAmount, BitmapSection.Length());
            DataBlocksSection = InitializeHardDrive(dataBlocksAmount, BitmapSection.Length(), InodesSection.Length());
        }

        private BitmapSection InitializeBitmap(int dataBlocksAmount) =>
            new BitmapSection(dataBlocksAmount, _hardDrive, _initFromDrive);

        private InodesSection InitializeInodes(int inodeAmount, int bitmapSize) =>
            new InodesSection(inodeAmount, _hardDrive, bitmapSize, _initFromDrive);

        private DataBlocksSection InitializeHardDrive(int dataBlocksAmount, int bitmapSize, int inodesSize) =>
            new DataBlocksSection(dataBlocksAmount, _hardDrive, bitmapSize, inodesSize, _initFromDrive);
    }
}