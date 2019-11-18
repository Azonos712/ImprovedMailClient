using MailKit;
using MailKit.Net.Imap;
using MimeKit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyMailClient
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Account curAcc;
        MailBox curMail;
        ImapClient imap;
        private static DataTemplate letterDT;
        public MainWindow(Account profile)
        {
            InitializeComponent();
            curAcc = new Account(profile);

            UpdateItemsInComboBox(listOfMails, curAcc.MlBxs);
            //listOfMails.ItemsSource = curAcc.MlBxs;
        }
        /// <summary>
        /// Добавить новый почтовый ящик
        /// </summary>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MailWindow mw = new MailWindow(curAcc);
            if (mw.ShowDialog() == true)
            {
                curAcc.Srlz();
                UpdateItemsInComboBox(listOfMails, curAcc.MlBxs);
            }
            this.Focus();
            //listOfMails.ItemsSource = curAcc.MlBxs;
        }
        /// <summary>
        /// Изменить выбранный почтовый ящик
        /// </summary>
        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            if (curMail == null)
            {
                Utility.MsgBox("Вам стоит выбрать почтовый ящик!", "Уведомление", this);
                return;
            }

            MailWindow mw = new MailWindow(curAcc,curMail);
            if (mw.ShowDialog()==true)
            {
                curAcc.Srlz();
                UpdateItemsInComboBox(listOfMails, curAcc.MlBxs, true);
            }
            this.Focus();
            //listOfMails.SelectedItem = listOfMails.Items[0];
        }
        /// <summary>
        /// Удалить выбранный почтовый ящик
        /// </summary>
        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            if (curMail == null)
            {
                Utility.MsgBox("Вам стоит выбрать почтовый ящик!", "Уведомление", this);
                return;
            }

            curAcc.MlBxs.Remove(curMail);
            curMail = null;
            curAcc.Srlz();
            UpdateItemsInComboBox(listOfMails, curAcc.MlBxs);
        }

        //TODO:Возможно переделать
        private void UpdateItemsInComboBox(ComboBox cb, List<MailBox> items, bool upload=false)
        {
            var selItem = upload == true ? items[items.Count-1] : cb.SelectedItem;
            cb.Items.Clear();
            foreach (MailBox m in items)
            {
                cb.Items.Add(m);
            }
            if (selItem != null && cb.Items.Contains(selItem))
                cb.SelectedItem = selItem;

            CountingBadge.Badge = items.Count;
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
                curMail = listOfMails.SelectedItem as MailBox;
                //string curFMN = listOfMails.SelectedItem.ToString();
                //string temp = curFMN.Substring(curFMN.IndexOf('<') + 1, curFMN.LastIndexOf('>') - curFMN.IndexOf('<') - 1);
                //curMail = curAcc.MlBxs.Find(x => x.Address.Contains(temp));

                //useMailBox();
            }
        }

        //void useMailBox()
        //{
        //    listOfLetters.Items.Clear();

        //    // TODO: выполнять подключение и загрузку писем в отдельном потоке
        //    if (ImapConnection(curMail))
        //        DownloadLetters();

        //    DisplayLetters();
        //}

        //private bool ImapConnection(MailBox mailbox)
        //{
        //    ImapDispose();
        //    imap = new ImapClient();
        //    try
        //    {
        //        imap.Connect(mailbox.IMAP_Dom, mailbox.IMAP_Port, true);
        //        imap.Authenticate(mailbox.Address, mailbox.Pass);
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Utility.MsgBox(ex.Message, "Ошибка", this);
        //        return false;
        //    }
        //}
        //private void ImapDispose()
        //{
        //    if (imap != null)
        //    {
        //        if (imap.IsConnected)
        //            imap.Disconnect(false);
        //        imap.Dispose();
        //        imap = null;
        //    }
        //}
        //public void DownloadLetters()
        //{
        //    /*foreach (ImapFolder folder in imap.GetFolders(imap.PersonalNamespaces.First()))
        //        downloadFolder(folder);*/
        //    DownloadFolder(imap.GetFolder(imap.PersonalNamespaces.First()) as ImapFolder);
        //}
        //private void DownloadFolder(ImapFolder folder)
        //{
        //    foreach (ImapFolder subfolder in folder.GetSubfolders())
        //        DownloadFolder(subfolder);

        //    if (folder.Attributes != FolderAttributes.None && (folder.Attributes & FolderAttributes.NonExistent) == 0)
        //    {
        //        folder.Open(FolderAccess.ReadOnly);
        //        string dirPath = Account.GetAccInfoPath(curAcc.Login) + "\\" + curMail.Address + "\\" + folder.Name;
        //        if (!Directory.Exists(dirPath))
        //            Directory.CreateDirectory(dirPath);

        //        List<string> files = Directory.EnumerateFiles(dirPath, "*.eml").OrderBy(filename => filename).ToList();
        //        string last = files.Count > 0 ? files.Last().Substring(files.Last().LastIndexOf('\\') + 1) : "0";
        //        last = System.IO.Path.GetFileNameWithoutExtension(last);
        //        IList<UniqueId> uids = folder.Search(MailKit.Search.SearchQuery.Uids(
        //                new UniqueIdRange(new UniqueId(uint.Parse(last) + 1), UniqueId.MaxValue)));
        //        foreach (UniqueId uid in uids)
        //        {
        //            MimeMessage message = folder.GetMessage(uid);
        //            message.WriteTo(System.IO.Path.Combine(dirPath, uid.ToString().PadLeft(15, '0') + ".eml"));
        //        }
        //        folder.Close();
        //    }
        //}

        //private void DisplayLetters()
        //{
        //    string dirPath = Account.GetAccInfoPath(curAcc.Login) + "\\" + curMail.Address;
        //    if (!Directory.Exists(dirPath))
        //    {
        //        Utility.MsgBox("Этот почтовый ящик не был синхронизирован","Ошибка",this);
        //        return;
        //    }

        //    foreach (string subdirPath in Directory.GetDirectories(dirPath))
        //        DisplayFolder(subdirPath, listOfLetters.Items);
        //}

        //private void DisplayFolder(string dirPath, ItemCollection collection)
        //{
        //    foreach (string subdirPath in Directory.GetDirectories(dirPath))
        //        DisplayFolder(subdirPath, collection);

        //    TreeViewItem twi = new TreeViewItem();
        //    string[] messages = Directory.GetFiles(dirPath, "*.eml");
        //    List<MimeMessage> buf = new List<MimeMessage>();
        //    foreach (string message in messages)
        //        buf.Add(MimeMessage.Load(message));
        //    buf.Reverse();
        //    foreach (MimeMessage message in buf)
        //        twi.Items.Add(message);

        //    twi.Header = (dirPath.Substring(dirPath.LastIndexOf('\\') + 1)) + (twi.Items.Count > 0 ?
        //            (" (" + twi.Items.Count + ")") : "");
        //    twi.ItemTemplate = letterDT;

        //    collection.Add(twi);
        //}

    }
}
