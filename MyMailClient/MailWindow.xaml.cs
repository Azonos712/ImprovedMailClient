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
    /// Логика взаимодействия для MailWindow.xaml
    /// </summary>
    public partial class MailWindow : Window
    {
        Account tempAcc;
        public MailWindow(Account profile)
        {
            InitializeComponent();
            tempAcc = profile;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Validation();

                int sm = int.Parse(txt_smtp.Text.Trim());
                int im = int.Parse(txt_imap.Text.Trim());

                MailBox m = new MailBox(txt_name.Text.Trim(), txt_address.Text.Trim(), txt_pass.Password, sm, im);

                tempAcc.MlBxs.Add(m);

                Utility.MsgBox("Вы успешно добавили новый почтовый ящик!", "Уведомление", mailWin);

                this.Close();
            }
            catch (Exception ex)
            {
                Utility.MsgBox(ex.Message, "Ошибка", mailWin);
            }

            //string address = mw.addressTB.Text.Trim();
            //if (!account.mailboxes.Any(item => item.Address.Equals(address)))
            //{
            //    account.mailboxes.Add(mw.mailbox);
            //    account.Serialize();
            //}
            //else
            //{
            //    Utils.ShowWarning(address + " уже есть в списке почтовых ящиков");
            //}
        }

        private void Validation()
        {
            if (txt_name.Text.Trim().Length == 0)
                throw new Exception("Введите название!");

            if (tempAcc.ContainMailName(txt_name.Text.Trim()))
                throw new Exception("Такое название уже используется!");

            if (txt_address.Text.Trim().Length == 0)
                throw new Exception("Введите адресс почтового клиента!");

            if (!Utility.ValidateEmail(txt_address.Text.Trim()))
                throw new Exception("Введён некорректный адрес!");

            if (tempAcc.ContainMailAddress(txt_address.Text.Trim()))
                throw new Exception("Такой адрес уже используется!");

            if (txt_pass.Password.Length == 0)
                throw new Exception("Введите пароль!");

            if (txt_smtp.Text.Trim().Length == 0)
                throw new Exception("Введите порт SMTP!");

            if (txt_imap.Text.Trim().Length == 0)
                throw new Exception("Введите порт IMAP!");
        }
    }
}
