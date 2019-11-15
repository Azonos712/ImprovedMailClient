using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
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

            zeroOpacitySettings();

            // Style = "{StaticResource MyOpacityWindow}" AllowsTransparency = "True" WindowStyle = "None"
            DoubleAnimation da = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(2));
            da.Completed += WinAnimation_Completed;
            auth.BeginAnimation(Window.OpacityProperty, da);
        }
        private void WinAnimation_Completed(object sender, EventArgs e)
        {
            DoubleAnimation daImgOpacity = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(2));
            daImgOpacity.Completed += ImgAnimation_Completed;
            img_mail.BeginAnimation(Image.OpacityProperty, daImgOpacity);
        }
        private void ImgAnimation_Completed(object sender, EventArgs e)
        {
            //TODO:удаление элемента
            img_mail.Margin = new Thickness(251, 0, 0, 0);
            DoubleAnimation daOpacity1 = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.6));
            DoubleAnimation daOpacity3 = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.6));

            DoubleAnimation daOpacity2 = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(1));
            DoubleAnimation daOpacity4 = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(1));
            daOpacity4.Completed += DaOpacity4_Completed;

            //daOpacity1.Completed += DaOpacity1_Completed;
            icon_log.BeginAnimation(MaterialDesignThemes.Wpf.PackIcon.OpacityProperty, daOpacity1); 
            txt_login.BeginAnimation(TextBox.OpacityProperty, daOpacity3);
            icon_pass.BeginAnimation(MaterialDesignThemes.Wpf.PackIcon.OpacityProperty, daOpacity2);
            txt_pass.BeginAnimation(TextBox.OpacityProperty, daOpacity4);
        }

        private void DaOpacity1_Completed(object sender, EventArgs e)
        {
        }

        private void DaOpacity4_Completed(object sender, EventArgs e)
        {
            DoubleAnimation daOpacity5 = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5));
            daOpacity5.Completed += DaOpacity5_Completed;
            btn_signIn.BeginAnimation(Button.OpacityProperty, daOpacity5);
        }

        private void DaOpacity5_Completed(object sender, EventArgs e)
        {
            DoubleAnimation daOpacity6 = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5));
            daOpacity6.Completed += DaOpacity6_Completed;
            btn_signUp.BeginAnimation(Button.OpacityProperty, daOpacity6);
        }

        private void DaOpacity6_Completed(object sender, EventArgs e)
        {
            DoubleAnimation daOpacity7 = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5));
            btn_exit.BeginAnimation(Button.OpacityProperty, daOpacity7);
        }

        void zeroOpacitySettings()
        {
            icon_log.Opacity = 0;
            icon_pass.Opacity = 0;
            txt_login.Opacity = 0;
            txt_pass.Opacity = 0;
            btn_signIn.Opacity = 0;
            btn_exit.Opacity = 0;
            btn_signUp.Opacity = 0;
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

                    Account account = new Account(txt_login.Text.Trim(), tempHash);
                    account.Srlz();

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
                        Account account = new Account(Account.Dsrlz(txt_login.Text.Trim()));
                        
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

        private void btn_exit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }


    }
}