using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

namespace MyMailClient
{
    /// <summary>
    /// Логика взаимодействия для WriteLetterWindow.xaml
    /// </summary>
    public partial class WriteLetterWindow : Window
    {
        public WriteLetterWindow()
        {
            InitializeComponent();
        }

        private void btn_send_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_attach_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Прикрепить файл...";
            ofd.Multiselect = true;
            if (ofd.ShowDialog().Value)
                foreach (string filename in ofd.FileNames)
                    AttachFile(filename);
        }

        public void AttachFile(string fullName)
        {
            FileInfo f = new FileInfo(fullName);
            attachmentsPanel.Items.Add(f);
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void chbx_encrypt_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void chbx_sign_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void cmbx_encryption_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void cmbx_sign_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }


    }
}
