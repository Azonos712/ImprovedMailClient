using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Documents;
using System.Windows.Media;

namespace MyMailClient
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Account curAcc;
        //MailBox curMail;
        //ImapClient imap;

        //private static DataTemplate letterDT;
        bool noConnection = false;
        public MainWindow()
        {
            InitializeComponent();

            UpdateItemsInComboBox(listOfMails, CurrentData.curAcc.MlBxs);
            //listOfMails.ItemsSource = curAcc.MlBxs;
        }

        /// <summary>
        /// Выход из программы
        /// </summary>
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
        /// <summary>
        /// Выход из аккаунта
        /// </summary>
        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            Authorization az = new Authorization();
            az.Show();
            this.Close();
        }

        //TODO:Возможно переделать
        private void UpdateItemsInComboBox(ComboBox cb, List<MailBox> items)
        {
            //var selItem = update == true ? items[items.Count - 1] : cb.SelectedItem;
            //var selItem = cb.SelectedItem;

            cb.Items.Clear();
            foreach (MailBox m in items)
            {
                cb.Items.Add(m);
            }
            if (CurrentData.curMail != null)
                cb.SelectedItem = CurrentData.curMail;

            CountingBadge.Badge = items.Count;
        }

        /// <summary>
        /// Добавить новый почтовый ящик
        /// </summary>
        private void btn_add_Click(object sender, RoutedEventArgs e)
        {
            MailWindow mw = new MailWindow(true);
            if (mw.ShowDialog() == true)
            {
                CurrentData.curAcc.Srlz();
                noConnection = true;
                UpdateItemsInComboBox(listOfMails, CurrentData.curAcc.MlBxs);
                noConnection = false;
            }
            this.Focus();
            //listOfMails.ItemsSource = curAcc.MlBxs;
        }

        /// <summary>
        /// Изменить выбранный почтовый ящик
        /// </summary>
        private void btn_settings_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentData.curMail == null)
            {
                Utility.MsgBox("Вам стоит выбрать почтовый ящик!", "Уведомление", this);
                return;
            }

            MailWindow mw = new MailWindow(false);
            if (mw.ShowDialog() == true)
            {
                CurrentData.curMail = CurrentData.curAcc.MlBxs[CurrentData.curAcc.MlBxs.Count - 1];
                CurrentData.curAcc.Srlz();
                UpdateItemsInComboBox(listOfMails, CurrentData.curAcc.MlBxs);
            }
            this.Focus();
        }

        /// <summary>
        /// Удалить выбранный почтовый ящик
        /// </summary>
        private void btn_delete_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentData.curMail == null)
            {
                Utility.MsgBox("Вам стоит выбрать почтовый ящик!", "Уведомление", this);
                return;
            }

            CurrentData.curAcc.MlBxs.Remove(CurrentData.curMail);
            CurrentData.curMail = null;
            clearLetFol();
            CurrentData.curAcc.Srlz();
            UpdateItemsInComboBox(listOfMails, CurrentData.curAcc.MlBxs);
        }


        private void btn_synch_Click(object sender, RoutedEventArgs e)
        {
            useSynch(); 
            showFolders();
        }

        private void listOfMails_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listOfMails.SelectedItem != null)
            {
                CurrentData.curMail = listOfMails.SelectedItem as MailBox;
                //string curFMN = listOfMails.SelectedItem.ToString();
                //string temp = curFMN.Substring(curFMN.IndexOf('<') + 1, curFMN.LastIndexOf('>') - curFMN.IndexOf('<') - 1);
                //curMail = curAcc.MlBxs.Find(x => x.Address.Contains(temp));

                showFolders();

                //if (noConnection == false)
                //useSynch();

            }
        }

        void showFolders()
        {
            try
            {
                clearLetFol();

                var temp = CurrentData.curMail.DisplayFolders();
                for (int i = 0; i < temp.Count; i++)
                {
                    listOfFolders.Items.Add(temp[i]);
                }
            }
            catch (Exception ex)
            {
                Utility.MsgBox(ex.Message, "Ошибка", this);
            }
        }

        void useSynch()
        {
            try
            {
                //полная очистка ящика
                CurrentData.curMail.deleteMailFolder();

                // TODO: выполнять подключение и загрузку писем в отдельном потоке
                if (CurrentData.curMail.ImapConnection())
                    CurrentData.curMail.DownloadLetters();
                else
                    Utility.MsgBox("Что-то помешало подключению данного почтового ящика! " +
                        "Будут отображены только письма, которые были синхронизированны во время " +
                        "последнего сеанса", "Ошибка", this);

            }
            catch (Exception ex)
            {
                Utility.MsgBox(ex.Message, "Ошибка", this);
            }
        }

        //private void listOfLetters_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        //{
        //    MimeMessage message = listOfFolders.SelectedItem as MimeMessage;

        //    if (message != null)
        //    {
        //        // TODO: добавить флаг "прочитано"
        //        //showLetter(message);
        //    }
        //}

        //private void listOfLetters_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //}

        private void listOfFolders_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (listOfFolders.SelectedItem != null)
            {
                TreeViewItem item = listOfFolders.SelectedItem as TreeViewItem;

                string lastPath = Utility.strFromPanelWithIcon(item); ;

                for (var i = Utility.GetParentItem(item); i != null; i = Utility.GetParentItem(i))
                    lastPath = Utility.strFromPanelWithIcon(i) + "\\" + lastPath;

                string fullPath = Account.GetAccMailDir() + "\\" + CurrentData.curMail.Address + "\\" + lastPath;

                List<MimeMessage> msg = CurrentData.curMail.DisplayLetters(fullPath);

                //listOfLetters.DataContext = msg;
                listOfLetters.ItemsSource = msg;
            }
        }

        private void listOfLetters_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CurrentData.curLetter = listOfLetters.SelectedItem as MimeMessage;


            LetterWindow lw = new LetterWindow();
            lw.ShowDialog();

            this.Focus();
            //    if (message != null)
            //    {
            //        // TODO: добавить флаг "прочитано"
            //        //showLetter(message);
            //    }
        }

        void clearLetFol()
        {
            listOfFolders.Items.Clear();
            listOfLetters.ItemsSource = null;
        }


    }
}