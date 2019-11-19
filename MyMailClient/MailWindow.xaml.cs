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
        bool add;
        public MailWindow(bool temp)
        {
            InitializeComponent();
            add = temp;
            if (add == false)
            {
                txt_name.Text = CurrentData.curMail.Name;
                txt_address.Text = CurrentData.curMail.Address;
                txt_pass.Password = CurrentData.curMail.Pass;
                txt_smtp.Text = CurrentData.curMail.SMTP_Port.ToString();
                txt_imap.Text = CurrentData.curMail.IMAP_Port.ToString();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (tempBox != null) { 
                //    tempAcc.MlBxs.Remove(tempBox);
                //    tempBox = null;
                //}

                Validation();

                if (add == false)
                    CurrentData.curAcc.MlBxs.Remove(CurrentData.curMail);

                MailBox m = new MailBox(txt_name.Text.Trim(), txt_address.Text.Trim(), txt_pass.Password, int.Parse(txt_smtp.Text.Trim()), int.Parse(txt_imap.Text.Trim()));
                CurrentData.curAcc.MlBxs.Add(m);
                //tempAcc.MlBxs.Add(m);
                Utility.MsgBox("Готово!", "Уведомление", mailWin);

                this.DialogResult = true;
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

            var temp1 = CurrentData.curMail != null ? CurrentData.curMail.Name : String.Empty;
            if (CurrentData.curAcc.ContainMailName(txt_name.Text.Trim(), temp1))
                throw new Exception("Такое название уже используется!");

            if (txt_address.Text.Trim().Length == 0)
                throw new Exception("Введите адресс почтового клиента!");

            if (!Utility.ValidateEmail(txt_address.Text.Trim()))
                throw new Exception("Введён некорректный адрес!");

            var temp2 = CurrentData.curMail != null ? CurrentData.curMail.Address : String.Empty;
            if (CurrentData.curAcc.ContainMailAddress(txt_address.Text.Trim(), temp2))
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
