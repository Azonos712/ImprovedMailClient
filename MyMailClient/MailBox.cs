using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Pop3;
using MailKit.Search;
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

                if (!Directory.Exists("Logs"))
                    Directory.CreateDirectory("Logs");

                CurrentData.imap = new ImapClient(new ProtocolLogger("Logs\\imap.log"));
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

        bool lettersCompare(MimeMessage msg1, MimeMessage msg2)
        {
            if (msg1.From.Mailboxes.First().Name != msg2.From.Mailboxes.First().Name)
                return false;

            if (msg1.From.Mailboxes.First().Address != msg2.From.Mailboxes.First().Address)
                return false;

            if (msg1.To.Mailboxes.First().Name != msg2.To.Mailboxes.First().Name)
                return false;

            if (msg1.To.Mailboxes.First().Address != msg2.To.Mailboxes.First().Address)
                return false;

            if (msg1.Cc.Count != msg2.Cc.Count)
            {
                return false;
            }

            for (int i = 0; i < msg1.Cc.Count; i++)
            {
                if (msg1.Cc[i].Name != msg2.Cc[i].Name)
                    return false;
            }

            if (msg1.Subject != msg2.Subject)
                return false;

            if (msg1.Date != msg2.Date)
                return false;

            string body1 = msg1.HtmlBody ?? msg1.TextBody;
            string body2 = msg2.HtmlBody ?? msg2.TextBody;

            if (body1 != body2)
                return false;

            if (msg1.Attachments.Count() != msg2.Attachments.Count())
                return false;

            return true;
        }

        public void StartResync()
        {
            IList<IMailFolder> serverFolders = CurrentData.imap.GetFolders(CurrentData.imap.PersonalNamespaces.First());
            //List<string> names = folders.Select(t => t.FullName).ToList();
            //List<ImapFolder> serverFolders = new List<ImapFolder>();
            //for (int i = 0; i < IserverFolders.Count; i++)
            //{
            //    serverFolders.Add(IserverFolders[i] as ImapFolder);
            //}

            if (!Directory.Exists(Account.GetAccMailDir() + "\\" + Address))
                Directory.CreateDirectory(Account.GetAccMailDir() + "\\" + Address);

            List<string> localFolders = Directory.GetDirectories(Account.GetAccMailDir() + "\\" + Address, "*.*", SearchOption.AllDirectories).ToList();


            foreach (var serverFolder in serverFolders)
            {
                var strlocfold = localFolders.Find(x => x.Contains(serverFolder.FullName.Replace('|', '\\').Replace('/', '\\')));

                if (strlocfold != null)
                    localFolders.Remove(strlocfold);

                string dirFullPath = Account.GetAccMailDir() + "\\" + Address + "\\" + serverFolder.FullName.Replace('|', '\\');

                if (!Directory.Exists(dirFullPath))
                    Directory.CreateDirectory(dirFullPath);

                if (serverFolder.FullName == "[Gmail]")
                {
                    continue;
                }
                serverFolder.Open(FolderAccess.ReadOnly);

                var serverLetters = serverFolder.Fetch(0, -1, MessageSummaryItems.UniqueId | MessageSummaryItems.Flags);

                List<string> localLetters = Directory.GetFiles(dirFullPath, "*.eml").ToList();
                //List<MimeMessage> localLetters = new List<MimeMessage>();
                //foreach (var tempmsg in tempmsgs)
                //localLetters.Add(MimeMessage.Load(tempmsg));
                //localLetters.Reverse();

                foreach (var serverLetter in serverLetters)
                {
                    var uniq = serverLetter.UniqueId;
                    var flag = serverLetter.Flags.Value;
                    var tempstr = uniq.ToString() + "_" + flag.ToString();

                    var strloclet = localLetters.Find(x => x.Contains(tempstr));
                    if (strloclet != null)
                    {
                        var servlet = serverFolder.GetMessage(uniq);
                        var loclet = MimeMessage.Load(strloclet);

                        if (lettersCompare(servlet, loclet))
                        {
                            localLetters.Remove(strloclet);
                        }
                        else
                        {
                            File.Delete(Path.Combine(dirFullPath, tempstr + ".eml"));
                            servlet.WriteTo(Path.Combine(dirFullPath, tempstr + ".eml"));
                        }
                    }
                    else
                    {
                        var servlet = serverFolder.GetMessage(uniq);
                        servlet.WriteTo(Path.Combine(dirFullPath, tempstr + ".eml"));
                    }
                }
                for (int i = 0; i < localLetters.Count; i++)
                    File.Delete(localLetters[i]);
                //ResyncFolder(folder, buf, folder.UidValidity);

                serverFolder.Close();
            }

            for (int i = 0; i < localFolders.Count; i++)
                Directory.Delete(localFolders[i], true);
        }

        public List<TreeViewItem> DisplayFolders()
        {
            string dirPath = Account.GetAccMailDir() + "\\" + Address;
            if (!Directory.Exists(dirPath))
            {
                throw new Exception("Этот почтовый ящик не был синхронизирован. Для дальнейшей работы с ним попробуйте его синхронизировать.");
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

            twi.Header = Utility.panelWithIcon("folder.png", temp);

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
