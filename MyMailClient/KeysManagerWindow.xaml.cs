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
using System.Windows.Shapes;

namespace MyMailClient
{
    /// <summary>
    /// Логика взаимодействия для KeysManagerWindow.xaml
    /// </summary>
    public partial class KeysManagerWindow : Window
    {
        private List<CryptoKey> filteredKeys;
        public KeysManagerWindow()
        {
            InitializeComponent();
            this.filteredKeys = new List<CryptoKey>();
            FilterKeys();
        }

        private void btn_add_Click(object sender, RoutedEventArgs e)
        {
            AddCryptoKeyWindow ckw = new AddCryptoKeyWindow();
            if (ckw.ShowDialog().Value)
            {
                if (CurrentData.curAcc.AddKey())
                {
                    FilterKeys();
                    CurrentData.curAcc.Srlz();
                }
                else
                {
                    Utility.MsgBox("Такой ключ уже есть в библиотеке ключей", "Ошибка", this);
                }
            }
        }

        private void FilterKeys()
        {
            filteredKeys.Clear();
            lb_keys.ItemsSource = null;
            foreach (CryptoKey key in CurrentData.curAcc.Keys)
                if (txt_filter.Text.Length == 0 ||
                        key.Name.IndexOf(txt_filter.Text, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        key.OwnerAddress.IndexOf(txt_filter.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                    filteredKeys.Add(key);


            lb_keys.ItemsSource = filteredKeys;
        }

        private void txt_filter_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterKeys();
        }
        private void btn_remove_Click(object sender, RoutedEventArgs e)
        {
            CryptoKey key = lb_keys.SelectedItem as CryptoKey;
            if (key != null)
            {
                CurrentData.curAcc.Keys.Remove(key);
                FilterKeys();
                CurrentData.curAcc.Srlz();
            }
        }

        private void btn_sendKey_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentData.curMail != null)
            {
                CryptoKey key = lb_keys.SelectedItem as CryptoKey;
                if (key != null)
                {
                    WriteLetterWindow wlw = new WriteLetterWindow(key.GetPublicCryptoKey());

                    wlw.Show();
                }
                else
                {
                    Utility.MsgBox("Выберите ключ для отправки!", "Ошибка", this);
                }
            }
            else
            {
                Utility.MsgBox("Для отправки открытого ключа, вернитесь на главный экран и выберите почтовый ящик", "Ошибка", this);
            }
        }
    }
}
