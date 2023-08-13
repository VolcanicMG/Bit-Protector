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
    /// Interaction logic for ZIPProtectorView.xaml
    /// </summary>
    public partial class ZIPProtectorView : UserControl
    {
        private ZipProtector zipProtector = new ZipProtector();

        public SaveFileDialog saveFileDialog = new SaveFileDialog();
        public OpenFileDialog openFileDialog = new OpenFileDialog();

        public string userPassword = "";

        public List<NameToPath> files = new List<NameToPath>();

        public ZIPProtectorView()
        {
            InitializeComponent();

            openFileDialog.Filter = "ZIP files (*.zip)|*.zip|All files (*.*)|*.*";
            saveFileDialog.Filter = "ZIP files (*.zip)|*.zip|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
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
            else if (listViewItems.Items.Count <= 0) MessageBox.Show("No items to encrypt...", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            else
            {

                foreach (var file in files)
                {
                    zipProtector.ProtectZIPWithPassword(file.path, passwordTextBox.Password);
                }

                listViewItems.Items.Clear();
                files.Clear();
            }
        }


        private void dragAndDropPanel_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            bool nonPDF = false;

            //Open the dialog
            openFileDialog.FileName = "";
            if (openFileDialog.ShowDialog() != true)
                return;

            //Loop through the selected zipProtector.files and make sure everything is validated
            foreach (var file in openFileDialog.FileNames)
            {
                if (files.Any(item => item.name == file))
                {
                    MessageBox.Show("One or more of the zip files uploaded have already be added. Skipping...", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    continue;
                }

                //Check for valid file
                if (!zipProtector.ZIPValidater(file))
                {
                    Console.WriteLine("Error: This isnt a valid .zip file");
                    MessageBox.Show("Error: This ZIP file is corrupt or empty", "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                if (System.IO.Path.GetExtension(file).Equals(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    CheckBox newCheckBox = new CheckBox();
                    newCheckBox.Content = MainWindow.GetFileName(file);
                    newCheckBox.ClickMode = ClickMode.Press;

                    listViewItems.Items.Add(newCheckBox);
                    files.Add(new NameToPath(MainWindow.GetFileName(file), file));
                }
                else nonPDF = true;
            }

            //If we got one or more non PDF file then tell the user
            if (nonPDF) MessageBox.Show("Error: not a ZIP file!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }


        private void dragAndDropPanel_DragDrop(object sender, DragEventArgs e)
        {
            bool nonPDF = false;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] droppedzipProtector = (string[])e.Data.GetData(DataFormats.FileDrop);

                //Loop through the selected zipProtector.files and make sure everything is validated
                foreach (string file in droppedzipProtector)
                {
                    if (files.Any(item => item.name == file))
                    {
                        MessageBox.Show("One or more of the files uploaded have already be added. Skipping...", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        continue;
                    }

                    //Check for valid file
                    if (!zipProtector.ZIPValidater(file))
                    {
                        Console.WriteLine("Error: This ZIP file is corrupt or empty");
                        MessageBox.Show("Error: This ZIP file is corrupt or empty", "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
                        continue;
                    }

                    if (System.IO.Path.GetExtension(file).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        CheckBox newCheckBox = new CheckBox();
                        newCheckBox.Content = MainWindow.GetFileName(file);
                        newCheckBox.ClickMode = ClickMode.Press;

                        listViewItems.Items.Add(newCheckBox);
                        files.Add(new NameToPath(MainWindow.GetFileName(file), file));

                        //Resets the text
                        dragAndDropLabel.Text = "Drag and drop files here";
                    }
                    else nonPDF = true;
                }

                //If we got one or more non PDF file then tell the user
                if (nonPDF) MessageBox.Show("Error not a ZIP!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void deleteSelectedBttn_Click(object sender, RoutedEventArgs e)
        {
            listViewItems.Items.Clear();
        }


        private void Rectangle_DragLeave(object sender, DragEventArgs e)
        {
            dragAndDropLabel.Text = "Drag and drop files here";
        }
    }
}
