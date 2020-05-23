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
        static void Main(string[] args)
        {
            var groupedFilepaths = GetFilesFromCard()
                .GroupBy(file => Path.GetExtension(file).ToLower())
                .OrderBy(group => group.Key);

            foreach (var group in groupedFilepaths)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(group.Key);
                Console.ForegroundColor = ConsoleColor.White;
                switch (group.Key)
                {
                    case ".jpg":
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        foreach (string filepath in group.ToList())
                        {
                            Console.WriteLine(Path.GetFileName(filepath));
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    case ".mts":
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        foreach (string filepath in group.ToList())
                        {
                            Console.WriteLine($"{Path.GetFileName(filepath)} — created {File.GetCreationTime(filepath)}");
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
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
                        foreach (string filepath in group.ToList())
                        {
                            Console.WriteLine($"{Path.GetFileName(filepath)} (not worth copying)");
                        }
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        foreach (string filepath in group.ToList())
                        {
                            Console.WriteLine($"{Path.GetFileName(filepath)} (what is this???)");
                        }
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
