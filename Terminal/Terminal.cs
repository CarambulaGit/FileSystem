using System;
using System.CommandLine;
using System.Linq;
using System.Threading.Tasks;
using FileSystem;
using Microsoft.Extensions.DependencyInjection;

namespace Terminal
{
    public class Terminal
    {
        private static bool _needToExit;
        private static IFileSystem _fileSystem;

        public static async Task<int> Main(string[] args)
        {
            _needToExit = false;
            var rootCommand = SetupCommands();

            while (!_needToExit)
            {
                if (_fileSystem != null)
                    Console.Write($"{_fileSystem.CurrentDirectoryPath}: ");
                args = Console.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                await rootCommand.InvokeAsync(args);
            }

            return 0;
        }

        private static RootCommand SetupCommands()
        {
            var rootCommand = new RootCommand("Terminal for using file system");
            SetupExitCommand(rootCommand);
            SetupMkfsCommand(rootCommand);
            SetupStatCommand(rootCommand);
            SetupLsCommand(rootCommand);
            SetupCreateCommand(rootCommand);
            SetupOpenCommand(rootCommand);
            SetupCloseCommand(rootCommand);
            SetupSeekCommand(rootCommand);
            SetupReadCommand(rootCommand);
            SetupWriteCommand(rootCommand);
            SetupLinkCommand(rootCommand);
            SetupUnlinkCommand(rootCommand);
            SetupTruncateCommand(rootCommand);
            SetupMkdirCommand(rootCommand);
            SetupRmdirCommand(rootCommand);
            SetupCdCommand(rootCommand);
            SetupSymlinkCommand(rootCommand);
            return rootCommand;
        }

        private static void SetupSymlinkCommand(RootCommand rootCommand)
        {
            var pathForLink = new Option<string>(
                    aliases: new[] {"-pl", "--pathForLink"},
                    description: "Path for link")
                {IsRequired = true};

            var pathToSavable = new Option<string>(
                    aliases: new[] {"-ps", "--pathToSavable"},
                    description: "Path to savable")
                {IsRequired = true};

            var symlinkCommand = new Command("symlink", "Create symlink")
            {
                pathForLink,
                pathToSavable
            };
            rootCommand.AddCommand(symlinkCommand);
            symlinkCommand.SetHandler(CreateSymlink, pathForLink, pathToSavable);
        }

        private static void SetupCdCommand(RootCommand rootCommand)
        {
            var path = new Option<string>(
                    aliases: new[] {"-p", "--path"},
                    description: "Path for new current directory")
                {IsRequired = true};

            var cdCommand = new Command("cd", "Change current working directory")
            {
                path
            };
            rootCommand.AddCommand(cdCommand);
            cdCommand.SetHandler(ChangeDirectory, path);
        }

        private static void SetupRmdirCommand(RootCommand rootCommand)
        {
            var pathToDir = new Option<string>(
                    aliases: new[] {"-p", "--path"},
                    description: "Path to directory")
                {IsRequired = true};

            var rmdirCommand = new Command("rmdir", "Delete directory at path")
            {
                pathToDir,
            };
            rootCommand.AddCommand(rmdirCommand);
            rmdirCommand.SetHandler(DeleteDir, pathToDir);
        }

        private static void SetupMkdirCommand(RootCommand rootCommand)
        {
            var path = new Option<string>(
                    aliases: new[] {"-p", "--path"},
                    description: "Path for directory")
                {IsRequired = true};

            var mkdirCommand = new Command("mkdir", "Create directory at path")
            {
                path,
            };
            rootCommand.AddCommand(mkdirCommand);
            mkdirCommand.SetHandler(MakeDir, path);
        }

        private static void SetupTruncateCommand(RootCommand rootCommand)
        {
            var descriptorOption = new Option<string>(
                    aliases: new[] {"-p", "--pathToFile"},
                    description: "Path to file")
                {IsRequired = true};

            var sizeOption = new Option<int>(
                    aliases: new[] {"-s", "--size"},
                    description: "Number of bytes to set")
                {IsRequired = true};

            var truncateCommand = new Command("truncate", "Change size of file in bytes")
            {
                descriptorOption,
                sizeOption
            };
            rootCommand.AddCommand(truncateCommand);
            truncateCommand.SetHandler(TruncateFile, descriptorOption, sizeOption);
        }

        private static void SetupUnlinkCommand(RootCommand rootCommand)
        {
            var pathToFile = new Option<string>(
                    aliases: new[] {"-p", "--pathToFile"},
                    description: "Path to file")
                {IsRequired = true};

            var unlinkCommand = new Command("unlink", "Delete hardlink of file")
            {
                pathToFile,
            };
            rootCommand.AddCommand(unlinkCommand);
            unlinkCommand.SetHandler(UnlinkFile, pathToFile);
        }

        private static void SetupLinkCommand(RootCommand rootCommand)
        {
            var pathToFile = new Option<string>(
                    aliases: new[] {"-pf", "--pathToFile"},
                    description: "Path to file")
                {IsRequired = true};

            var pathForLink = new Option<string>(
                    aliases: new[] {"-pl", "--pathForLink"},
                    description: "Path for link")
                {IsRequired = true};

            var linkCommand = new Command("link", "Create hardlink for file")
            {
                pathToFile,
                pathForLink
            };
            rootCommand.AddCommand(linkCommand);
            linkCommand.SetHandler(LinkFile, pathToFile, pathForLink);
        }

        private static void SetupWriteCommand(RootCommand rootCommand)
        {
            var descriptorOption = new Option<string>(
                    aliases: new[] {"-ds", "--descriptor"},
                    description: "Descriptor for this file")
                {IsRequired = true};

            var dataOption = new Option<string>(
                    aliases: new[] {"-dt", "--data"},
                    description: "Data to write")
                {IsRequired = true};

            var writeCommand = new Command("write", "Write data to file")
            {
                descriptorOption,
                dataOption
            };
            rootCommand.AddCommand(writeCommand);
            writeCommand.SetHandler(WriteFile, descriptorOption, dataOption);
        }

        private static void SetupReadCommand(RootCommand rootCommand)
        {
            var descriptorOption = new Option<string>(
                    aliases: new[] {"-d", "--descriptor"},
                    description: "Descriptor for this file")
                {IsRequired = true};

            var sizeOption = new Option<int>(
                    aliases: new[] {"-s", "--size"},
                    description: "Number of bytes to read")
                {IsRequired = true};

            var readCommand = new Command("read", "Read bytes from regular file")
            {
                descriptorOption,
                sizeOption
            };
            rootCommand.AddCommand(readCommand);
            readCommand.SetHandler(ReadFile, descriptorOption, sizeOption);
        }

        private static void SetupSeekCommand(RootCommand rootCommand)
        {
            var descriptorOption = new Option<string>(
                    aliases: new[] {"-d", "--descriptor"},
                    description: "Descriptor for this file")
                {IsRequired = true};

            var offsetOption = new Option<int>(
                    aliases: new[] {"-o", "--offset"},
                    description: "Offset for this file descriptor")
                {IsRequired = true};

            var seekCommand = new Command("seek", "Set offset for this file descriptor")
            {
                descriptorOption,
                offsetOption
            };
            rootCommand.AddCommand(seekCommand);
            seekCommand.SetHandler(SeekFile, descriptorOption, offsetOption);
        }

        private static void SetupCloseCommand(RootCommand rootCommand)
        {
            var descriptorOption = new Option<string>(
                    aliases: new[] {"-d", "--descriptor"},
                    description: "Descriptor for this file")
                {IsRequired = true};
            var closeCommand = new Command("close", "Close file")
            {
                descriptorOption
            };
            rootCommand.AddCommand(closeCommand);
            closeCommand.SetHandler(CloseRegularFile, descriptorOption);
        }

        private static void SetupOpenCommand(RootCommand rootCommand)
        {
            var descriptorOption = new Option<string>(
                    aliases: new[] {"-d", "--descriptor"},
                    description: "Descriptor for this file")
                {IsRequired = true};
            var pathOption = new Option<string>(
                    aliases: new[] {"-p", "--path"},
                    description: "Path to create savable")
                {IsRequired = true};
            var openCommand = new Command("open", "Open file")
            {
                descriptorOption,
                pathOption
            };
            rootCommand.AddCommand(openCommand);
            openCommand.SetHandler(OpenRegularFile, descriptorOption, pathOption);
        }

        private static void SetupCreateCommand(RootCommand rootCommand)
        {
            var pathOption = new Option<string>(
                    aliases: new[] {"-p", "--path"},
                    description: "Path to create savable")
                {IsRequired = true};
            var createCommand = new Command("create", "Create regular file")
            {
                pathOption
            };
            rootCommand.AddCommand(createCommand);
            createCommand.SetHandler(CreateRegularFile, pathOption);
        }

        private static void SetupLsCommand(RootCommand rootCommand)
        {
            var lsCommand = new Command("ls", "Print data of current folder");
            rootCommand.AddCommand(lsCommand);
            lsCommand.SetHandler(GetCWDData);
        }

        private static void SetupStatCommand(RootCommand rootCommand)
        {
            var pathOption = new Option<string>(
                    aliases: new[] {"-p", "--path"},
                    description: "Path to savable")
                {IsRequired = true};
            var statCommand = new Command("stat", "Show info about savable")
            {
                pathOption
            };
            rootCommand.AddCommand(statCommand);
            statCommand.SetHandler(ShowSavableStat, pathOption);
        }

        private static void SetupMkfsCommand(RootCommand rootCommand)
        {
            var inodesAmountOption = new Option<int>(
                    aliases: new[] {"-na", "--inodesAmount"},
                    description: "Number of inodes in file system")
                {IsRequired = true};

            var readFromDiskOption = new Option<bool>(
                aliases: new[] {"-rfd", "--readFromDisk"},
                description: "Read already existing file");

            var mkfsCommand = new Command("mkfs", "Initialize file system")
            {
                inodesAmountOption,
                readFromDiskOption
            };
            rootCommand.AddCommand(mkfsCommand);
            mkfsCommand.SetHandler(InitFileSystem,
                inodesAmountOption, readFromDiskOption);
        }

        private static void SetupExitCommand(RootCommand rootCommand)
        {
            var exitCommand = new Command("exit", "Stop program");
            rootCommand.AddCommand(exitCommand);
            exitCommand.SetHandler(() => { _needToExit = true; });
        }

        private static void InitFileSystem(int inodeAmount, bool initFromDrive)
        {
            if (_fileSystem != null)
            {
                Console.WriteLine("File system already initialized");
                return;
            }

            var dataBlocksAmount = inodeAmount * 2;
            var services = Program.SetupDI((inodeAmount, dataBlocksAmount, initFromDrive));
            _fileSystem = services.GetRequiredService<IFileSystem>();
            _fileSystem.Initialize();
        }

        private static void ShowSavableStat(string path) =>
            ExceptionHandlerWrapper(() => Console.WriteLine(_fileSystem.GetInodeData(path)));

        private static void GetCWDData() => ExceptionHandlerWrapper(() => Console.WriteLine(_fileSystem.GetCWDData()));

        private static void CreateRegularFile(string path) =>
            ExceptionHandlerWrapper(() => _fileSystem.CreateFile(path));

        private static void OpenRegularFile(string descriptor, string path) =>
            ExceptionHandlerWrapper(() => _fileSystem.OpenFile(path, descriptor));

        private static void CloseRegularFile(string descriptor) =>
            ExceptionHandlerWrapper(() => _fileSystem.CloseFile(descriptor));

        private static void SeekFile(string descriptor, int offset) =>
            ExceptionHandlerWrapper(() => _fileSystem.SeekFile(descriptor, offset));

        private static void ReadFile(string descriptor, int size) =>
            ExceptionHandlerWrapper(() =>
                Console.WriteLine(string.Join("", _fileSystem.ReadFile(descriptor, size).Select(b => (char) b))));

        private static void WriteFile(string descriptor, string data)
        {
            ExceptionHandlerWrapper(() =>
            {
                var byteArray = data.ToCharArray().Select(chr => (byte) chr).ToArray();
                _fileSystem.SaveFile(descriptor, byteArray);
            });
        }

        private static void LinkFile(string pathToFile, string pathForLink) =>
            ExceptionHandlerWrapper(() => _fileSystem.LinkFile(pathToFile, pathForLink));

        private static void UnlinkFile(string pathToFile) =>
            ExceptionHandlerWrapper(() => _fileSystem.DeleteFile(pathToFile));

        private static void TruncateFile(string pathToFile, int size) =>
            ExceptionHandlerWrapper(() => _fileSystem.TruncateFile(pathToFile, size));

        private static void MakeDir(string path) => ExceptionHandlerWrapper(() => _fileSystem.CreateDirectory(path));

        private static void DeleteDir(string path) => ExceptionHandlerWrapper(() => _fileSystem.DeleteDirectory(path));

        private static void ChangeDirectory(string path) =>
            ExceptionHandlerWrapper(() => _fileSystem.ChangeCurrentDirectory(path));

        private static void CreateSymlink(string pathForLink, string pathToSavable) =>
            ExceptionHandlerWrapper(() => _fileSystem.CreateSymlink(pathForLink, pathToSavable));

        private static void ExceptionHandlerWrapper(Action action)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}