using System;

namespace VRC
{
    public static class Input
    {
        public static string GetInput(string prompt = "", bool isSecret = false)
        {
            if (string.IsNullOrEmpty(prompt)) prompt = "> ";
            Console.Write(prompt);
            if (isSecret)
            {
                string input = "";
                do
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                    {
                        input += key.KeyChar;
                        Console.Write("*");
                    }
                    else
                    {
                        if (key.Key == ConsoleKey.Backspace && input.Length > 0)
                        {
                            input = input.Substring(0, input.Length - 1);
                            Console.Write("\b \b");
                        }
                        else if (key.Key == ConsoleKey.Enter)
                        {
                            break;
                        }
                    }
                } while (true);
                return input;
            }
            return Console.ReadLine();
        }
    }
}
