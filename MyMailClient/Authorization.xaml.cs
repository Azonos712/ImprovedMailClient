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
    public partial class Authorization : Window
    {
        public Authorization()
        {
            InitializeComponent();
            Account.CheckAccPath();
        }

        private void Btn_signIn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Validation(false);


                Utility.MsgBox("Вы успешно авторизировались!", "Уведомление", auth);
            }
            catch (Exception ex)
            {
                Utility.MsgBox(ex.Message, "Ошибка", auth);
            }
        }

        void Validation(bool registration)
        {
            if (txt_login.Text.Trim().Length == 0)
                throw new Exception("Введите логин!");

            if (txt_pass.Password.Length == 0)
                throw new Exception("Введите пароль!");

            if (registration)
            {
                if (Account.CheckAccount(txt_login.Text.Trim()))
                {
                    throw new Exception("Такой пользователь уже есть!");
                }
                else
                {
                    string tempHash = Utility.ByteArrayToString(Cryptography.GetSHA1(txt_pass.Password.Trim()));

                    using (StreamWriter fs = new StreamWriter(Account.GetAccPath(), true))
                    {
                        fs.WriteLine(txt_login.Text.Trim());
                        fs.WriteLine(tempHash);
                    }
                }
            }
            else
            {
                if (Account.CheckAccount(txt_login.Text.Trim()))
                {
                    if (Account.CheckPassword(txt_login.Text.Trim(), txt_pass.Password.Trim()))
                    {
                        Account account = new Account(txt_login.Text.Trim());
                        Start(account);
                    }
                    else
                    {
                        throw new Exception("Неправильный пароль!");
                    }
                }
                else
                {
                    throw new Exception("Такого пользователя не существует!");
                }
            }
        }

        private void Btn_signUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Validation(true);


                Utility.MsgBox("Вы успешно зарегестрировались!", "Уведомление", auth);
            }
            catch (Exception ex)
            {
                Utility.MsgBox(ex.Message, "Ошибка", auth);
            }
        }

        private void Start(Account profile)
        {
            MainWindow mw = new MainWindow(profile);
            mw.Show();
            Close();
        }
    }
}