using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MyMailClient
{
    [Serializable]
    public class CryptoKey
    {
        public string Name { get; set; }
        public string Id { get; private set; }
        public string OwnerAddress { get; private set; }
        public string PublicKey { get; private set; }
        public string PrivateKey { get; private set; }
        public bool PublicOnly { get; private set; }
        public bool EncrOrSign { get; private set; }
        public DateTime DateTime { get; private set; }

        public CryptoKey()
        {

        }
        public CryptoKey(RSACryptoServiceProvider rsa, string name, string ownerAddress) :
                this(rsa.ToXmlString(false), rsa.PublicOnly ? null : rsa.ToXmlString(true),
                name, ownerAddress, true)
        {
            ;
        }

        public CryptoKey(DSACryptoServiceProvider dsa, string name, string ownerAddress) :
                this(dsa.ToXmlString(false), dsa.PublicOnly ? null : dsa.ToXmlString(true),
                name, ownerAddress, false)
        {
            ;
        }

        internal CryptoKey(string publicKey, string privateKey, string name, string ownerAddress,
                bool purpose)
        {
            this.PublicKey = publicKey;
            this.PrivateKey = privateKey;
            this.Name = name;
            this.OwnerAddress = ownerAddress;
            this.EncrOrSign = purpose;

            this.Id = Utility.ByteArrayToHexString(Cryptography.GetSHA1(publicKey));
            this.PublicOnly = PrivateKey == null;
            this.DateTime = DateTime.Now;
        }

        public CryptoKey(string publicKey, string privateKey, string name, string ownerAddress,bool purpose, string id,DateTime time)
        {
            this.PublicKey = publicKey;
            this.PrivateKey = privateKey;
            this.Name = name;
            this.OwnerAddress = ownerAddress;
            this.EncrOrSign = purpose;
            this.Id = id;
            this.PublicOnly = PrivateKey == null;
            this.DateTime = time;
        }

        public CryptoKey GetPublicCryptoKey()
        {
            CryptoKey output = new CryptoKey(PublicKey, null, Name, OwnerAddress, EncrOrSign);
            output.DateTime = this.DateTime;
            return output;
        }
    }
}
