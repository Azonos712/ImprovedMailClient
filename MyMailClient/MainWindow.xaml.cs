using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace MyMailClient
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Account curAcc;
        //MailBox curMail;
        //ImapClient imap;

        //private static DataTemplate letterDT;
        bool noConnection = false;
        public MainWindow()
        {
            InitializeComponent();

            //setTemplate();

            UpdateItemsInComboBox(listOfMails, CurrentData.curAcc.MlBxs);
            //listOfMails.ItemsSource = curAcc.MlBxs;
        }

        void setTemplate()
        {
            var spFactory = new FrameworkElementFactory(typeof(StackPanel));
            spFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

            var iconTbFactory = new FrameworkElementFactory(typeof(Image));
            iconTbFactory.SetBinding(Image.SourceProperty, new Binding("Resources\\empty_mail.png"));
            spFactory.AppendChild(iconTbFactory);

            var senderTbFactory = new FrameworkElementFactory(typeof(TextBlock));
            senderTbFactory.SetBinding(TextBlock.TextProperty, new Binding("From[0].Name"));
            spFactory.AppendChild(senderTbFactory);

            var sep1TbFactory = new FrameworkElementFactory(typeof(TextBlock));
            sep1TbFactory.SetValue(TextBlock.TextProperty, " -> ");
            spFactory.AppendChild(sep1TbFactory);

            var receiverTbFactory = new FrameworkElementFactory(typeof(TextBlock));
            receiverTbFactory.SetBinding(TextBlock.TextProperty, new Binding("To[0].Address"));
            spFactory.AppendChild(receiverTbFactory);

            var sep2TbFactory = new FrameworkElementFactory(typeof(TextBlock));
            sep2TbFactory.SetValue(TextBlock.TextProperty, " : ");
            spFactory.AppendChild(sep2TbFactory);

            var subjectTbFactory = new FrameworkElementFactory(typeof(TextBlock));
            subjectTbFactory.SetBinding(TextBlock.TextProperty, new Binding("Subject"));
            spFactory.AppendChild(subjectTbFactory);

            // TODO: установить триггер "прочитано-не прочитано"

            CurrentData.curTemplate = new DataTemplate();
            CurrentData.curTemplate.VisualTree = spFactory;
        }

        //TODO:Возможно переделать
        private void UpdateItemsInComboBox(ComboBox cb, List<MailBox> items)
        {
            //var selItem = update == true ? items[items.Count - 1] : cb.SelectedItem;
            //var selItem = cb.SelectedItem;

            cb.Items.Clear();
            foreach (MailBox m in items)
            {
                cb.Items.Add(m);
            }
            if (CurrentData.curMail != null)
                cb.SelectedItem = CurrentData.curMail;

            CountingBadge.Badge = items.Count;
        }

        /// <summary>
        /// Добавить новый почтовый ящик
        /// </summary>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MailWindow mw = new MailWindow(true);
            if (mw.ShowDialog() == true)
            {
                CurrentData.curAcc.Srlz();
                noConnection = true;
                UpdateItemsInComboBox(listOfMails, CurrentData.curAcc.MlBxs);
                noConnection = false;
            }
            this.Focus();
            //listOfMails.ItemsSource = curAcc.MlBxs;
        }
        /// <summary>
        /// Изменить выбранный почтовый ящик
        /// </summary>
        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            if (CurrentData.curMail == null)
            {
                Utility.MsgBox("Вам стоит выбрать почтовый ящик!", "Уведомление", this);
                return;
            }

            MailWindow mw = new MailWindow(false);
            if (mw.ShowDialog() == true)
            {
                CurrentData.curMail = CurrentData.curAcc.MlBxs[CurrentData.curAcc.MlBxs.Count - 1];
                CurrentData.curAcc.Srlz();
                UpdateItemsInComboBox(listOfMails, CurrentData.curAcc.MlBxs);
            }
            this.Focus();
            //listOfMails.SelectedItem = listOfMails.Items[0];
        }
        /// <summary>
        /// Удалить выбранный почтовый ящик
        /// </summary>
        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            if (CurrentData.curMail == null)
            {
                Utility.MsgBox("Вам стоит выбрать почтовый ящик!", "Уведомление", this);
                return;
            }

            CurrentData.curAcc.MlBxs.Remove(CurrentData.curMail);
            CurrentData.curMail = null;
            listOfLetters.Items.Clear();
            showLetter(null);
            CurrentData.curAcc.Srlz();
            UpdateItemsInComboBox(listOfMails, CurrentData.curAcc.MlBxs);
        }

        /// <summary>
        /// Выход из программы
        /// </summary>
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
        /// <summary>
        /// Выход из аккаунта
        /// </summary>
        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            Authorization az = new Authorization();
            az.Show();
            this.Close();
        }

        private void listOfMails_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listOfMails.SelectedItem != null)
            {
                CurrentData.curMail = listOfMails.SelectedItem as MailBox;
                //string curFMN = listOfMails.SelectedItem.ToString();
                //string temp = curFMN.Substring(curFMN.IndexOf('<') + 1, curFMN.LastIndexOf('>') - curFMN.IndexOf('<') - 1);
                //curMail = curAcc.MlBxs.Find(x => x.Address.Contains(temp));

                if (noConnection == false)
                    useMailBox();
                //useMailBox();
            }
        }

        void useMailBox()
        {
            try
            {
                listOfLetters.Items.Clear();
                showLetter(null);

                // TODO: выполнять подключение и загрузку писем в отдельном потоке
                if (CurrentData.curMail.ImapConnection())
                    CurrentData.curMail.DownloadLetters();
                else
                    Utility.MsgBox("Что-то помешало подключению данного почтового ящика!", "Ошибка", this);

                var temp = CurrentData.curMail.DisplayLetters();
                for (int i = 0; i < temp.Count; i++)
                {
                    listOfLetters.Items.Add(temp[i]);
                }
            }
            catch (Exception ex)
            {
                Utility.MsgBox(ex.Message, "Ошибка", this);
            }
        }

        private void listOfLetters_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MimeMessage message = listOfLetters.SelectedItem as MimeMessage;
            
            if (message != null)
            {
                // TODO: добавить флаг "прочитано"
                showLetter(message);
            }
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