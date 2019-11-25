using MimeKit;

namespace MyMailClient
{
    public class HelpMimeMessage
    {
        public bool SeenFlag {get;set;}
        public string Seen { get; set; }
        public string FullPath { get; set; }
        public MimeMessage Msg { get; set; }
    }
}
