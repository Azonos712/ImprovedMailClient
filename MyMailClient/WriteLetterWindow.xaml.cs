using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
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
            txt_namefrom.Text = CurrentData.curMail.Name;
            txt_addressfrom.Text = "<" + CurrentData.curMail.Address + ">";
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

        private void btn_send_Click(object s, RoutedEventArgs e)
        {
            try
            {
                if (tmscntrl_toPanel.Items.Count == 0)
                    throw new Exception("Добавьте хотя бы одного получателя!");

                string senderName = txt_namefrom.Text.Trim();
                senderName = senderName.Length > 0 ? senderName : CurrentData.curMail.Address;
                MailAddress sender = new MailAddress(CurrentData.curMail.Address, senderName);

                MimeKit.MimeMessage mimeMsg = new MimeKit.MimeMessage();
                //mimeMsg.From.Add(new MimeKit.MailboxAddress());
                //message1.From(sender);

                using (MailMessage message = new MailMessage())
                {
                    message.From = sender;
                    //message.To.Add(txt_to.Text);
                    message.Subject = txt_subject.Text.Length > 0 ? txt_subject.Text : "Без темы";

                    //if (KeyToDeliver != null)
                    //{
                    //    message.Headers.Add(Cryptography.KEY_DELIVERY_HEADER, "public");

                    //    string filename = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "tcr-public.key");
                    //    KeyToDeliver.SerializeToFile(filename);
                    //    AttachFile(filename);

                    //    string purpuse;
                    //    if (KeyToDeliver.KeyPurpose == CryptoKey.Purpose.Encryption)
                    //        purpuse = "для шифрования";
                    //    else if (KeyToDeliver.KeyPurpose == CryptoKey.Purpose.Signature)
                    //        purpuse = "для верификации цифровой подписи";
                    //    else
                    //        throw new NotImplementedException("Как Вы здесь оказались?");

                    //    string ownerMatch;
                    //    if (KeyToDeliver.OwnerAddress.Equals(mailbox.Address))
                    //        ownerMatch = "<span style=\"color: " + Utils.ColorToHexString(Colors.Green) +
                    //                "\">ключ принадлежит отправителю</span>";
                    //    else
                    //        ownerMatch = "<span style=\"color: " + Utils.ColorToHexString(Colors.DarkOrange) +
                    //                "\">не совпадает с адресом отправителя</span>";

                    //    StringBuilder body = new StringBuilder();
                    //    body.Append("Это письмо содержит открытый ключ " + purpuse + "<br>");
                    //    body.Append("Адрес владельца ключа: " + KeyToDeliver.OwnerAddress + " - " + ownerMatch + "<br>");
                    //    body.Append("Дата и время создания ключа: " + KeyToDeliver.DateTime + "<br>");
                    //    body.Append("<br>");
                    //    body.Append("Приймите запрос в The Crypto, чтобы добавить этот ключ в Вашу библиотеку ключей");
                    //    bodyHtmlEditor.ContentHtml = body.ToString();
                    //}

                    message.Body = "<html><meta charset=\"" + Utility.HTML_CHARSET + "\"><body>" + bodyHtmlEditor.ContentHtml + "</body></html>";

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

                    message.IsBodyHtml = true;
                    foreach (FileInfo f in attachmentsPanel.Items)
                        message.Attachments.Add(new Attachment(f.FullName));



                    //CurrentData.curMail.sendMessage(message);


                }
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
