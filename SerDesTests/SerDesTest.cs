using System;
using System.IO;
using HardDrive;
using NUnit.Framework;
using SerDes;
using Utils;

namespace SerDesTests
{
    public class Tests
    {
        private ISerDes _serDes;
        private IHardDrive _hardDrive;

        [SetUp]
        public void Setup()
        {
            _serDes = new SerDes.SerDes();
            _hardDrive = new HardDrive.HardDrive(_serDes);
        }

        [Test]
        public void Test1()
        {
            using (var stream = File.Open("test.txt", FileMode.OpenOrCreate))
            {
                var section = new BitmapSection(1, _hardDrive, false);
                section.OccupiedMask[0] = true;
                var byteArray = section.OccupiedMask.ToByteArray();
                _serDes.Write(stream, byteArray);
            }

            using (var stream = File.Open("test.txt", FileMode.OpenOrCreate))
            {
                var bytes = _serDes.Read(stream, (int) stream.Length);
                // var parsed = bytes.To<bool[]>();
                stream.Close();
                Assert.Pass();
            }
        }

        [Test]
        public void Test2()
        {
            using (var stream = File.Open("test2.txt", FileMode.OpenOrCreate))
            {
                _serDes.Write(stream, "test".ToByteArray().ByteArrayToBinaryStr());
            }

            using (var stream = File.Open("test2.txt", FileMode.Open))
            {
                var chars = _serDes.Read(stream, (int) stream.Length);
                var result = chars.BinaryCharsArrayToByteArray();
            }
        }

        [Test]
        public void Test3()
        {
            int prev = 0;
            for (int i = 0; i < 10; i++)
            {
                var binaryStr = new bool[i].ToByteArray().ByteArrayToBinaryStr();
                Console.WriteLine($"{i} === {binaryStr.Length} === {binaryStr.Length - prev}");
                prev = binaryStr.Length;
            }
        }

        [Test]
        public void TestDataBlocksCapacity()
        {
            int prev = 0;
            for (int i = 0; i < 10; i++)
            {
                var binaryStr = new string('0', i).ToByteArray().ByteArrayToBinaryStr();
                Console.WriteLine($"{i} === {binaryStr.Length} === {binaryStr.Length - prev}");
                prev = binaryStr.Length;
            }
        }

        [Test]
        public void TestBitmapSection()
        {
            var hardDrive = new HardDrive.HardDrive(_serDes, "test4.txt");
            var size = 2;
            var section = new BitmapSection(size, hardDrive, false);
            section.OccupiedMask[0] = true;
            section.SaveSection();
            var section2 = new BitmapSection(size, hardDrive, true);
            Assert.IsTrue(section.OccupiedMask.ContentsMatchOrdered(section2.OccupiedMask));
        }

        [Test]
        public void TestInodesSection()
        {
            var hardDrive = new HardDrive.HardDrive(_serDes, "test5.txt");
            var bitmapSize = 0;
            var section = new InodesSection(1, hardDrive, bitmapSize, false);
            section.Inodes[0].OccupiedDataBlocks = new BlockAddress[]
            {
                new BlockAddress(1),
            };
            section.SaveInode(section.Inodes[0]);
            var section2 = new InodesSection(1, hardDrive, bitmapSize, true);
            Assert.IsTrue(section.Inodes.ContentsMatch(section2.Inodes));
        }

        [Test]
        public void TestDataBlocksSection()
        {
            var hardDrive = new HardDrive.HardDrive(_serDes, "test6.txt");
            var bitmapSize = 0;
            var inodesSize = 0;
            var section = new DataBlocksSection(1, hardDrive, bitmapSize, inodesSize, false);
            section.WriteBlock(0, "TestDataBlocks".ToByteArray().ByteArrayToBinaryStr());
            var section2 = new DataBlocksSection(1, hardDrive, bitmapSize, inodesSize, true);
            Assert.IsTrue(section.ReadBlock(0).ContentsMatchOrdered(section2.ReadBlock(0)));
        }

        [Test]
        public void TestAllSection()
        {
            var hardDrive = new HardDrive.HardDrive(_serDes, "test7.txt");
            var size = 2;
            var bitmapSection = new BitmapSection(size, hardDrive, false);
            bitmapSection.OccupiedMask[0] = true;
            bitmapSection.SaveSection();

            var bitmapLength = bitmapSection.Length();
            var inodesSection = new InodesSection(1, hardDrive, bitmapLength, false);
            inodesSection.Inodes[0].OccupiedDataBlocks = new BlockAddress[]
            {
                new BlockAddress(1),
            };
            inodesSection.SaveInode(inodesSection.Inodes[0]);

            var inodesLength = inodesSection.Length();
            var dataBlocksSection = new DataBlocksSection(1, hardDrive, bitmapLength, inodesLength, false);
            dataBlocksSection.WriteBlock(0, "TestDataBlocks".ToByteArray().ByteArrayToBinaryStr());
            
            var bitmapSection1 = new BitmapSection(size, hardDrive, true);
            Assert.IsTrue(bitmapSection.OccupiedMask.ContentsMatchOrdered(bitmapSection1.OccupiedMask));
            var inodesSection1 = new InodesSection(1, hardDrive, bitmapLength, true);
            Assert.IsTrue(inodesSection.Inodes.ContentsMatch(inodesSection1.Inodes));
            var dataBlocksSection1 = new DataBlocksSection(1, hardDrive, bitmapLength, inodesLength, true);
            Assert.IsTrue(dataBlocksSection.ReadBlock(0).ContentsMatchOrdered(dataBlocksSection1.ReadBlock(0)));
        }
    }
}