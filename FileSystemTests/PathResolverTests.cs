using System;
using FileSystem;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace PathResolverTests
{
    public class PathResolverTests
    {
        private IFileSystem _fileSystem;
        private IPathResolver _pathResolver;
        private IServiceProvider _services;

        [SetUp]
        public void Setup()
        {
            (int inodesAmount, int dataBlocksAmount, bool initFromDrive) fileSystemConfiguration = (20, 40, false);
            _services = Program.SetupDI(Array.Empty<string>(), fileSystemConfiguration);
            _fileSystem = _services.GetRequiredService<IFileSystem>();
            _pathResolver = _services.GetRequiredService<IPathResolver>();
            _fileSystem.Initialize();
        }

        [Test]
        [TestCase("/", "/")]
        [TestCase("/.", "/.")]
        [TestCase("/./", "/./")]
        [TestCase("/..", "")]
        [TestCase("//./..", "//.")]
        [TestCase("/bad/..", "")]
        [TestCase("/bad///..", "//")]
        [TestCase("/bad/././..", "/./.")]
        [TestCase("/bad/../../..", "")]
        [TestCase("/bad/../guy/../..", "")]
        public void RemoveDoubleDotsTest(string path, string result)
        {
            // _pathResolver.RemoveDoubleDots(ref path);
            Assert.AreEqual(result, path);
        }
    }
}