using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
using UnityEngine;
#endif

namespace Automata.Debugging
{
    public static class Logger
    {
        private static string filePath = "";

        public static void SetFile(string path)
        {
            // set the file path to the chosen path
            filePath = path;
        }

        public static void CreateLog()
        {
            string path = Path.GetDirectoryName(filePath);

            // create the directory if it doesn't exist
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // create a log if it doesn't exist
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
            }
        }

        public static void Log(string text)
        {
            // export the logged text
            Export(text);
        }

        public static void LogInfo(string text)
        {
            // Dump the logged text
            Export("INFO: " + text);

            // show the logged text in the unity editor
#if UNITY_EDITOR
        Debug.Log(text);
#endif
        }

        public static void LogWarning(string text)
        {
            // Dump the logged text
            Export("WARNING: " + text);

            // show the logged text in the unity editor
#if UNITY_EDITOR
        Debug.LogWarning(text);
#endif
        }

        public static void LogError(string text)
        {
            // Dump the logged text
            Export("ERROR: " + text);

            // show the logged text in the unity editor
#if UNITY_EDITOR
        Debug.LogError(text);
#endif
        }

        private static void Export(string text)
        {
            // check if the file exists
            if (File.Exists(filePath))
            {
                // append to last line of file and write text
                StreamWriter streamWriter = new StreamWriter(filePath, true);
                streamWriter.WriteLine(text);
                streamWriter.Close();
            }
        }

        public static string[] Import(string filePath)
        {
            List<string> log = new List<string>();

            // check if the file exists
            if (File.Exists(filePath))
            {
                StreamReader streamReader = new StreamReader(filePath);

                // read file and store readed lines
                while (streamReader.Peek() >= 0)
                {
                    log.Add(streamReader.ReadLine());
                }

                streamReader.Close();
            }

            return log.ToArray();
        }
    }
}