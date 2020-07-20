using Microsoft.JScript;
using System;
using System.Diagnostics;
using System.Text;

namespace VRC
{
    public static class Utils
    {
        static Utils()
        {
            Random = new Random();
        }

        public static void SetConsoleTitle(string title = "")
        {
            Console.Title = $"VRChat World Spoofer by Moons • {title}";
        }

        public static bool CheckProcess(string process)
        {
            Process[] p = Process.GetProcessesByName(process);
            if (p.Length == 0)
            {
                SetConsoleTitle("VRChat not running");
                Error("VRChat is not currently running.");
                return false;
            }
            else if (p.Length > 1)
            {
                SetConsoleTitle("Too many instances of VRChat running");
                Error("There is more than 1 instance of VRChat running.");
                return false;
            }
            return true;
        }

        public static void WriteLine(string text = "", ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void Log(string text)
        {
            Console.Write(Timestamp);
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write(" [LOG] ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(text);
        }

        public static void Warn(string text)
        {
            Console.Write(Timestamp);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(" [WARN] ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(text);
        }

        public static void Error(string text)
        {
            Console.Write(Timestamp);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(" [ERROR] ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(text);
        }

        public static string GenerateNonce()
        {
            string nonce = "";
            for (int i = 0; i < 8; i++)
            {
                nonce += Random.Next(0, int.MaxValue).ToString("X8");
            }
            return nonce;
        }

        public static string Base64Encode(string text)
        {
            return System.Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
        }

        public static string Base64Decode(string b64)
        {
            return Encoding.UTF8.GetString(System.Convert.FromBase64String(b64));
        }

        public static string Escape(string text)
        {
            return GlobalObject.escape(text);
        }

        public static string Timestamp
        {
            get
            {
                return $"[{DateTime.Now:HH:mm:ss}]";
            }
        }

        public static Random Random { get; }
    }
}