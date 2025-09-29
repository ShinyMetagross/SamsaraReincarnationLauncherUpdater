using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;

namespace SamsaraReincarnationLauncherBootStrapper
{
    class SamsaraReincarnationLauncherBootStrapper
    {
        bool generatedLog = false;
        string logPath = string.Empty;
        static void Main(string[] args)
        {
            //Process main = Process.GetProcessesByName("Samsara_Reincarnation")[0];
            //main.WaitForExit();
            while (Process.GetProcessesByName("Samsara_Reincarnation").Length > 0) Thread.Sleep(100);
            var me = new SamsaraReincarnationLauncherBootStrapper();
            Directory.CreateDirectory("Logs");
            me.logPath = Path.Combine("Logs","update_" + DateTime.Now.ToString().Replace("/", "-").Replace(":", "_") + ".txt");
            //Delete old version files
            for (int i = 1; i < args.Count(); i++)
            {
                string fileName = args[i];
                if (fileName == "SamsaraUpdater.exe") continue;
                me.writeToLog("Checking file: " + fileName);
                if (Path.Exists(fileName))
                {
                    me.writeToLog("Deleting file: " + fileName);
                    try 
                    {
                        FileInfo fileInfo = new FileInfo(fileName);
                        fileInfo.IsReadOnly = false;
                        File.Delete(fileName); 
                    }
                    catch (Exception e) { me.writeToLog(e.Message); }
                }
            }
            me.writeToLog("Beginning extraction of:" + args[0]);
            using (ZipArchive zippy = new ZipArchive(System.IO.File.Open(args[0], FileMode.Open)))
            {
                int currentItemCount = 0;
                foreach (var item in zippy.Entries)
                {
                    if (item.FullName == "SamsaraUpdater.exe") continue;
                    currentItemCount++;
                    bool fileInstalled = true;
                    string itemPath = Path.GetFullPath(item.FullName);

                    //Check if the file exists. If it does, check for the length. Right now I'm not interested in checksums
                    if (System.IO.File.Exists(itemPath))
                    {
                        FileInfo currentItem = new FileInfo(itemPath);
                        if (currentItem.Length != item.Length)
                            fileInstalled = false;
                    }
                    else fileInstalled = false;

                    //If it's a directory, just create the directory. Otherwise, extract the file
                    if (item.FullName.EndsWith("/"))
                        Directory.CreateDirectory(itemPath);
                    else if (!Path.Exists(Path.GetDirectoryName(itemPath)))
                        Directory.CreateDirectory(Path.GetDirectoryName(itemPath));
                    else if (!fileInstalled)
                    {
                        item.ExtractToFile(itemPath, true);
                        me.writeToLog("Extracting: " + itemPath);
                    }
                }
                zippy.Dispose();
            }

            me.writeToLog("Update sucessfully completed!");

            var ps = new ProcessStartInfo("Samsara_Reincarnation.exe")
            {
                UseShellExecute = true,
                Verb = "open"
            };
            Process.Start(ps);
        }

        /// <summary>
        /// Use this method to write to a log
        /// </summary>
        private void writeToLog(string text)
        {
            generateLog(logPath);
            if (Path.Exists(logPath))
            {
                System.Console.WriteLine(text);
                StreamWriter sw = new StreamWriter(logPath, append: true);
                sw.WriteLine(text);
                sw.Close();
            }
        }

        /// <summary>
        /// Generate a log file if it doesn't exist
        /// </summary>
        /// <param name="fileName"></param>
        private void generateLog(string fileName)
        {
            if (!generatedLog)
            {
                System.IO.File.Create(fileName).Close();
                generatedLog = true;
            }
        }
    }
}