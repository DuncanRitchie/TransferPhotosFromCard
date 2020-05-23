using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace TransferPhotosFromCard
{
    class Program
    {
        static string Disk = @"D:\";
        static string FolderToCopyTo = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

        enum DesiredActionForFileType
        {
            AskUserForAction, Delete, Ignore, Move, MoveAndRename
        }

        static DesiredActionForFileType GetDesiredActionForFileType(string filepath)
        {
            switch (Path.GetExtension(filepath).ToLower())
            {
                case ".jpg":
                    return DesiredActionForFileType.Move;
                case ".mts":
                    return DesiredActionForFileType.MoveAndRename;
                case "":
                case ".bdm":
                case ".bin":
                case ".bnp":
                case ".cpi":
                case ".dat":
                case ".inp":
                case ".int":
                case ".mpl":
                case ".xml":
                    return DesiredActionForFileType.Delete;
                default:
                    return DesiredActionForFileType.AskUserForAction;
            }
        }

        static void Main(string[] args)
        {
            Console.Title = "Transfer Photos from Card";
            AnnounceCurrentTask(ConsoleColor.Yellow, "Welcome to Duncan Ritchie’s photo-transferring app.");
            AnnounceCurrentTask("Looking for files on your memory card...\n");

            var groupedFilepaths = GetFilesFromCard(true)
                .GroupBy(file => GetDesiredActionForFileType(file))
                .OrderBy(group => group.Key);

            foreach (var group in groupedFilepaths)
            {
                ProcessOneGroupOfFiles(group);
            }

            CopyDefaultDiskContents();
            DeleteEmptySubdirectories(Disk);

            AskForUserInput("The program has finished. Type Enter to exit.");
            Console.ReadLine();
        }

        //// If there’s no card, it waits for a card to be inserted, then returns the filepaths.
        private static string[] GetFilesFromCard(bool warnIfNoDisk)
        {
            if (Directory.Exists(Disk))
            {
                return Directory.GetFiles(Disk, "*", SearchOption.AllDirectories);
            }
            else
            {
                if (warnIfNoDisk) {
                    AnnounceCurrentTask(ConsoleColor.Red, $"Disk {Disk} does not exist. Please insert a memory card.");
                }
                else
                {
                    //// Prevent a StackOverflowException due to too much recursion!
                    Thread.Sleep(1000);
                }
                return GetFilesFromCard(false);
            }
        }

        private static void ProcessOneGroupOfFiles(IGrouping<DesiredActionForFileType, string> group)
        {
            AnnounceGroup(group);
            switch (group.Key)
            {
                case DesiredActionForFileType.Move:
                    PerformActionOnSeveralFilesIfUserAllows(Move, group, "Do you want to move these files?");
                    break;
                case DesiredActionForFileType.MoveAndRename:
                    PerformActionOnSeveralFilesIfUserAllows(MoveAndRename, group, "Do you want to move and rename these files?");
                    break;
                case DesiredActionForFileType.Ignore:
                    PerformActionOnSeveralFiles(Ignore, group);
                    break;
                case DesiredActionForFileType.Delete:
                    PerformActionOnSeveralFilesIfUserAllows(Delete, group, "Do you want to delete these files?");
                    break;
                case DesiredActionForFileType.AskUserForAction:
                    PerformActionOnSeveralFiles(AskUserForAction, group);
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Enum no longer matches switch block...");
                    break;
            }
            Console.WriteLine();
        }

        private static void CopyDefaultDiskContents()
        {
            AskForUserInput("Do you want to copy default contents to the disk?");
            if (GetBoolFromUser())
            {
                DirectoryInfo sourceDir = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory,
                          @"..\..\DefaultDiskContents"));

                if (sourceDir.Exists)
                {
                    AnnounceCurrentTask("Copying default disk contents to ", Disk, "...");
                    CopyDirectory(sourceDir.FullName, Disk);
                }
                else
                {
                    AnnounceCurrentTask("Sorry, the default disk contents do not exist.");
                }
            }
        }

        //// Adapted from https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
        private static void CopyDirectory(string sourceDirName, string destDirName)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                AnnounceCurrentTask("Copying ", file.Name, " to ", destDirName, "...");
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, true);
            }

            // Copy subdirectories and their contents to new location.
            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destDirName, subdir.Name);
                CopyDirectory(subdir.FullName, temppath);
            }
        }

        private static void Move(string filepath)
        {
            string filename = Path.GetFileName(filepath);
            string newPath = Path.Combine(FolderToCopyTo, filename);
            AnnounceCurrentTask("Moving ", filename, " to ", FolderToCopyTo, "...");
            File.Move(filepath, newPath);
        }

        private static void MoveAndRename(string filepath)
        {
            string filename = Path.GetFileName(filepath);
            string newFileName = $"{File.GetCreationTime(filepath).ToString("yyyy-MM-dd hh-mm-ss")}{Path.GetExtension(filepath)}";
            string newPath = Path.Combine(FolderToCopyTo, newFileName);
            AnnounceCurrentTask("Renaming ", filename, " to ", newFileName, " and moving it to ", FolderToCopyTo, "...");
            File.Move(filepath, newPath);
        }

        private static void Ignore(string filepath)
        {
            AnnounceCurrentTask("", Path.GetFileName(filepath), " will not be copied.");
        }

        private static void Delete(string filepath)
        {
            AnnounceCurrentTask("Deleting ", Path.GetFileName(filepath), "...");
            File.Delete(filepath);
        }

        private static void AskUserForAction(string filepath)
        {
            AskForUserInput($"I don’t know what to do with {Path.GetFileName(filepath)}. Should I copy it?");
            if (GetBoolFromUser())
            {
                AskForUserInput("Should I rename it?");
                if (GetBoolFromUser())
                {
                    MoveAndRename(filepath);
                }
                else { Move(filepath); }
            }
            else
            {
                Ignore(filepath);
            }
        }

        private static void PerformActionOnSeveralFiles(Action<string> action, IEnumerable<string> filepaths)
        {
            foreach (string filepath in filepaths)
            {
                action(filepath);
            }
        }

        private static void PerformActionOnSeveralFilesIfUserAllows(Action<string> action, IEnumerable<string> filepaths, string questionToAskUser)
        {
            AskForUserInput(questionToAskUser);
            if (GetBoolFromUser())
            {
                foreach (string filepath in filepaths)
                {
                    action(filepath);
                }
            }
            else
            {
                AnnounceCurrentTask("We will not do that then.");
            }
        }

        private static bool GetBoolFromUser()
        {
            AskForUserInput("Please enter y/n/true/false.");
            Console.ForegroundColor = ConsoleColor.White;
            string userEntry = Console.ReadLine();
            if (userEntry.Contains("y") || userEntry.Contains("true"))
            {
                return true;
            }
            else if (userEntry.Contains("n") || userEntry.Contains("false"))
            {
                return false;
            }
            else
            {
                return GetBoolFromUser();
            }
        }

        private static void AskForUserInput(string question)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(question);
        }

        private static void AnnounceCurrentTask(string message)
        {
            AnnounceCurrentTask(ConsoleColor.Cyan, message);
        }

        private static void AnnounceCurrentTask(ConsoleColor colour, string message)
        {
            Console.ForegroundColor = colour;
            Console.WriteLine(message);
        }

        private static void AnnounceCurrentTask(params string[] message)
        {
            for (int i = 0; i < message.Length; i++)
            {
                if (i % 2 == 0) { Console.ForegroundColor = ConsoleColor.Cyan; }
                else { Console.ForegroundColor = ConsoleColor.White; }
                Console.Write(message[i]);
            }
            Console.Write("\n");
        }

        private static void AnnounceFile(string filepath)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Found file: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(Path.GetFileName(filepath));
        }

        private static void AnnounceGroup(IGrouping<DesiredActionForFileType, string> group)
        {
            PerformActionOnSeveralFiles(AnnounceFile, group);
        }

        private static void DeleteEmptyDirectory(string directory)
        {
            if (Directory.GetFiles(directory, "*", SearchOption.AllDirectories).Length == 0)
            {
                foreach (string subdirectory in Directory.GetDirectories(directory))
                {
                    DeleteEmptyDirectory(subdirectory);
                }
                if (Directory.GetDirectories(directory).Length == 0)
                {
                    AnnounceCurrentTask("Deleting empty directory ", directory, "...");
                    Directory.Delete(directory);
                }
            }
        }

        private static void DeleteEmptySubdirectories(string directory)
        {
            foreach (string subdirectory in Directory.GetDirectories(directory))
            {
                DeleteEmptyDirectory(subdirectory);
            }
            Console.WriteLine();
        }
    }
}
