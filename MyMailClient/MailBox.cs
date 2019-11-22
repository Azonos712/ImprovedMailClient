using MailKit;
using MailKit.Net.Imap;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace MyMailClient
{
    [Serializable]
    public class MailBox
    {

        public const string DEFAULT_SMTP_SUBDOMAIN = "smtp.";
        public const string DEFAULT_SMTP_PORT = "587";
        public const string DEFAULT_IMAP_SUBDOMAIN = "imap.";
        public const string DEFAULT_IMAP_PORT = "993";

        public string Name { get; set; }
        public string Address { get; set; }
        public string Pass { get; set; }
        public string SMTP_Dom { get; set; }
        public int SMTP_Port { get; set; }
        public string IMAP_Dom { get; set; }
        public int IMAP_Port { get; set; }

        public MailBox(string n, string a, string p, int sm, int im)
        {
            this.Name = n;
            this.Address = a;
            this.Pass = p;
            this.SMTP_Port = sm;
            this.SMTP_Dom = DEFAULT_SMTP_SUBDOMAIN + GetServerByMail(a);
            this.IMAP_Port = im;
            this.IMAP_Dom = DEFAULT_IMAP_SUBDOMAIN + GetServerByMail(a);
        }

        private string GetServerByMail(string e)
        {
            return e.Substring(e.IndexOf('@') + 1);
        }

        public override string ToString() => Name + " <" + Address + ">";


        public bool ImapConnection()
        {
            try
            {
                ImapDispose();
                CurrentData.imap = new ImapClient();
                CurrentData.imap.Connect(IMAP_Dom, IMAP_Port, true);
                CurrentData.imap.Authenticate(Address, Pass);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private void ImapDispose()
        {
            if (CurrentData.imap != null)
            {
                if (CurrentData.imap.IsConnected)
                    CurrentData.imap.Disconnect(false);
                CurrentData.imap.Dispose();
                CurrentData.imap = null;
            }
        }

        public void DownloadLetters()
        {
            //GetFolder - получает папку для указаного пространства имён
            //PersonalNamespaces - Получаем пространство имён личных папок, 
            //которое содержит личные папки почтового ящика пользователя.
            DownloadFolder(CurrentData.imap.GetFolder(CurrentData.imap.PersonalNamespaces[0]) as ImapFolder);
        }
        private void DownloadFolder(ImapFolder folder)
        {
            //поиск под папок
            foreach (ImapFolder subfolder in folder.GetSubfolders())
                DownloadFolder(subfolder);

            //Проверка на правильность папки (неправильные или ещё что-то)
            if (folder.Attributes != FolderAttributes.None && (folder.Attributes & FolderAttributes.NonExistent) == 0)
            {
                //открываем папочку
                folder.Open(FolderAccess.ReadOnly);
                //прописываем для неё полный путь на диске
                string dirFullPath = Account.GetAccMailDir() + "\\" + Address + "\\" + folder.FullName.Replace('|','\\');
                //проверяем есть ли папка, в отрицательном случае - создаём папку
                if (!Directory.Exists(dirFullPath))
                    Directory.CreateDirectory(dirFullPath);
                //Получаем файлы на диске
                List<string> files = Directory.EnumerateFiles(dirFullPath, "*.eml").OrderBy(filename => filename).ToList();
                //Получаем последний файл, если такой есть
                string last = files.Count > 0 ? files.Last().Substring(files.Last().LastIndexOf('\\') + 1) : "0";
                //получаем просто имя последнего файла
                last = System.IO.Path.GetFileNameWithoutExtension(last);
                //Ищем письма в заданной области по uid (проверяем есть ли новые письма, которые не синхронизированны)
                IList<UniqueId> uids = folder.Search(MailKit.Search.SearchQuery.Uids(
                        new UniqueIdRange(new UniqueId(uint.Parse(last) + 1), UniqueId.MaxValue)));
                //Докачиваем новые письма
                foreach (UniqueId uid in uids)
                {
                    MimeMessage message = folder.GetMessage(uid);
                    message.WriteTo(System.IO.Path.Combine(dirFullPath, uid.ToString().PadLeft(15, '0') + ".eml"));
                }
                folder.Close();
            }
        }

        public List<TreeViewItem> DisplayFolders()
        {
            string dirPath = Account.GetAccMailDir() + "\\" + Address;
            if (!Directory.Exists(dirPath))
            {
                throw new Exception("Этот почтовый ящик не был синхронизирован");
            }

            List<TreeViewItem> itco = new List<TreeViewItem>();

            foreach (string subdirPath in Directory.GetDirectories(dirPath))
                itco.Add(DisplayFolder(subdirPath));

            return itco;

        }

        TreeViewItem DisplayFolder(string pathFile)
        {
            TreeViewItem twi = new TreeViewItem();

            string[] messages = Directory.GetFiles(pathFile, "*.eml");
            List<MimeMessage> buf = new List<MimeMessage>();
            foreach (string message in messages)
                buf.Add(MimeMessage.Load(message));
            //buf.Reverse();
            //foreach (MimeMessage message in buf)
            //{
            //    twi.Items.Add(message);
            //}

            string temp = (pathFile.Substring(pathFile.LastIndexOf('\\') + 1)) + (buf.Count > 0 ?
                    (" (" + buf.Count + ")") : "");

            twi.Header = Utility.panelWithIcon("folder.png",temp);

            //twi.ItemTemplate = CurrentData.curTemplate;

            foreach (string subdirPath in Directory.GetDirectories(pathFile))
                twi.Items.Add(DisplayFolder(subdirPath));

            return twi;
        }

        public List<MimeMessage> DisplayLetters(string pathFolder)
        {
            string[] messages = Directory.GetFiles(pathFolder, "*.eml");
            List<MimeMessage> buf = new List<MimeMessage>();
            foreach (string message in messages)
                buf.Add(MimeMessage.Load(message));
            buf.Reverse();
            //foreach (MimeMessage message in buf)
            //{

            //    TreeViewItem lettertwi = new TreeViewItem();
            //    bool read = true;
            //    if (read)
            //        lettertwi.Header = Utility.panelWithIcon("empty_mail.png", "Название письма");
            //    else
            //        lettertwi.Header = Utility.panelWithIcon("full_mail.png", "Название письма");


            //    twi.Items.Add(message);
            //}
            return buf;
        }

    }
}
