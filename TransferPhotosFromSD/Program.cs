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
                foreach (string filepath in group.ToList())
                {
                    Console.WriteLine(Path.GetFileName(filepath));
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
