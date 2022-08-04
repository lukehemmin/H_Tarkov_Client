/* LogManager.cs
 * License: NCSA Open Source License
 * 
 * Copyright: Merijn Hendriks
 * AUTHORS:
 * Merijn Hendriks
 */


using System;
using System.IO;

namespace Aki.Launcher.Controllers
{
    /// <summary>
    /// LogManager
    /// </summary>
    public class LogManager
    {
        //TODO - update this to use reflection to get the calling method, class, etc 
        private static LogManager _instance;
        public static LogManager Instance => _instance ?? (_instance = new LogManager());
        private string filepath;

        public LogManager()
        {
            filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "user", "logs");
        }

        public void Write(string text)
        {
            if (!Directory.Exists(filepath))
            {
                Directory.CreateDirectory(filepath);
            }

            string filename = Path.Combine(filepath, "launcher.log");
            File.AppendAllLines(filename, new[] { $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]{text}" });
        }

        public void Debug(string text) => Write($"[Debug]{text}");

        public void Info(string text) => Write($"[Info]{text}");

        public void Warning(string text) => Write($"[Warning]{text}");

        public void Error(string text) => Write($"[Error]{text}");

        public void Exception(Exception ex) => Write($"[Exception]{ex.Message}\nStacktrace:\n{ex.StackTrace}");
    }
}
