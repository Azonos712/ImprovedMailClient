using MailKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMailClient
{
    [Serializable]
    public class CachedMessageInfo
    {
        public UniqueId UniqueId { get; set; }
        public MessageFlags Flags { get; set; }
        public HashSet<string> UserFlags { get; set; }
        public Envelope Envelope { get; set; }
        public BodyPart Body { get; set; }
    }
}
