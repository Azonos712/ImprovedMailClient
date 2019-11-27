using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
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
        CryptoKey publicToKey;
        public WriteLetterWindow(CryptoKey tempKey=null)
        {
            InitializeComponent(); 
            txt_namefrom.Text = CurrentData.curMail.Name;
            txt_addressfrom.Text = "<" + CurrentData.curMail.Address + ">";

            publicToKey = tempKey;
            if (publicToKey != null)
            {
                bodyHtmlEditor.IsEnabled = btn_attach.IsEnabled =
                        cmbx_encryption.IsEnabled = chbx_encrypt.IsEnabled =
                        cmbx_sign.IsEnabled = chbx_sign.IsEnabled = false;
            }
        }

        private void btn_addAddress_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txt_addAddress.Text.Trim().Length == 0)
                    throw new Exception("Введите адрес получателя!");

                if (!Utility.ValidateEmail(txt_addAddress.Text.Trim()))
                    throw new Exception("Введён некорректный адрес!");

                tmscntrl_toPanel.Items.Add(txt_addAddress.Text.Trim());
                txt_addAddress.Text = "";
            }
            catch (Exception ex)
            {
                Utility.MsgBox(ex.Message, "Ошибка", writeLetterWnd);
            }
        }
        private void Chip_DeleteClick(object sender, RoutedEventArgs e)
        {
            tmscntrl_toPanel.Items.Remove((sender as FrameworkElement).DataContext);
        }

        private async void btn_send_Click(object s, RoutedEventArgs e)
        {
            try
            {
                if (tmscntrl_toPanel.Items.Count == 0)
                    throw new Exception("Добавьте хотя бы одного получателя!");

                string senderName = txt_namefrom.Text.Trim();
                senderName = senderName.Length > 0 ? senderName : CurrentData.curMail.Address;

                MimeKit.MimeMessage mimeMsg = new MimeKit.MimeMessage();
                mimeMsg.From.Add(new MimeKit.MailboxAddress(senderName, CurrentData.curMail.Address));
                
                foreach (var tempaddress in tmscntrl_toPanel.Items)
                    mimeMsg.To.Add(new MimeKit.MailboxAddress(tempaddress.ToString()));
                
                mimeMsg.Subject = txt_subject.Text.Trim().Length > 0 ? txt_subject.Text.Trim() : "Без темы";

                if (publicToKey != null)
                {
                    mimeMsg.Headers.Add(Cryptography.KEY_DELIVERY_HEADER, "public");

                    //string filename = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "tcr-public.key");
                    //KeyToDeliver.SerializeToFile(filename);
                    //AttachFile(filename);

                    string purpuse=" ";
                    if (publicToKey.EncrOrSign == true)
                        purpuse = "для шифрования";
                    else if (publicToKey.EncrOrSign == false)
                        purpuse = "для верификации цифровой подписи";

                    string ownerMatch;
                    if (publicToKey.OwnerAddress.Equals(CurrentData.curMail.Address))
                        ownerMatch = "<span style=\"color: " + Utility.ColorToHexString(Colors.Green) +
                                "\">ключ принадлежит отправителю</span>";
                    else
                        ownerMatch = "<span style=\"color: " + Utility.ColorToHexString(Colors.DarkOrange) +
                                "\">не совпадает с адресом отправителя</span>";

                    StringBuilder body = new StringBuilder();
                    body.Append("Это письмо содержит открытый ключ " + purpuse + "<br>");
                    body.Append("Адрес владельца ключа: " + publicToKey.OwnerAddress + " - " + ownerMatch + "<br>");
                    body.Append("Дата и время создания ключа: " + publicToKey.DateTime + "<br>");
                    body.Append("<br>");
                    body.Append("Примите запрос в менджере ключей, чтобы добавить этот ключ в Вашу библиотеку ключей"); 
                    body.Append("<br>");
                    body.Append("$"+ new JavaScriptSerializer().Serialize(publicToKey) + "$");
                    bodyHtmlEditor.ContentHtml = body.ToString();
                }


                var bodyBuilder = new MimeKit.BodyBuilder();
                bodyBuilder.HtmlBody= "<html><meta charset=\"" + Utility.HTML_CHARSET + "\"><body>" + bodyHtmlEditor.ContentHtml + "</body></html>";
                foreach (FileInfo f in attachmentsPanel.Items)
                    bodyBuilder.Attachments.Add((f.FullName));

                mimeMsg.Body = bodyBuilder.ToMessageBody();

                btn_send.IsEnabled = false;
                var temp = await Task.Run(() => CurrentData.curMail.sendMessage(mimeMsg));
                btn_send.IsEnabled = true;

                Utility.MsgBox("Сообщение успешно отправлено!","Уведомление",writeLetterWnd);
                this.Close();


                

                
                //CryptoKey signatureKey = signatureCB.SelectedItem as CryptoKey;
                //if (signatureKey != null)
                //{
                //    string signature = Cryptography.Sign(message.Body, signatureKey);
                //    message.Headers.Add(Cryptography.SIGNATURE_ID_HEADER, signatureKey.Id);
                //    message.Headers.Add(Cryptography.SIGNATURE_HEADER, signature);
                //}

                //CryptoKey encryptionKey = encryptionCB.SelectedItem as CryptoKey;
                //if (encryptionKey != null)
                //{
                //    message.Body = Cryptography.Encrypt(message.Body, encryptionKey);
                //    message.Headers.Add(Cryptography.ENCRYPTION_ID_HEADER, encryptionKey.Id);
                //}

                //message.IsBodyHtml = true;
                //foreach (FileInfo f in attachmentsPanel.Items)
                //    message.Attachments.Add(new Attachment(f.FullName));



                //CurrentData.curMail.sendMessage(message);

            }
            catch (Exception ex)
            {
                Utility.MsgBox(ex.Message, "Ошибка", writeLetterWnd);
            }
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
        private void Chip_DeleteClick_1(object sender, RoutedEventArgs e)
        {
            attachmentsPanel.Items.Remove((sender as FrameworkElement).DataContext);
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
