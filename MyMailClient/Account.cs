using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace MyMailClient
{
    [Serializable]
    public class Account
    {
        public const string ACC_DIR = "accounts";
        const string ACC_FILE = "all.acc";
        const string INF_FILE = "acc.inf";

        /// <summary>
        /// Проверка существования директории и файла для списка всех аккаунтов
        /// </summary>
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

        public static string GetAccMailDir()
        {
            return ACC_DIR + "\\" + CurrentData.curAcc.Login;
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

        internal List<MailBox> MlBxs;
        internal List<CryptoKey> Keys;
        //ObservableCollection<Mailbox> mailboxes;
        //ObservableCollection<CryptoKey> keys;
        public Account(string l, string h)
        {
            this.Login = l;
            this.Hash = h;

            this.MlBxs = new List<MailBox>();
            this.Keys = new List<CryptoKey>();
        }

        public Account(Account acc)
        {
            if(acc==null)
                throw new Exception("Аккаунта нет!");

            this.Login = acc.Login;
            this.Hash = acc.Hash;

            this.MlBxs = new List<MailBox>(acc.MlBxs);
            this.Keys = new List<CryptoKey>(acc.Keys);
        }

        public bool ContainMailName(string newName, string exceptName)
        {
            foreach (MailBox m in MlBxs)
            {
                if (m.Name == exceptName)
                    continue;

                if (m.Name == newName)
                    return true;
            }
            return false;
        }
        public bool ContainMailAddress(string newAddress, string exceptAddres)
        {
            foreach (MailBox m in MlBxs)
            {
                if (m.Address == exceptAddres)
                    continue;

                if (m.Address == newAddress)
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

        public bool AddKey()
        {
            // TODO: вдруг ID разных ключей совпадут
            List<CryptoKey> results = Keys.Where(k => k.Id == CurrentData.curKey.Id).ToList();
            if (results.Count == 0)
            {
                Keys.Add(CurrentData.curKey);
                return true;
            }
            else
            {
                CryptoKey existing = results.First();
                if (existing.PublicOnly && !CurrentData.curKey.PublicOnly)
                {
                    int index = Keys.IndexOf(existing);
                    Keys.RemoveAt(index);
                    Keys.Insert(index, CurrentData.curKey);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
