using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Word;
using Microsoft.Win32;

namespace BitProtector
{
    internal class OfficeFileProtector
    {
        public static void ProtectFileWithPassword(string filePath, string password)
        {
            string extension = System.IO.Path.GetExtension(filePath).ToLower();

            switch (extension)
            {
                case ".docx":
                    ProtectWordDocument(filePath, password);
                    break;
                case ".xlsx":
                    ProtectExcelWorkbook(filePath, password);
                    break;
                default:
                    throw new ArgumentException("Unsupported file type.");
            }
        }

        private static void ProtectWordDocument(string filePath, string password)
        {
            Microsoft.Office.Interop.Word.Application wordApp = new Microsoft.Office.Interop.Word.Application();
            Document doc = wordApp.Documents.Open(filePath);

            doc.Password = password;

            string savePath = GetSavePath(filePath);
            doc.SaveAs(System.IO.Path.Combine(savePath, System.IO.Path.ChangeExtension(System.IO.Path.GetFileName(filePath), "_encrypted.docx")));
            doc.Close();
            wordApp.Quit();
        }

        private static void ProtectExcelWorkbook(string filePath, string password)
        {
            Microsoft.Office.Interop.Excel.Application excelApp = new Microsoft.Office.Interop.Excel.Application();
            Workbook workbook = excelApp.Workbooks.Open(filePath);

            workbook.Password = password;

            string savePath = GetSavePath(filePath);
            workbook.SaveAs(System.IO.Path.Combine(savePath, System.IO.Path.ChangeExtension(System.IO.Path.GetFileName(filePath), "_encrypted.xlsx")));
            workbook.Close();
            excelApp.Quit();
        }

        private static string GetSavePath(string originalFilePath)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Protected Files|*.docx;*.xlsx";
            saveFileDialog.FileName = System.IO.Path.GetFileNameWithoutExtension(originalFilePath) + "_encrypted";
            if (saveFileDialog.ShowDialog() == true)
            {
                return System.IO.Path.GetDirectoryName(saveFileDialog.FileName);
            }
            else
            {
                throw new OperationCanceledException("Save operation canceled.");
            }
        }
    }
}
