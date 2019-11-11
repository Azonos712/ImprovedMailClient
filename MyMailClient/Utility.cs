using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MyMailClient
{
    public static class Utility
    {
        public static void MsgBox(string msg, string title, Window w)
        {
            SoundPlayer sp = new SoundPlayer();
            sp.Play();
            MyMsg mm = new MyMsg(msg, title);
            mm.ShowDialog();
            w.Focus();
            sp.Dispose();
        }
    }
}
