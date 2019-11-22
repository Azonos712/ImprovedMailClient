using MimeKit;
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
    /// Логика взаимодействия для LetterWindow.xaml
    /// </summary>
    public partial class LetterWindow : Window
    {
        public LetterWindow()
        {
            InitializeComponent();
        }

        private void showLetter(MimeMessage message)
        {
            CurrentData.curLetter = message;
            if (message == null)
            // Если нужно очистить форму
            {
                lbl_send.Content = lbl_send.ToolTip = null;
                lbl_subj.Content = lbl_subj.ToolTip = null;
                lbl_date.Content = lbl_date.ToolTip = null;
                lbl_encr.Content = lbl_encr.ToolTip = null;
                lbl_ecp.Content = lbl_ecp.ToolTip = null;
                wbBrwsr.NavigateToString("<html></html>");
                attachPanel.Items.Clear();
                //replyBtn.IsEnabled = false;
            }
            else
            // Если нужно открыть письмо
            {
                // Заполнение полей из заголовка
                StringBuilder from_toStr = new StringBuilder("От " + message.From.Mailboxes.First().Name
                    + " <" + message.From.Mailboxes.First().Address + "> для ");
                //заполняем для кого
                foreach (MailboxAddress receiver in message.To)
                    from_toStr.Append(receiver.Name + " <" + receiver.Address + ">, ");
                //удаляем запятую и пробел
                from_toStr.Remove(from_toStr.Length - 2, 2);
                if (message.Cc.Count > 0)
                {
                    from_toStr.Append("; Копии: ");
                    foreach (MailboxAddress receiver in message.Cc)
                        from_toStr.Append(receiver.Name + " <" + receiver.Address + ">, ");
                    from_toStr.Remove(from_toStr.Length - 2, 2);
                }
                lbl_send.Content = lbl_send.ToolTip = from_toStr;

                lbl_subj.Content = lbl_subj.ToolTip = message.Subject;
                lbl_date.Content = lbl_date.ToolTip = message.Date;
                string body = message.HtmlBody ?? message.TextBody;
                //if (message.Headers.Contains(Cryptography.ENCRYPTION_ID_HEADER))
                //// Если письмо зашифровано
                //{
                //    List<CryptoKey> results = account.keys.Where(k => k.Id.Equals(message.
                //            Headers[Cryptography.ENCRYPTION_ID_HEADER]) && !k.PublicOnly).ToList();
                //    if (results.Count > 0)
                //    {
                //        CryptoKey key = results.First();
                //        // TODO: обработать неудачу
                //        body = Cryptography.Decrypt(body, key);

                //        encryptionStatusLabel.Content = encryptionStatusLabel.ToolTip =
                //                "Расшифровано с помощью \"" + key + "\"";
                //        encryptionStatusLabel.Foreground = Brushes.Green;
                //    }
                //    else
                //    {
                //        encryptionStatusLabel.Content = encryptionStatusLabel.ToolTip =
                //                "Письмо зашифровано, ключ не найден";
                //        encryptionStatusLabel.Foreground = Brushes.DarkRed;
                //    }
                //}
                //else
                //{
                //    encryptionStatusLabel.Content = encryptionStatusLabel.ToolTip =
                //            "Письмо не зашифровано";
                //    encryptionStatusLabel.Foreground = Brushes.Black;
                //}

                //if (message.Headers.Contains(Cryptography.SIGNATURE_ID_HEADER))
                //// Если письмо подписано
                //{
                //    List<CryptoKey> results = account.keys.Where(k => k.Id.Equals(message.
                //            Headers[Cryptography.SIGNATURE_ID_HEADER])).ToList();
                //    if (results.Count > 0)
                //    {
                //        CryptoKey key = results.First();
                //        if (Cryptography.Verify(body, message.Headers[Cryptography.SIGNATURE_HEADER], key))
                //        {
                //            signatureStatusLabel.Content = signatureStatusLabel.ToolTip =
                //                    "Верифицировано с помощью \"" + key + "\"";
                //            if (message.From.Mailboxes.First().Address.Equals(key.OwnerAddress))
                //                signatureStatusLabel.Foreground = Brushes.Green;
                //            else
                //            {
                //                signatureStatusLabel.Content = signatureStatusLabel.ToolTip +=
                //                        " (отправитель не совпадает)";
                //                signatureStatusLabel.Foreground = Brushes.DarkOrange;
                //            }
                //        }
                //        else
                //        {
                //            signatureStatusLabel.Content = signatureStatusLabel.ToolTip =
                //                    "Подпись распознана с помощью\"" + key +
                //                    "\", однако целостность письма нарушена";
                //            signatureStatusLabel.Foreground = Brushes.DarkRed;
                //        }
                //    }
                //    else
                //    {
                //        signatureStatusLabel.Content = signatureStatusLabel.ToolTip =
                //                "Письмо подписано, но нет подходящего ключа для верификации";
                //        signatureStatusLabel.Foreground = Brushes.Black;
                //    }
                //}
                //else
                //{
                //    signatureStatusLabel.Content = signatureStatusLabel.ToolTip =
                //            "Письмо не подписано";
                //    signatureStatusLabel.Foreground = Brushes.Black;
                //}

                // Отображение тела письма
                int index = body.IndexOf("<html>", StringComparison.OrdinalIgnoreCase);
                if (index < 0)
                    body = "<html><meta charset=\"" + Utility.HTML_CHARSET + "\"><body>" + body + "</body></html>";
                else
                    body = body.Insert(index + 6, "<meta charset=\"" + Utility.HTML_CHARSET + "\">");
                // TODO: превратить переносы строк в <br>, если письмо не HTML

                wbBrwsr.NavigateToString(body);

                //if (message.Headers.Contains(Cryptography.KEY_DELIVERY_HEADER))
                //// Если это письмо - доставка ключа
                //{
                //    // Получить объект ключа
                //    MimeEntity attachment = message.Attachments.First();
                //    string tmpFile = System.IO.Path.GetTempFileName();
                //    SaveAttachment(attachment, tmpFile);
                //    CryptoKey key = CryptoKey.DeserializeFromFile(tmpFile);

                //    if (Utils.ShowConfirmation("Добавить ключ \"" + key + "\" в библиотеку ключей?") == MessageBoxResult.Yes)
                //    {
                //        if (KeysManagerWindow.AddKey(account, key))
                //            account.Serialize();
                //        else
                //            Utils.ShowWarning("Такой ключ уже есть в библиотеке ключей");
                //    }
                //}
                //else
                //// Если это письмо обыкновенное
                //{
                // Отображение прикреплений
                attachPanel.Items.Clear();
                foreach (MimeEntity attachment in message.Attachments)
                    attachPanel.Items.Add(attachment);
                //}
                //replyBtn.IsEnabled = true;
            }
        }
    }
}
