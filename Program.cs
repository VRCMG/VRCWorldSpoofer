using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace VRC
{
    public static class Program
    {
        static Program()
        {
            // Load embedded resources
            AppDomain.CurrentDomain.AssemblyResolve += (sender, e) =>
            {
                string resourceName = new AssemblyName(e.Name).Name + ".dll";
                string resource = Array.Find(typeof(Program).Assembly.GetManifestResourceNames(), element => element.EndsWith(resourceName));

                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
                {
                    byte[] assemblyData = new byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    return Assembly.Load(assemblyData);
                }
            };
        }

        public static void Main(string[] args)
        {
            Utils.SetConsoleTitle();
            if (Utils.CheckProcess("vrchat"))
            {
                new Spoofer().Start();
            }
            Thread.Sleep(-1); // Keep console open
        }
    }
}