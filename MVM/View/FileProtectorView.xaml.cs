using BitProtector.EncryptHelpers;
using Microsoft.Win32;
using System;
using System.Linq;
using System.Security;
using System.Windows;
using System.Windows.Controls;

namespace BitProtector.MVM.View
{
    /// <summary>
    /// Interaction logic for FileProtectorView.xaml
    /// </summary>
    public partial class FileProtectorView : UserControl
    {
        public SaveFileDialog saveFileDialog = new SaveFileDialog();
        public OpenFileDialog openFileDialog = new OpenFileDialog();

        public string userPassword = "";

        private string filePath = "";
        private string outputPath = "";

        public FileProtectorView()
        {
            InitializeComponent();
        }


        private void dragAndDropPanel_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;

            dragAndDropLabel.Text = "Upload";
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
                //Encrypt
                FileProtector.EncryptFile(filePath, filePath, passwordTextBox.SecurePassword);
            }
        }


        private void dragAndDropPanel_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //Open the dialog
            openFileDialog.FileName = "";
            if (openFileDialog.ShowDialog() != true)
                return;

            fileNameLabel.Content = openFileDialog.FileName;
            filePath = openFileDialog.FileName;
        }


        private void dragAndDropPanel_DragDrop(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] droppedzipProtector = (string[])e.Data.GetData(DataFormats.FileDrop);

                
            }
        }

        private void decryptBttn_Click(object sender, RoutedEventArgs e)
        {
            filePath = openFileDialog.FileName;
            outputPath = System.IO.Path.ChangeExtension(filePath, "");

            FileProtector.DecryptFile(filePath, outputPath, passwordTextBox.SecurePassword);
        }

        private void Rectangle_DragLeave(object sender, DragEventArgs e)
        {
            dragAndDropLabel.Text = "Drag and drop files here";
        }
    }
}

