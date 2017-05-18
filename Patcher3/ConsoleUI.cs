using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Patcher3
{
    public class ConsoleUI
    {
        public static int ShowSelection(string[] options, int defaultChoice = 0)
        {
            int choice = defaultChoice;
            for (int i = 0; i < options.Length; i++)
            {
                if (i == choice)
                {
                    Console.Write("[" + options[i] + "] ");
                }
                else
                {
                    Console.Write(" " + options[i] + "  ");
                }
            }
            while (true)
            {
                ConsoleKeyInfo cki = Console.ReadKey();
                if (cki.Key == ConsoleKey.LeftArrow)
                {
                    if (choice < 1)
                    {
                        choice = options.Length - 1;
                    } else
                    {
                        choice--;
                    }
                } else if (cki.Key == ConsoleKey.RightArrow)
                {
                    if (choice > options.Length - 2)
                    {
                        choice = 0;
                    }
                    else
                    {
                        choice++;
                    }
                } else if (cki.Key == ConsoleKey.Enter)
                {
                    return choice;
                }
                Console.SetCursorPosition(0, Console.CursorTop);
                for (int i = 0; i < options.Length; i++)
                {
                    if (i == choice)
                    {
                        Console.Write("[" + options[i] + "] ");
                    }
                    else
                    {
                        Console.Write(" " + options[i] + "  ");
                    }
                }
            }
        }

        public static string ShowTextfield(int length)
        {
            Console.WriteLine(" " + new string('_', length - 2));
            Console.WriteLine("|" + new string(' ', length - 2) + "|");
            Console.WriteLine("|" + new string('_', length - 2) + "|");
            Console.SetCursorPosition(1, Console.CursorTop - 2);
            return Console.ReadLine();
        }

        public static void ClearTextfield(int length)
        {
            int cursorPos = Console.CursorTop;
            Console.SetCursorPosition(0, cursorPos - 2);
            Console.Write(new string(' ', length));
            Console.SetCursorPosition(0, cursorPos - 1);
            Console.Write(new string(' ', length));
            Console.SetCursorPosition(0, cursorPos);
            Console.Write(new string(' ', length));
            Console.SetCursorPosition(0, cursorPos - 3);
        }
    }
}
