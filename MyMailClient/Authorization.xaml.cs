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
    /// Логика взаимодействия для Authorization.xaml
    /// </summary>
    public partial class Authorization : Window
    {
        public Authorization()
        {
            InitializeComponent();
        }

        private void Btn_signIn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Validation();


                MessageBox.Show("Вы успешно авторизировались!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void Validation()
        {
            if (txt_login.Text.Trim().Length == 0)
                throw new Exception("Введите логин!");

            if (txt_pass.Text.Length == 0)
                throw new Exception("Введите пароль!");

            switch (CheckAccount(txt_login.Text.Trim()))
            {
                case 1:
                    throw new Exception("Такого пользователя не существует!");
                case 2:
                    throw new Exception("Такой пользователь уже существует!");
            }
        }

        int CheckAccount(string login)
        {
            int code = 0;



            return code;
        }
    }
}
