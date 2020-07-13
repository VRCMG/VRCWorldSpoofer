using System;
using System.IO;
using System.Reflection;

namespace VRC
{
    public static class Program
    {
        static Program()
        {
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

        public static void Main()
        {
            Start();
            Utils.KeepConsoleOpen();
        }

        private static void Start()
        {
            Utils.SetConsoleTitle();
            if (Utils.CheckProcess("vrchat"))
            {
                new Spoofer().Start();
            }
        }
    }
}