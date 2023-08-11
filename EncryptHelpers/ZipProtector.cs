using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Ionic.Zip;
using Microsoft.Win32;
using System.Windows;

namespace BitProtector.EncryptHelpers
{
    internal class ZipProtector
    {
        public static void ProtectZIPWithPassword(string path, string password)
        {
            using (ZipFile zip = new ZipFile())
            {
                zip.Password = password;

                // Add files from the source directory
                zip.AddDirectory(path);

                // Set passwords for individual entries (files)
                foreach (var entry in zip.Entries)
                {
                    entry.Password = password;
                }

                // Save the password-protected ZIP file
                string outputPath = GetSavePath(path);
                if (!outputPath.Equals("")) zip.Save(outputPath);
            }

            Console.WriteLine("Zip file created and entries password protected using DotNetZip.");
        }


        private static string GetSavePath(string originalFilePath)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "ZIP Files|*.zip";
            saveFileDialog.FileName = Path.GetFileNameWithoutExtension(originalFilePath) + "_encrypted";

            return saveFileDialog.ShowDialog() == true ? saveFileDialog.FileName : "";
        }
    }
}
