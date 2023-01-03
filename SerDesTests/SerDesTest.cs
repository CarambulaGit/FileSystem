using System;
using System.IO;
using NUnit.Framework;

namespace SerDesTests
{
    public class Tests
    {
        private SerDes.SerDes _serDes;

        [SetUp]
        public void Setup()
        {
            _serDes = new SerDes.SerDes(new HardDrive.HardDrive());
        }

        [Test]
        public void Test1()
        {
            _serDes.Write(new byte[] {1, 2, 3, 5, 6, 8});
            Console.WriteLine(_serDes.Read(1));
            Assert.Pass();
        }
    }
}