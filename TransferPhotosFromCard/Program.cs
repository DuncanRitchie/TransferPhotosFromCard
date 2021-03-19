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
        private static string Disk = @"D:\";
        private static string FolderToCopyTo = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

        private enum DesiredActionForFileType
        {
            AskUserForAction, Delete, Ignore, Move, MoveAndRename
        }

        private static DesiredActionForFileType GetDesiredActionForFileType(string filepath)
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
            Announce(ConsoleColor.Yellow, "Welcome to Duncan Ritchie’s photo-transferring app!");
            Announce("Looking for files on your memory-card...\n");

            var groupedFilepaths = GetFilesFromCard(true)
                .GroupBy(file => GetDesiredActionForFileType(file))
                .OrderBy(group => group.Key);

            foreach (var group in groupedFilepaths)
            {
                ProcessOneGroupOfFiles(group);
            }

            CopyDefaultDiskContents();
            DeleteEmptySubdirectories(Disk);

            AskUserAQuestion("The program has finished. Type Enter to exit.");
            Console.ReadLine();
        }

        //// Returns an array of filepaths.
        private static string[] GetFilesFromCard(bool warnIfNoDisk)
        {
            if (Directory.Exists(Disk))
            {
                //// Default drive is D:\, but if D:\ is a hard drive, this might throw
                //// due to files being “inaccessible”. So we try E:\ if that happens.
                try
                {
                    return Directory.GetFiles(Disk, "*", SearchOption.AllDirectories);
                }
                catch
                {
                    Disk = @"E:\";
                    return GetFilesFromCard(false);
                }
            }
            //// If there’s no card, it waits for a card to be inserted, then returns the filepaths.
            else
            {
                if (warnIfNoDisk) {
                    Announce(ConsoleColor.Red, $"Disk {Disk} does not exist. Please insert a memory-card.");
                }
                else
                {
                    //// Prevent a StackOverflowException due to too much recursion!
                    Thread.Sleep(1000);
                }
                return GetFilesFromCard(false);
            }
        }

        //// Do whatever bulk action is appropriate for a group of files for which a common action is appropriate.
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

        //// The contents of “DefaultDiskContents” folder allow my camera to maintain
        //// an internal catalogue of photos and videos. 
        //// If these catalogue files are deleted and not replaced on the memory-card,
        //// my camera will spend a few seconds re-creating them before I can use it.
        private static void CopyDefaultDiskContents()
        {
            AskUserAQuestion("Do you want to copy default contents to the disk?");
            if (GetBoolFromUser())
            {
                DirectoryInfo sourceDir = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory,
                          @"..\..\DefaultDiskContents"));

                if (sourceDir.Exists)
                {
                    Announce("Copying default disk contents to ", Disk, "...");
                    CopyDirectory(sourceDir.FullName, Disk);
                }
                else
                {
                    Announce("Sorry, the default disk contents do not exist.");
                }
            }
        }

        //// Adapted from https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
        private static void CopyDirectory(string sourceDirName, string destDirName)
        {
            //// Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            //// If the destination directory doesn’t exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            //// Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                Announce("Copying ", file.Name, " to ", destDirName, "...");
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, true);
            }

            //// Copy subdirectories and their contents to their new location.
            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destDirName, subdir.Name);
                CopyDirectory(subdir.FullName, temppath);
            }
        }

        //// Actions to be applied to files individually, listed in the enum `DesiredActionForFileType`.
        private static void Move(string filepath)
        {
            string filename = Path.GetFileName(filepath);
            string newPath = Path.Combine(FolderToCopyTo, filename);
            Announce("Moving ", filename, " to ", FolderToCopyTo, "...");
            File.Move(filepath, newPath);
        }

        private static void MoveAndRename(string filepath)
        {
            string filename = Path.GetFileName(filepath);
            string newFileName = $"{File.GetCreationTime(filepath).ToString("yyyy-MM-dd hh-mm-ss")}{Path.GetExtension(filepath)}";
            string newPath = Path.Combine(FolderToCopyTo, newFileName);
            Announce("Renaming ", filename, " to ", newFileName, " and moving it to ", FolderToCopyTo, "...");
            File.Move(filepath, newPath);
        }

        private static void Ignore(string filepath)
        {
            Announce("", Path.GetFileName(filepath), " will not be copied.");
        }

        private static void Delete(string filepath)
        {
            Announce("Deleting ", Path.GetFileName(filepath), "...");
            File.Delete(filepath);
        }

        private static void AskUserForAction(string filepath)
        {
            AskUserAQuestion("I don’t know what to do with ", Path.GetFileName(filepath), ". Should I move it onto your computer?");
            if (GetBoolFromUser())
            {
                AskUserAQuestion("Should I rename it?");
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

        //// Apply a given action to multiple given files.
        private static void PerformActionOnSeveralFiles(Action<string> action, IEnumerable<string> filepaths)
        {
            foreach (string filepath in filepaths)
            {
                action(filepath);
            }
        }

        private static void PerformActionOnSeveralFilesIfUserAllows(Action<string> action, IEnumerable<string> filepaths, string questionToAskUser)
        {
            AskUserAQuestion(questionToAskUser);
            if (GetBoolFromUser())
            {
                PerformActionOnSeveralFiles(action, filepaths);
            }
            else
            {
                Announce("We will not do that then.");
            }
        }

        //// Ask user to enter a value and return a boolean based off it.
        private static bool GetBoolFromUser()
        {
            AskUserAQuestion("Please enter y/n/true/false.");
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

        //// Overloaded methods based on `Announce`.
        private static void AskUserAQuestion(string question)
        {
            Announce(ConsoleColor.Magenta, question);
        }

        private static void AskUserAQuestion(params string[] question)
        {
            Announce(ConsoleColor.Magenta, question);
        }

        private static void Announce(string message)
        {
            Announce(ConsoleColor.Cyan, message);
        }

        private static void Announce(ConsoleColor colour, string message)
        {
            Console.ForegroundColor = colour;
            Console.WriteLine(message);
        }

        private static void Announce(params string[] message)
        {
            Announce(ConsoleColor.Cyan, message);
        }

        //// Write to console, alternating between the given colour and white.
        private static void Announce(ConsoleColor colour, params string[] message)
        {
            for (int i = 0; i < message.Length; i++)
            {
                if (i % 2 == 0) { Console.ForegroundColor = colour; }
                else { Console.ForegroundColor = ConsoleColor.White; }
                Console.Write(message[i]);
            }
            Console.Write("\n");
        }

        private static void AnnounceFile(string filepath)
        {
            Announce("Found file: ", Path.GetFileName(filepath));
        }

        private static void AnnounceGroup(IGrouping<DesiredActionForFileType, string> group)
        {
            PerformActionOnSeveralFiles(AnnounceFile, group);
        }

        //// Recursively delete `directory` and any subdirectories, where empty.
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
                    Announce("Deleting empty directory ", directory, "...");
                    Directory.Delete(directory);
                }
            }
        }

        //// Delete subfolders, but not `superdirectory`.
        private static void DeleteEmptySubdirectories(string superdirectory)
        {
            foreach (string subdirectory in Directory.GetDirectories(superdirectory))
            {
                DeleteEmptyDirectory(subdirectory);
            }
            Console.WriteLine();
        }
    }
}
