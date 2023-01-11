using System;
using System.Collections.Generic;
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
        public string ParentDirectory => "..";

        public string CurrentDirectory => ".";

        public PathResolver(Lazy<IFileSystem> fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public string Resolve(string path) =>
            ResolveAbsolutePath(IsPathAbsolute(path)
                ? path
                : $"{GetWorkingDirectoryPath()}{Path.AltDirectorySeparatorChar}{path}"
            );

        public string Resolve(Directory directory)
        {
            Inode GetInodeById(int curParentInodeId) => FileSystem.InodesSection.Inodes[curParentInodeId];

            if (directory.Inode.Id == FileSystem.RootDirectory.Inode.Id)
            {
                return FileSystem.RootName;
            }

            var pathParts = new List<string> {directory.Inode.FileNames[0]};
            var curParentInode = GetInodeById(directory.GetParentDirectoryInodeId());
            while (curParentInode.Id != FileSystem.RootDirectory.Inode.Id)
            {
                pathParts.Add(curParentInode.FileNames[0]);
                var parentDir = FileSystem.ReadDirectory(curParentInode);
                curParentInode = GetInodeById(parentDir.GetParentDirectoryInodeId());
            }

            pathParts.Add(FileSystem.RootName);
            pathParts.Reverse();
            return string.Join(Path.AltDirectorySeparatorChar, pathParts);
        }

        public (string pathToSavable, string savableName) SplitPath(string absolutePath)
        {
            if (IsPathAbsolute(absolutePath))
            {
                throw new PathMustBeAbsoluteException(absolutePath);
            }

            var indexOfLastSplitter = absolutePath.LastIndexOf(Path.AltDirectorySeparatorChar);
            return absolutePath.SplitByIndex(indexOfLastSplitter);
        }

        private bool IsPathAbsolute(string path) => path.StartsWith(FileSystem.RootDirectoryPath);

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
        string ParentDirectory { get; }
        string CurrentDirectory { get; }
        (string pathToSavable, string savableName) SplitPath(string absolutePath);
    }
}