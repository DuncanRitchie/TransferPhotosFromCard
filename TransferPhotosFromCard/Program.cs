using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TransferPhotosFromCard
{
    class Program
    {
        static string FolderToCopyTo = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

        enum DesiredActionForFileType
        {
            AskUserForAction, Ignore, Move, MoveAndRename
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
                    return DesiredActionForFileType.Ignore;
                default:
                    return DesiredActionForFileType.AskUserForAction;
            }
        }

        static void Main(string[] args)
        {
            AnnounceCurrentTask(ConsoleColor.Yellow, "Welcome to Duncan Ritchie’s photo-transferring app.");
            AnnounceCurrentTask("Looking for files on your memory card...\n");

            var groupedFilepaths = GetFilesFromCard()
                .GroupBy(file => GetDesiredActionForFileType(file))
                .OrderBy(group => group.Key);

            foreach (var group in groupedFilepaths)
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
            AskForUserInput("The program has finished. Type Enter to exit.");
            Console.ReadLine();
        }

        private static string[] GetFilesFromCard()
        {
            return Directory.GetFiles(@"D:\", "*", SearchOption.AllDirectories);
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
            AskForUserInput("Please enter y/n or true/false");
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
    }
}
