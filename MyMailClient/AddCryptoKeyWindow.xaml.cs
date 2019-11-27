using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
    /// Логика взаимодействия для AddCryptoKeyWindow.xaml
    /// </summary>
    public partial class AddCryptoKeyWindow : Window
    {
        public AddCryptoKeyWindow()
        {
            InitializeComponent();
            cmbx_owner.ItemsSource = CurrentData.curAcc.MlBxs;
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void btn_Ok_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = txt_name.Text.Trim();
                if (name.Length == 0 || cmbx_owner.SelectedItem == null)
                {
                    throw new Exception("Заполните все поля");
                }

                if (rdbtn_encryption.IsChecked.Value)
                {
                    RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                    CurrentData.curKey = new CryptoKey(rsa, name, (cmbx_owner.SelectedItem as MailBox).Address);
                }
                else
                {
                    DSACryptoServiceProvider dsa = new DSACryptoServiceProvider();
                    CurrentData.curKey = new CryptoKey(dsa, name, (cmbx_owner.SelectedItem as MailBox).Address);
                }

                DialogResult = true;
            }
            catch (Exception ex)
            {
                Utility.MsgBox(ex.Message, "Ошибка", this);
            }
        }
    }
}
