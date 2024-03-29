﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

        public static byte[] StringToByte(string input)
        {
            return System.Convert.FromBase64String(input);
        }

        public static string ByteArrayToHexString(byte[] input)
        {
            StringBuilder output = new StringBuilder();
            foreach (byte x in input)
                output.Append(string.Format("{0:x2}", x));
            return output.ToString();
        }
        public static byte[] HexStringToByteArray(string hex)
        {
            if (hex.Length % 2 > 0)
                throw new FormatException("Строка должна иметь чётное число символов");
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static string ColorToHexString(Color c)
        {
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
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

        public static StackPanel panelWithIcon(string image,string text)
        {
            StackPanel pan = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            PngBitmapDecoder icon = new PngBitmapDecoder(new Uri("Resources\\" + image,
                UriKind.RelativeOrAbsolute), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            Image img = new Image
            {
                Height = 19,
                Width = 19,
                Source = icon.Frames[0]
            };
            pan.Children.Add(img);

            pan.Children.Add(new TextBlock(new Run("  " + text)));

            return pan;
        }

        public static string strFromPanelWithIcon(TreeViewItem twi)
        {
            StackPanel sp = twi.Header as StackPanel;
            TextBlock tb = sp.Children[1] as TextBlock;
            Inline ic = tb.Inlines.ElementAt(0);
            TextRange tr = new TextRange(ic.ContentStart, ic.ContentEnd);
            string str = tr.Text.Trim();

            return str;
        }

        public static TreeViewItem GetParentItem(TreeViewItem item)
        {
            for (var i = VisualTreeHelper.GetParent(item); i != null; i = VisualTreeHelper.GetParent(i))
                if (i is TreeViewItem)
                    return (TreeViewItem)i;

            return null;
        }

        public static string CutEndOfPathFolder(string str)
        {
            if (new Regex(@"^[A-Za-zА-Яа-я0-9- ]+ \(\d+\)$").IsMatch(str))
                str = str.Substring(0, str.Length - (str.Length - str.LastIndexOf(' ')));
            return str;
        }

        public static void SaveAttachment(MimeKit.MimeEntity attachment, string filepath)
        {
            using (var stream = System.IO.File.Create(filepath))
            {
                if (attachment is MimeKit.MessagePart)
                {
                    var rfc822 = (MimeKit.MessagePart)attachment;
                    rfc822.Message.WriteTo(stream);
                }
                else
                {
                    var part = (MimeKit.MimePart)attachment;
                    part.Content.DecodeTo(stream);
                }
            }
        }

        public static MessageBoxResult ShowConfirmation(string message)
        {
            return MessageBox.Show(message, "Подтвердите действие", MessageBoxButton.YesNo, MessageBoxImage.Question);
        }
    }
}
