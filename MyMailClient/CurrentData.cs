using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MimeKit;
using System.Windows;

namespace MyMailClient
{
    static class CurrentData
    {
        public static Account curAcc { get; set; }
        public static MailBox curMail { get; set; }
        public static MimeMessage curLetter { get; set; }
        public static ImapClient imap { get; set; }
        public static SmtpClient smtp { get; set; }
        //public static DataTemplate curTemplate { get; set; }
    }
}
