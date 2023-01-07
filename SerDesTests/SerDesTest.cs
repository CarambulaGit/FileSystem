using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HardDrive;
using NUnit.Framework;
using SerDes;

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
        public void TestBitmapSection()
        {
            var hardDrive = new HardDrive.HardDrive(_serDes, "test4.txt");
            var section = new BitmapSection(1, hardDrive, false);
            var section2 = new BitmapSection(1, hardDrive, true);
            Console.WriteLine(section.OccupiedMask.ContentsMatch(section2.OccupiedMask));
        }

        [Test]
        public void TestInodesSection()
        {
            var hardDrive = new HardDrive.HardDrive(_serDes, "test5.txt");
            var bitmapSize = 0;
            var section = new InodesSection(1, hardDrive, bitmapSize, false);
            var section2 = new InodesSection(1, hardDrive, bitmapSize, true);
            Console.WriteLine(section.Inodes.ContentsMatch(section2.Inodes));
        }

        [Test]
        public void TestDataBlocksSection()
        {
            var hardDrive = new HardDrive.HardDrive(_serDes, "test6.txt");
            var bitmapSize = 0;
            var inodesSize = 0;
            var section = new DataBlocksSection(1, hardDrive, bitmapSize, inodesSize, false);
            section.WriteBlock(0, "TestDataBlocks");
            var section2 = new DataBlocksSection(1, hardDrive, bitmapSize, inodesSize, true);
            Console.WriteLine(section.ReadBlock(0).ContentsMatch(section2.ReadBlock(0)));
        }

        [Test]
        public void Test()
        {
           Console.WriteLine(FileType.RegularFile.ToString());
        }
    }
}