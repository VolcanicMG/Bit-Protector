using BitProtector.EncryptHelpers;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BitProtector.MVM.View
{
    /// <summary>
    /// Interaction logic for PDFProtectorView.xaml
    /// </summary>
    public partial class PDFProtectorView : UserControl
    {
        private PDFProtector pdfProtector = new PDFProtector();

        public PDFProtectorView()
        {
            InitializeComponent();

            //Setup
            pdfProtector.Setup();
            pdfProtector.SetListView(listViewItems);
            pdfProtector.SetEncryptionCheckbox(strongerEncrptionCheckBox);
        }


        private void dragAndDropPanel_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
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
                pdfProtector.userPassword = passwordTextBox.Password;

                foreach (var file in pdfProtector.files)
                {
                    pdfProtector.StartPDFEncrypt(file.path);
                }
            }
        }


        private void dragAndDropPanel_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            bool nonPDF = false;

            //Open the dialog
            pdfProtector.openFileDialog.FileName = "";
            if (pdfProtector.openFileDialog.ShowDialog() != true)
                return;

            //Loop through the selected pdfProtector.files and make sure everything is validated
            foreach (var file in pdfProtector.openFileDialog.FileNames)
            {
                if (pdfProtector.files.Any(item => item.name == file))
                {
                    MessageBox.Show("One or more of the pdfProtector.files uploaded have already be added. Skipping...", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    continue;
                }

                //Check for valid file
                if (!PDFProtector.ValidatePDF(file))
                {
                    Console.WriteLine("Error: This PDF file is corrupt or doesn't have a header.");
                    MessageBox.Show("Error: This PDF file is corrupt or doesn't have a header.", "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                if (System.IO.Path.GetExtension(file).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    CheckBox newCheckBox = new CheckBox();
                    newCheckBox.Content = MainWindow.GetFileName(file);
                    newCheckBox.ClickMode = ClickMode.Press;

                    listViewItems.Items.Add(newCheckBox);
                    pdfProtector.files.Add(new NameToPath(MainWindow.GetFileName(file), file));
                }
                else nonPDF = true;
            }

            //If we got one or more non PDF file then tell the user
            if (nonPDF) MessageBox.Show("Error not a pdf!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }


        private void dragAndDropPanel_DragDrop(object sender, DragEventArgs e)
        {
            bool nonPDF = false;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] droppedpdfProtector = (string[])e.Data.GetData(DataFormats.FileDrop);

                //Loop through the selected pdfProtector.files and make sure everything is validated
                foreach (string file in droppedpdfProtector)
                {
                    if (pdfProtector.files.Any(item => item.name == file))
                    {
                        MessageBox.Show("One or more of the pdfProtector.files uploaded have already be added. Skipping...", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        continue;
                    }

                    //Check for valid file
                    if (!PDFProtector.ValidatePDF(file))
                    {
                        Console.WriteLine("Error: This PDF file is corrupt or doesn't have a header.");
                        MessageBox.Show("Error: This PDF file is corrupt or doesn't have a header.", "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
                        continue;
                    }

                    if (System.IO.Path.GetExtension(file).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        CheckBox newCheckBox = new CheckBox();
                        newCheckBox.Content = MainWindow.GetFileName(file);
                        newCheckBox.ClickMode = ClickMode.Press;

                        listViewItems.Items.Add(newCheckBox);
                        pdfProtector.files.Add(new NameToPath(MainWindow.GetFileName(file), file));
                    }
                    else nonPDF = true;
                }

                //If we got one or more non PDF file then tell the user
                if (nonPDF) MessageBox.Show("Error not a pdf!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void deleteSelectedBttn_Click(object sender, RoutedEventArgs e)
        {
            pdfProtector.DeleteSelected();
        }
    }
}
