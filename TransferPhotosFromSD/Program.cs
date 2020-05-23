using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TransferPhotosFromSD
{
    class Program
    {
        enum DesiredActionForFileType
        {
            AskUser, Ignore, Move, MoveAndRename
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
                    return DesiredActionForFileType.AskUser;
            }
        }

        static void Main(string[] args)
        {
            var groupedFilepaths = GetFilesFromCard()
                .GroupBy(file => GetDesiredActionForFileType(file))
                .OrderBy(group => group.Key);

            foreach (var group in groupedFilepaths)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(group.Key);
                Console.ForegroundColor = ConsoleColor.White;
                switch (group.Key)
                {
                    case DesiredActionForFileType.Move:
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        foreach (string filepath in group)
                        {
                            Console.WriteLine(Path.GetFileName(filepath));
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    case DesiredActionForFileType.MoveAndRename:
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        foreach (string filepath in group)
                        {
                            Console.WriteLine($"{Path.GetFileName(filepath)} — created {File.GetCreationTime(filepath)}");
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    case DesiredActionForFileType.Ignore:
                        foreach (string filepath in group)
                        {
                            Console.WriteLine(Path.GetFileName(filepath));
                        }
                        break;
                    case DesiredActionForFileType.AskUser:
                        Console.ForegroundColor = ConsoleColor.Red;
                        foreach (string filepath in group)
                        {
                            Console.WriteLine($"{Path.GetFileName(filepath)} (what is this???)");
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Enum no longer matches switch block...");
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                }
            }
            Console.ReadLine();
        }

        private static string[] GetFilesFromCard()
        {
            return Directory.GetFiles(@"D:\", "*", SearchOption.AllDirectories);
        }
    }
}
