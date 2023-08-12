using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using iText.Kernel.Exceptions;
using System.IO;
using iText.Kernel.Pdf;
using Microsoft.Win32;


namespace BitProtector.EncryptHelpers
{
    internal class PDFProtector
    {
        public SaveFileDialog saveFileDialog;
        public OpenFileDialog openFileDialog;

        public string userPassword = "";

        public List<NameToPath> files = new List<NameToPath>();

        internal ListBox listViewItems;
        internal CheckBox strongerEncrptionCheckBox;


        public void Setup()
        {
            //Dialog boxes
            saveFileDialog = new SaveFileDialog();
            openFileDialog = new OpenFileDialog();

            saveFileDialog.Filter = "PDF Files|*.pdf|All Files|*.*";
        }


        public void SetListView(ListBox listView)
        {
            listViewItems = listView;
        }


        public void SetEncryptionCheckbox(CheckBox checkBox)
        {
            strongerEncrptionCheckBox = checkBox;
        }


        public void DeleteSelected()
        {
            if (listViewItems.Items.Count <= 0)
            {
                MessageBox.Show("No items were selected!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            //Remove the checkboxes
            List<CheckBox> checkBoxesToRemove = new List<CheckBox>();

            foreach (CheckBox checkBox in listViewItems.Items)
            {
                if (checkBox.IsChecked == true)
                {
                    checkBoxesToRemove.Add(checkBox);
                }
            }

            foreach (CheckBox checkBox in checkBoxesToRemove)
            {
                listViewItems.Items.Remove(checkBox);
                files.Remove(files.FirstOrDefault(item => item.name == checkBox.Content.ToString()));
            }
        }


        /// <summary>
        /// Validates the pdf
        /// </summary>
        /// <param name="pdfFilePath"></param>
        /// <returns></returns>
        public static bool ValidatePDF(string pdfFilePath)
        {
            try
            {
                using (FileStream fs = new FileStream(pdfFilePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] headerBytes = new byte[5];
                    int bytesRead = fs.Read(headerBytes, 0, headerBytes.Length);

                    //Check for the PDF signature
                    if (bytesRead >= 5)
                    {
                        string header = System.Text.Encoding.ASCII.GetString(headerBytes);
                        return header.StartsWith("%PDF-");
                    }

                    return false;
                }
            }
            catch (PdfException ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                MessageBox.Show("Error: " + ex.Message, "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }


        private void ResetList()
        {
            //Remove encrypted files from the list
            for (int i = 0; i < listViewItems.Items.Count; i++)
            {
                listViewItems.Items.Clear();
            }
        }


        /// <summary>
        /// Encryption
        /// </summary>
        /// <param name="inputFilePath"></param>
        public void StartPDFEncrypt(string inputFilePath)
        {
            try
            {
                // Show the save dialog box
                saveFileDialog.Title = "Select a PDF File Name and Location";
                saveFileDialog.FileName = $"{MainWindow.GetFileName(inputFilePath)}_PROTECTED";

                //Show the save dialog
                if (saveFileDialog.ShowDialog() != true)
                    return;

                string outputFilePath = saveFileDialog.FileName;
                var password = Encoding.ASCII.GetBytes(userPassword);

                PdfReader reader = new PdfReader(inputFilePath);
                WriterProperties props = null;

                //Check for stronger encryption
                if ((bool)strongerEncrptionCheckBox.IsChecked)
                {
                    props = new WriterProperties()
                    .SetStandardEncryption(password, password, EncryptionConstants.ALLOW_PRINTING,
                        EncryptionConstants.ENCRYPTION_AES_256 | EncryptionConstants.DO_NOT_ENCRYPT_METADATA);
                }
                else
                {
                    props = new WriterProperties()
                    .SetStandardEncryption(password, password, EncryptionConstants.ALLOW_PRINTING,
                        EncryptionConstants.ENCRYPTION_AES_128 | EncryptionConstants.DO_NOT_ENCRYPT_METADATA);
                }

                PdfWriter writer = new PdfWriter(saveFileDialog.FileName, props);
                PdfDocument pdfDoc = new PdfDocument(reader, writer);
                pdfDoc.Close();

                MessageBox.Show("PDF encrypted successfully.", "Encryption Success", MessageBoxButton.OK, MessageBoxImage.Exclamation);

                ResetList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
