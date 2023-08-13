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
        public void ProtectZIPWithPassword(string path, string password)
        {
            using (ZipFile zip = new ZipFile())
            {
                zip.Password = password;
                zip.AddFile(path, "");

                // Save the password-protected ZIP file
                string outputPath = GetSavePath(path);
                if (!outputPath.Equals("")) zip.Save(outputPath);
            }

            Console.WriteLine("Zip file created and entries password protected.");
            MessageBox.Show("ZIP file has been encrypted", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        /// <summary>
        /// Gets the save path for a .zip file
        /// </summary>
        /// <param name="originalFilePath"></param>
        /// <returns></returns>
        private string GetSavePath(string originalFilePath)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "ZIP Files|*.zip";
            saveFileDialog.FileName = Path.GetFileNameWithoutExtension(originalFilePath) + "_encrypted";

            return saveFileDialog.ShowDialog() == true ? saveFileDialog.FileName : "";
        }


        /// <summary>
        /// Validates the file a .zip
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool ZIPValidater(string path)
        {
            if (File.Exists(path))
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    byte[] header = new byte[4];
                    fs.Read(header, 0, 4);

                    // Check if the first four bytes match the .zip file signature "PK\x03\x04"
                    if (header[0] == 0x50 && header[1] == 0x4B && header[2] == 0x03 && header[3] == 0x04)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
