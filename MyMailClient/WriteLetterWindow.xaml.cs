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

            List<CryptoKey> encryptionKeys = SelectKeys(true, true);
            cmbx_encryption.ItemsSource = encryptionKeys;
            List<CryptoKey> signatureKeys = SelectKeys(false, false);
            cmbx_sign.ItemsSource = signatureKeys;


            publicToKey = tempKey;
            if (publicToKey != null)
            {
                bodyHtmlEditor.IsEnabled = btn_attach.IsEnabled =
                        cmbx_encryption.IsEnabled = chbx_encrypt.IsEnabled =
                        cmbx_sign.IsEnabled = chbx_sign.IsEnabled = false;
            }
        }

        private List<CryptoKey> SelectKeys(bool purpose, bool includePublic)
        {
            return new List<CryptoKey>(CurrentData.curAcc.Keys.Where(key =>
                key.EncrOrSign == purpose && (includePublic || !key.PublicOnly)));
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

                if (chbx_encrypt.IsChecked.Value && cmbx_encryption.SelectedItem as CryptoKey == null)
                    throw new Exception("Выберите ключ шифрования из списка или снимите галочку");

                if (chbx_sign.IsChecked.Value && cmbx_sign.SelectedItem as CryptoKey == null)
                    throw new Exception("Выберите подпись из списка или снимите галочку");

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
                
                CryptoKey encryptionKey = cmbx_encryption.SelectedItem as CryptoKey;
                if (encryptionKey != null && chbx_encrypt.IsChecked.Value)
                {
                    bodyBuilder.HtmlBody = Cryptography.Encrypt(bodyBuilder.HtmlBody, encryptionKey);
                    mimeMsg.Headers.Add(Cryptography.ENCRYPTION_ID_HEADER, encryptionKey.Id);
                }
                //var signtemp = "";
                CryptoKey signatureKey = cmbx_sign.SelectedItem as CryptoKey;
                if (signatureKey != null && chbx_sign.IsChecked.Value)
                {
                    string signature = Cryptography.Sign(bodyBuilder.HtmlBody, signatureKey);
                    mimeMsg.Headers.Add(Cryptography.SIGNATURE_ID_HEADER, signatureKey.Id);
                    mimeMsg.Headers.Add(Cryptography.SIGNATURE_HEADER, signature);

                    //signtemp = signature;
                    //string signature2 = Cryptography.Sign("Привет", signatureKey);
                    //string body2 = mimeMsg.HtmlBody ?? mimeMsg.TextBody;
                    //bool temp2 = Cryptography.Verify(body2, signtemp, signatureKey);

                }
                foreach (FileInfo f in attachmentsPanel.Items)
                    bodyBuilder.Attachments.Add((f.FullName));

                mimeMsg.Body = bodyBuilder.ToMessageBody();

                


                btn_send.IsEnabled = false;
                var temp = await Task.Run(() => CurrentData.curMail.sendMessage(mimeMsg));
                btn_send.IsEnabled = true;
                if (temp == false)
                    throw new Exception("Что-то помешало отправке сообщения!");


                Utility.MsgBox("Сообщение успешно отправлено!","Уведомление",writeLetterWnd);

                DialogResult = true;
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
