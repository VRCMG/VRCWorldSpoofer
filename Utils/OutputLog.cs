using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace VRC
{
    public class OutputLog
    {
        public DirectoryInfo Directory
        {
            get
            {
                DirectoryInfo directory = new DirectoryInfo($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/../LocalLow/VRChat/VRChat");
                if (directory.Exists) return directory;
                return null;
            }
        }

        public FileInfo File
        {
            get
            {
                if (Directory == null) return null;
                FileInfo[] files = Directory.GetFiles().OrderByDescending(f => f.LastWriteTime).ToArray();
                if (files.Count() == 0) return null;
                return files.FirstOrDefault(f => f.Name.ToLower().StartsWith("output_log_"));
            }
        }

        public void StartWatching()
        {
            if (File == null) return;
            bool firstRun = true;
            long initialFileSize = File.Length;
            long lastReadLength = initialFileSize - 1024;
            if (lastReadLength < 0) lastReadLength = 0;

            new Thread(() =>
            {
                while (true)
                {
                    long fileSize = new FileInfo(File.FullName).Length;
                    if (fileSize > lastReadLength)
                    {
                        using (FileStream fs = new FileStream(File.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            fs.Seek(lastReadLength, SeekOrigin.Begin);
                            byte[] buffer = new byte[1024];

                            while (true)
                            {
                                int bytesRead = fs.Read(buffer, 0, buffer.Length);

                                if (firstRun)
                                {
                                    lastReadLength = fileSize;
                                    bytesRead = 0;
                                }
                                else
                                {
                                    lastReadLength += bytesRead;
                                }

                                if (bytesRead == 0) break;

                                string text = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                                TextWritten.Invoke(this, text);
                            }
                        }
                    }
                    firstRun = false;
                    Thread.Sleep(100);
                }
            })
            {
                IsBackground = true
            }.Start();
        }

        public event TextWrittenHandler TextWritten;
        public delegate void TextWrittenHandler(object sender, string data);
    }
}