using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Patcher3
{
    class Startup
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Title = "pat3";
                Console.WriteLine("pat3");
                if (File.Exists("patch.pt3"))
                {
                    Console.WriteLine("patch.pt3 found, using default file.");
                    Console.WriteLine("");
                    Patcher.PatMetadata metadata = Patcher.GetMetadata(AppDomain.CurrentDomain.BaseDirectory + "\\patch.pt3");
                    string filename = metadata.name;
                    string author = metadata.author;
                    int i = 3;
                    while (i != -1)
                    {
                        Console.CursorLeft = 0;
                        Console.WriteLine($"starting in {i} \"{filename}\" by {author}");
                        System.Threading.Thread.Sleep(1000);
                        i--;
                        Console.CursorTop--;
                    }
                    Console.WriteLine("");
                    Patcher.PatchFromFile("patch.pt3", true);
                }
                else
                {
                    Console.WriteLine("patch.pt3 was not found, please specify file.");
                    Console.WriteLine("");
                    int choice = 1;
                    string pathLocation = "";
                    while (choice == 1)
                    {
                        choice = -1;
                        pathLocation = ConsoleUI.ShowTextfield(35);
                        ConsoleUI.ClearTextfield(35);
                        Patcher.PatMetadata metadata = Patcher.GetMetadata(AppDomain.CurrentDomain.BaseDirectory + "\\" + pathLocation);
                        string filename = metadata.name;
                        string author = metadata.author;
                        Console.WriteLine($"are you sure you want to patch \"{filename}\" by {author}");
                        choice = ConsoleUI.ShowSelection(new string[] { "yes", "choose file" });
                    }
                    Patcher.PatchFromFile(pathLocation, true);
                }
                Console.WriteLine("patcher finished. press key to exit.");
                Console.ReadKey();
            } else
            {
                Console.WriteLine("pat3");
                switch (args[0])
                {
                    case "-f":
                        if (args.Length == 1) {
                            Console.WriteLine("no file specified.");
                            return;
                        }
                        if (File.Exists(Path.GetFullPath(args[1])))
                        {
                            Console.WriteLine(args[1] + " found, using file.");
                            Console.WriteLine("");
                            Patcher.PatchFromFile(args[1], true);
                        }
                        else
                        {
                            Console.WriteLine(args[1] + " was not found.");
                        }
                        break;
                    default:
                        Console.WriteLine("usage: patcher3 -f file...");
                        Console.WriteLine("files should start with pt3 at the end.");
                        Console.WriteLine("want a feature added or find a bug? add an issue on github:");
                        Console.WriteLine("https://github.com/nesrak1/SubnauticaMods/issues/");
                        break;
                }
            }
        }
    }
}
