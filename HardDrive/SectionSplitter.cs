namespace HardDrive
{
    public class SectionSplitter
    {
        private readonly IHardDrive _hardDrive;
        public BitmapSection BitmapSection { get; private set; }
        public InodesSection InodesSection { get; private set; }
        public HardDriveSection HardDriveSection { get; private set; }

        public SectionSplitter(IHardDrive hardDrive)
        {
            _hardDrive = hardDrive;
        }

        public void SplitSections(int inodeAmount, int dataBlocksAmount)
        {
            var sections = InitializeSections(inodeAmount, dataBlocksAmount);
            BitmapSection = sections.bitmap;
            InodesSection = sections.inodes;
            HardDriveSection = sections.hardDrive;
        }

        private (BitmapSection bitmap, InodesSection inodes, HardDriveSection hardDrive) InitializeSections(int inodeAmount, int dataBlocksAmount)
        {
            var bitmap = InitializeBitmap(dataBlocksAmount);
            var inodes = InitializeInodes();
            var hardDrive = InitializeHardDrive();
            return (bitmap, inodes, hardDrive);
        }

        private BitmapSection InitializeBitmap(int dataBlocksAmount)
        {
            _hardDrive.Read(dataBlocksAmount);
            return new BitmapSection(1); // todo
        }

        private InodesSection InitializeInodes()
        {
            throw new System.NotImplementedException();
        }

        private HardDriveSection InitializeHardDrive()
        {
            throw new System.NotImplementedException();
        }
    }
}