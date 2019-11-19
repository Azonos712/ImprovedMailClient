using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace MyMailClient
{
    public static class Utility
    {
        public static string HTML_CHARSET = "utf-8";
        public static void MsgBox(string msg, string title, Window w)
        {
            SoundPlayer sp = new SoundPlayer();
            sp.Play();
            MyMsg mm = new MyMsg(msg, title);
            mm.ShowDialog();
            w.Focus();
            sp.Dispose();
        }

        public static string ByteArrayToString(byte[] input)
        {
            return System.Convert.ToBase64String(input);
        }

        public static bool ValidateEmail(this string s)
        {
            try
            {   
                MailAddress m = new MailAddress(s);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
