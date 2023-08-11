using iText.Kernel.Exceptions;
using iText.Kernel.Pdf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BitProtector
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        SaveFileDialog saveFileDialog;
        OpenFileDialog openFileDialog;

        private string userPassword = "";

        private List<NameToPath> files = new List<NameToPath>();

        public MainWindow()
        {
            InitializeComponent();

            //Set the checkbox tooltip

            //Dialog boxes
            saveFileDialog = new SaveFileDialog();
            openFileDialog = new OpenFileDialog();

            saveFileDialog.Filter = "PDF Files|*.pdf|All Files|*.*";
        }


        /// <summary>
        /// A helper to simplify the creation of tooltips
        /// </summary>
        /// <param name="tip"></param>
        /// <returns></returns>
        private ToolTip CreateTooltip(string tip)
        {
            //Create the tooltip
            ToolTip toolTip = new ToolTip();
            toolTip.Content = tip;

            return toolTip;
        }


        /// <summary>
        /// Helper method to find a child of a specific type within a Visual
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent"></param>
        /// <returns></returns>
        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                {
                    return typedChild;
                }
                T childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                {
                    return childOfChild;
                }
            }
            return null;
        }


        /// <summary>
        /// Gets the file path and returns the name of the file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetFileName(string filePath, bool removeExtension = true)
        {
            FileInfo fileInfo = new FileInfo(filePath);

            if (removeExtension)
            {
                // Remove the ".pdf" extension
                string filePathWithoutExtension = System.IO.Path.ChangeExtension(filePath, null);

                return $"{GetFileName(filePathWithoutExtension, false)}";
            }

            return $"{fileInfo.Name}";
        }


        /// <summary>
        /// Validates the pdf
        /// </summary>
        /// <param name="pdfFilePath"></param>
        /// <returns></returns>
        private bool ValidatePDF(string pdfFilePath)
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


        /// <summary>
        /// Encryption
        /// </summary>
        /// <param name="inputFilePath"></param>
        public void StartEncrypt(string inputFilePath)
        {
            try
            {
                // Show the save dialog box
                saveFileDialog.Title = "Select a PDF File Name and Location";
                saveFileDialog.FileName = $"{GetFileName(inputFilePath)}_PROTECTED";

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

                ResetForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void ResetForm()
        {
            //Remove encrypted files from the list
            for (int i = 0; i < listViewItems.Items.Count; i++)
            {
                listViewItems.Items.Clear();
            }
        }


        private void dragAndDropPanel_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }


        private void dragAndDropPanel_DragDrop(object sender, DragEventArgs e)
        {
            bool nonPDF = false;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] droppedFiles = (string[])e.Data.GetData(DataFormats.FileDrop);

                //Loop through the selected files and make sure everything is validated
                foreach (string file in droppedFiles)
                {
                    if (files.Any(item => item.name == file))
                    {
                        MessageBox.Show("One or more of the files uploaded have already be added. Skipping...", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        continue;
                    }

                    //Check for valid file
                    if (!ValidatePDF(file))
                    {
                        Console.WriteLine("Error: This PDF file is corrupt or doesn't have a header.");
                        MessageBox.Show("Error: This PDF file is corrupt or doesn't have a header.", "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
                        continue;
                    }

                    if (System.IO.Path.GetExtension(file).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        CheckBox newCheckBox = new CheckBox();
                        newCheckBox.Content = GetFileName(file);
                        newCheckBox.ClickMode = ClickMode.Press;

                        listViewItems.Items.Add(newCheckBox);
                        files.Add(new NameToPath(GetFileName(file), file));
                    }
                    else nonPDF = true;
                }

                //If we got one or more non PDF file then tell the user
                if (nonPDF) MessageBox.Show("Error not a pdf!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void dragAndDropPanel_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            bool nonPDF = false;

            //Open the dialog
            openFileDialog.FileName = "";
            if (openFileDialog.ShowDialog() != true)
                return;

            //Loop through the selected files and make sure everything is validated
            foreach (var file in openFileDialog.FileNames)
            {
                if (files.Any(item => item.name == file))
                {
                    MessageBox.Show("One or more of the files uploaded have already be added. Skipping...", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    continue;
                }

                //Check for valid file
                if (!ValidatePDF(file))
                {
                    Console.WriteLine("Error: This PDF file is corrupt or doesn't have a header.");
                    MessageBox.Show("Error: This PDF file is corrupt or doesn't have a header.", "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                if (System.IO.Path.GetExtension(file).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    CheckBox newCheckBox = new CheckBox();
                    newCheckBox.Content = GetFileName(file);
                    newCheckBox.ClickMode = ClickMode.Press;

                    listViewItems.Items.Add(newCheckBox); 
                    files.Add(new NameToPath(GetFileName(file), file));
                }
                else nonPDF = true;
            }

            //If we got one or more non PDF file then tell the user
            if (nonPDF) MessageBox.Show("Error not a pdf!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }


        private void deleteSelected_Click(object sender, RoutedEventArgs e)
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

        private void encryptBttn_Click(object sender, RoutedEventArgs e)
        {
            //Check to make sure we are not using blanks
            if (passwordTextBox.Password.Equals(""))
            {
                MessageBox.Show("Password is blank!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                //Set the user password
                userPassword = passwordTextBox.Password;

                foreach (var file in files)
                {
                    StartEncrypt(file.path);
                }
            }
        }
    }


    /// <summary>
    /// Used to combine and link names to paths
    /// </summary>
    public class NameToPath
    {
        public string name;
        public string path;

        public NameToPath(string name, string path)
        {
            this.name = name;
            this.path = path;
        }
    }

}

