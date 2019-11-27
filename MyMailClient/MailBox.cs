using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MyMailClient
{
    [Serializable]
    public class MailBox
    {

        public const string DEFAULT_SMTP_SUBDOMAIN = "smtp.";
        public const string DEFAULT_SMTP_PORT = "465";
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

                if (File.Exists("Logs\\imap.log"))
                    File.Delete("Logs\\imap.log");

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

                //List<string> templocalLetters = new List<string>(localLetters);
                //templocalLetters.ForEach(x =>x.Substring(x.LastIndexOf("\\") + 1, x.LastIndexOf("_") - x.LastIndexOf("\\") - 1));
                //List<string> tst = templocalLetters.Select(x => x.Substring(x.LastIndexOf("\\") + 1, x.LastIndexOf("_") - x.LastIndexOf("\\") - 1)).ToList();

                List<uint> UIDlocalLetters = new List<uint>(localLetters.Select(x => x.Substring(x.LastIndexOf("\\") + 1,
                    x.LastIndexOf("_") - x.LastIndexOf("\\") - 1))
                    .ToList().Select(uint.Parse).ToList());

                //foreach (var x in localLetters)
                //{
                //    var temp = x.Substring(x.LastIndexOf("\\") + 1, x.LastIndexOf("_") - x.LastIndexOf("\\") - 1);
                //    UIDlocalLetters.Add(Convert.ToUInt32(temp));
                //}

                foreach (var serverLetter in serverLetters)
                {
                    var uniq = serverLetter.UniqueId;
                    var flag = serverLetter.Flags.Value;
                    var tempstr = uniq.ToString() + "_" + flag.ToString();

                    //foreach(localLetter)
                    //localLetters.
                    //List<string> templocalLetters = new List<string>(localLetters);
                    //localLetters.ForEach(x =>x.Substring(x.LastIndexOf("\\"),x.LastIndexOf("_")- x.LastIndexOf("\\")));
                    
                    int index = UIDlocalLetters.FindIndex(x => x == uniq.Id);

                    //var strloclet = localLetters.Find(x => x.Contains(uniq.ToString()));
                    //if (strloclet != null)
                    if(index!=-1)
                    {
                        var strloclet = localLetters[index];
                        if (!strloclet.Contains(flag.ToString()))
                        {
                            string newstrlocletflag = (strloclet.Substring(0, strloclet.LastIndexOf("\\") + 1) + tempstr + ".eml");

                            File.Move(strloclet, newstrlocletflag);
                        }

                        //var servlet = serverFolder.GetMessage(uniq);
                        //var loclet = MimeMessage.Load(strloclet);

                        //if (lettersCompare(servlet, loclet))
                        //{
                        localLetters.Remove(strloclet);
                        UIDlocalLetters.RemoveAt(index);
                        //}
                        //else
                        //{
                        //    File.Delete(Path.Combine(dirFullPath, tempstr + ".eml"));
                        //    servlet.WriteTo(Path.Combine(dirFullPath, tempstr + ".eml"));
                        //}
                    }
                    else
                    {
                        var servlet = serverFolder.GetMessage(uniq);
                        servlet.WriteTo(Path.Combine(dirFullPath, tempstr + ".eml"));
                    }
                }
                for (int i = 0; i < localLetters.Count; i++)
                    File.Delete(localLetters[i]);

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

            string[] messages = Directory.GetFiles(pathFile, "*.eml");

            string temp = (pathFile.Substring(pathFile.LastIndexOf('\\') + 1)) + (messages.Length > 0 ?
                    (" (" + messages.Length + ")") : "");

            //List<TreeViewItem> twi = new List<TreeViewItem>();
            //twi.Add(new TreeViewItem());

            TreeViewItem twi = new TreeViewItem();

            twi.Header = Utility.panelWithIcon("folder.png", temp);
            foreach (string subdirPath in Directory.GetDirectories(pathFile))
                twi.Items.Add(DisplayFolder(subdirPath));

            return twi;
        }

        public List<HelpMimeMessage> DisplayLetters(string pathFolder)
        {
            //string[] messages = Directory.GetFiles(pathFolder, "*.eml");
            List<string> messages = Directory.GetFiles(pathFolder, "*.eml").ToList();
            messages.Sort(new NaturalStringComparer());

            List<HelpMimeMessage> buf = new List<HelpMimeMessage>();

            foreach (string message in messages)
            {
                HelpMimeMessage temp = new HelpMimeMessage();
                temp.Msg = MimeMessage.Load(message);
                temp.SeenFlag = message.Contains("Seen") ? true : false;
                temp.Seen = message.Contains("Seen") ? new FileInfo("Resources\\empty_mail.png").FullName : new FileInfo("Resources\\full_mail.png").FullName;
                temp.FullPath = message;
                buf.Add(temp);
            }

            buf.Reverse();

            return buf;
        }

        public bool markLetter(string fullLetterPath, string fullfolderPath)
        {
            string letterName = new FileInfo(fullLetterPath).Name;
            if (!letterName.Contains("Seen"))
            {
                if ((CurrentData.curMail.ImapConnection()))
                {
                    var uid_flag = letterName.Split('_');

                    var serverFolder = CurrentData.imap.GetFolder(fullfolderPath);
                    
                    serverFolder.Open(FolderAccess.ReadWrite);
                    List<UniqueId> uids = new List<UniqueId>();
                    uids.Add(new UniqueId(Convert.ToUInt32(uid_flag[0])));
                    var uid = serverFolder.Fetch(uids, MessageSummaryItems.UniqueId | MessageSummaryItems.Flags);
                    serverFolder.AddFlags(uid[0].UniqueId, MessageFlags.Seen, true);

                    var serverLetter = serverFolder.Fetch(uids, MessageSummaryItems.UniqueId | MessageSummaryItems.Flags);
                    
                    serverFolder.Close();
                    ImapDispose();
                    
                    string newLetterName = serverLetter[0].UniqueId.ToString() + "_" + serverLetter[0].Flags.Value.ToString();
                    string newFullLetterPath = fullLetterPath.Substring(0, fullLetterPath.LastIndexOf("\\")+1) + newLetterName + ".eml";
                    File.Move(fullLetterPath, newFullLetterPath);
                }
            }
            return true;
        }

        public bool sendMessage(MimeMessage msg)
        {
            CurrentData.smtp = new SmtpClient();
            CurrentData.smtp.Connect(SMTP_Dom, SMTP_Port, true);
            CurrentData.smtp.Authenticate(Address, Pass);
            CurrentData.smtp.Send(msg);
            CurrentData.smtp.Disconnect(true);

            ImapConnection();
            var sentFolder = CurrentData.imap.GetFolder(SpecialFolder.Sent);
            sentFolder.Append(msg, MessageFlags.Seen, DateTimeOffset.Now);
            ImapDispose();

            return true;
        }
    }


    [SuppressUnmanagedCodeSecurity]
    internal static class NativeMethods
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern int StrCmpLogicalW(string psz1, string psz2);
    }

    public sealed class NaturalStringComparer : IComparer<string>
    {
        public int Compare(string a, string b)
        {
            return NativeMethods.StrCmpLogicalW(a, b);
        }
    }

    public sealed class NaturalFileInfoNameComparer : IComparer<FileInfo>
    {
        public int Compare(FileInfo a, FileInfo b)
        {
            return NativeMethods.StrCmpLogicalW(a.Name, b.Name);
        }
    }


}
