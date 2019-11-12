using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace MyMailClient
{
    [Serializable]
    public class Account
    {
        const string ACC_DIR = "accounts";
        const string ACC_FILE = "all.acc";
        const string INF_FILE = "acc.inf";

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

        public static string GetAccInfoPath(string l)
        {
            return ACC_DIR + "\\" + l + "\\" + INF_FILE;
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

        public string Login { get; set; }
        public string Hash { get; set; }

        //bool useSsl;
        internal List<MailBox> MlBxs;
        //ObservableCollection<Mailbox> mailboxes;

        //ObservableCollection<CryptoKey> keys;
        public Account(string l, string h)
        {
            this.Login = l;
            this.Hash = h;

            //this.useSsl = true;
            this.MlBxs = new List<MailBox>();
            //this.keys = new ObservableCollection<CryptoKey>();
        }

        public Account(Account acc)
        {
            this.Login = acc.Login;
            this.Hash = acc.Hash;

            //this.useSsl = true;
            this.MlBxs = new List<MailBox>(acc.MlBxs);
            //this.keys = new ObservableCollection<CryptoKey>();
        }

        public bool ContainMailName(string newName)
        {
            foreach (MailBox m in MlBxs)
            {
                if (m.Name == newName)
                    return true;
            }
            return false;
        }

        public bool ContainMailAddress(string newName)
        {
            foreach (MailBox m in MlBxs)
            {
                if (m.Address == newName)
                    return true;
            }
            return false;
        }

        public void Srlz()
        {
            Directory.CreateDirectory(Account.ACC_DIR + "\\" + Login);
            using (FileStream fstream = File.Open(Account.GetAccInfoPath(Login), FileMode.Create))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fstream, this);
            }
        }
        public static Account Dsrlz(string l)
        {
            Account acc = null;
            using (FileStream fstream = File.Open(Account.GetAccInfoPath(l), FileMode.Open))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                acc = binaryFormatter.Deserialize(fstream) as Account;
            }
            return acc;
        }
    }
}
