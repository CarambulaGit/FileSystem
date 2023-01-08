using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace PathResolverTests
{
    public class PathResolverTests
    {
        [SetUp]
        public void Setup() { }

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
            RemoveDoubleDots(ref path);
            Assert.AreEqual(result, path);
        }
        
        private const string DoubleDotPattern =
            @"(\/(?!(?:\.(?:\/|\.\/))|(?:\/))[^\/]*(?=\/))*(?<" + ContentGroupName +
            @">(?:\/\.(?:(?=\/)))|\/+)*(\/\.\.(?:(?=\/)|(?=$)))";

        private const string ContentGroupName = "content";

        private void RemoveDoubleDots(ref string path)
        {
            var sb = new StringBuilder(path);
            var regex = new Regex(DoubleDotPattern);
            var match = regex.Match(sb.ToString());
            while (match.Success)
            {
                var newValue = string.Join(string.Empty,
                    match.Groups[ContentGroupName].Captures.Select(capture => capture.Value));
                sb.Replace(match.Value, newValue);
                match = regex.Match(sb.ToString());
            }

            path = sb.ToString();
        }
    }
}