using MailKit.Net.Imap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMailClient
{
    static class CurrentData
    {
        public static Account curAcc { get; set; }
        public static MailBox curMail { get; set; }
        public static ImapClient imap { get; set; }
    }
}
