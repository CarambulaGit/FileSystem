using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FileSystem.Savable;
using HardDrive;
using SerDes;
using Utils;
using Directory = FileSystem.Savable.Directory;

namespace FileSystem
{
    public class PathResolver : IPathResolver
    {
        private const string MultipleSeparatorsPattern = @"\/{2,}";
        private const string SingleDotPattern = @"\/\.((?=\/)|(?=$))";

        private const string DoubleDotPattern =
            @"(\/(?!(?:\.(?:\/|\.\/))|(?:\/))[^\/]*(?=\/))*(?<" + ContentGroupName +
            @">(?:\/\.(?:(?=\/)))|\/+)*(\/\.\.(?:(?=\/)|(?=$)))";

        private const string ContentGroupName = "content";

        private readonly Lazy<IFileSystem> _fileSystem;
        private IFileSystem FileSystem => _fileSystem.Value;

        public PathResolver(Lazy<IFileSystem> fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public string Resolve(string path) =>
            ResolveAbsolutePath(IsPathAbsolute(path)
                ? path
                : Path.Combine(GetWorkingDirectoryPath(), path)
            );

        public string Resolve(Directory directory)
        {
            Inode GetInodeById(int curParentInodeId) => FileSystem.InodesSection.Inodes[curParentInodeId];

            if (directory == FileSystem.RootDirectory)
            {
                return FileSystem.RootDirectoryPath;
            }

            var sb = new StringBuilder();
            var curParentInode = GetInodeById(directory.GetParentDirectoryInodeId());
            while (curParentInode.Id != FileSystem.RootDirectory.Inode.Id)
            {
                sb.Append(curParentInode.FileNames[0]);
                var parentDir = FileSystem.ReadDirectory(curParentInode);
                curParentInode = GetInodeById(parentDir.GetParentDirectoryInodeId());
            }

            // todo check
            return sb.ToString();
        }

        public bool IsPathAbsolute(string path) => path.StartsWith(FileSystem.RootDirectoryPath);

        private string ResolveAbsolutePath(string path)
        {
            RemoveMultipleSeparators(ref path);
            RemoveSingleDots(ref path);
            RemoveDoubleDots(ref path);
            return path.IsNullOrEmpty() ? FileSystem.RootDirectoryPath : path;
        }

        private void RemoveMultipleSeparators(ref string path) =>
            path = Regex.Replace(path, MultipleSeparatorsPattern, string.Empty);

        private void RemoveSingleDots(ref string path) => path = Regex.Replace(path, SingleDotPattern, "");

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

        private string GetWorkingDirectoryPath() => Resolve(FileSystem.CurrentDirectory);
    }

    public interface IPathResolver
    {
        string Resolve(string path);
        string Resolve(Directory directory);
    }
}