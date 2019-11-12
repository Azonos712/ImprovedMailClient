using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMailClient
{
    class MailBox
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Pass { get; set; }
        public string SMTP_Dom { get; set; }
        public int SMTP_Port { get; set; }
        public string IMAP_Dom { get; set; }
        public int IMAP_Port { get; set; }

        public MailBox(string n, string a, string p)
        {
            this.Name = n;
            this.Address = a;
            this.Pass = p;
        }

        public void setSmtpServer(string d, int p)
        {
            this.SMTP_Dom = d;
            this.SMTP_Port = p;
        }

        public void setImapServer(string d, int p)
        {
            this.IMAP_Dom = d;
            this.IMAP_Port = p;
        }
    }
}
