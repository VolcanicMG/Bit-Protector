using iText.Kernel.Exceptions;
using iText.Kernel.Pdf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BitProtector
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {

        public MainWindow()
        {
            InitializeComponent();
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
        public static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
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


        private void ExitBttn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }


        private void MinimizeBttn_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        //Drag input
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();



        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Start moving the window
            ReleaseCapture();
            SendMessage(new System.Windows.Interop.WindowInteropHelper(this).Handle, 0x112, 0xf012, 0);
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

