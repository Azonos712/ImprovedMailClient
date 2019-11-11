using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMailClient
{
    public class Account
    {
        const string ACC_DIR = "accounts";
        const string ACC_FILE = "all.acc";

        public static void CheckAccPath()
        {
            if (!Directory.Exists(ACC_DIR))
                Directory.CreateDirectory(ACC_DIR);

            if (!File.Exists(Path.Combine(ACC_DIR + "\\" + ACC_FILE)))
                File.Create(ACC_DIR + "\\" + ACC_FILE);
        }

        public static string GetAccPath()
        {
            return ACC_DIR + "\\" + ACC_FILE;
        }

        public static bool CheckAccount(string login)
        {
            if (File.Exists(GetAccPath()))
            {
                string[] allAcc = File.ReadAllLines(GetAccPath());
                for (int i = 0; i < allAcc.Length; i += 2)
                {
                    if (allAcc[i] == login)
                        return true;
                }
                return false;
            }
            return false;
        }

        public static bool CheckPassword(string login, string pass)
        {
            if (File.Exists(GetAccPath()))
            {
                string[] allAcc = File.ReadAllLines(GetAccPath());
                for (int i = 0; i < allAcc.Length; i += 2)
                {
                    if (allAcc[i] == login)
                    {
                        if (allAcc[i + 1] == Utility.ByteArrayToString(Cryptography.GetSHA1(pass)))
                            return true;
                        else
                            return false;
                    }
                }
                return false;
            }
            return false;
        }

        string login;
        //internal string digest;
        //bool useSsl;

        //ObservableCollection<Mailbox> mailboxes;
        //ObservableCollection<CryptoKey> keys;
        public Account(string login)
        {
            this.login = login;
            //this.digest = digest;

            //this.useSsl = true;
            //this.mailboxes = new ObservableCollection<Mailbox>();
            //this.keys = new ObservableCollection<CryptoKey>();
        }
    }
}
