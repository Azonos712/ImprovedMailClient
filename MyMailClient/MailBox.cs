using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        //public void setSmtpServer(string d, int p)
        //{
        //    this.SMTP_Dom = d;
        //    this.SMTP_Port = p;
        //}

        //public void setImapServer(string d, int p)
        //{
        //    this.IMAP_Dom = d;
        //    this.IMAP_Port = p;
        //}

    }
}
