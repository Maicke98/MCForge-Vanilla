﻿

using System.IO;
using System.Net;
using MCForge.Utils;

namespace MCForge.Utils {

    //TODO: add xml comments

    public class FileUtils {

        public const string PropertiesPath = "properties/";
        public const string LevelsPath = "levels/";
        public const string DllsPath = "dlls/";
        public const string ExtrasPath = "extras/";
        public const string TextPath = "text/";


        /// <summary>
        /// Downloads  a file from the specifed website
        /// </summary>
        /// <param name="url">File address</param>
        /// <param name="saveLocation">Location to save the file</param>
        public static void CreateFileFromWeb(string url, string saveLocation) {
            using (var client = new WebClient())
                client.DownloadFile(url, saveLocation);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="saveLocation"></param>
        public static void CreateFileFromBytes(byte[] bytes, string saveLocation) {
            using (var stuff = File.Create(saveLocation))
                stuff.Write(bytes, 0, bytes.Length);
        }


        /// <summary>
        /// Creates a directory if it doesn't exist, will log results
        /// </summary>
        public static void CreateDirIfNotExist(string directory) {
            if (Directory.Exists(directory))
                return;

            Directory.CreateDirectory(directory);
            Logger.Log(string.Format("[Directory] Created \"{0}\"", directory));
        }

        /// <summary>
        /// Creats a file if it doesnt already exist, logs results.
        /// </summary>
        /// <param name="fileLoc"></param>
        public static void CreateFileIfNotExist(string fileLoc, string contents = null) {
            if (File.Exists(fileLoc))
                return;

            if (contents == null)
                File.Create(fileLoc).Close();
            else
                using (var filer = File.CreateText(fileLoc))
                    filer.Write(contents);

            Logger.Log(string.Format("[File] \"{0}\" was created", fileLoc));
        }
    }
}