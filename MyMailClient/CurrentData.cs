using MailKit.Net.Imap;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MyMailClient
{
    static class CurrentData
    {
        public static Account curAcc { get; set; }
        public static MailBox curMail { get; set; }
        public static MimeMessage curLetter { get; set; }
        public static ImapClient imap { get; set; }
        public static DataTemplate curTemplate { get; set; }
    }
}
