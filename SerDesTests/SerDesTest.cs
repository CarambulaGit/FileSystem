using System;
using System.IO;
using System.Text;
using HardDrive;
using NUnit.Framework;
using SerDes;

namespace SerDesTests
{
    public class Tests
    {
        private SerDes.SerDes _serDes;

        [SetUp]
        public void Setup()
        {
            _serDes = new SerDes.SerDes();
        }

        [Test]
        public void Test1()
        {
            using (var stream = File.Open("test.txt", FileMode.OpenOrCreate))
            {
                var section = new BitmapSection(1);
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
    }
}