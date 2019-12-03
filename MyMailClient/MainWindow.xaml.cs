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
using System.Threading.Tasks;

namespace MyMailClient
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
            CurrentData.curAcc = null;
            CurrentData.curKey = null;
            CurrentData.curLetter = null;
            CurrentData.curMail = null;
            if (CurrentData.imap != null)
            {
                if (CurrentData.imap.IsConnected)
                    CurrentData.imap.Disconnect(false);
                CurrentData.imap.Dispose();
                CurrentData.imap = null;
            }
            CurrentData.smtp = null;
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
            if (CurrentData.curMail == null)
            {
                Utility.MsgBox("Вам стоит выбрать почтовый ящик!", "Уведомление", this);
                return;
            }
            startSynchronization();
        }

        void lockForm()
        {
            blackPanel.Margin = new Thickness(0, 0, 0, 0);
            progress.Margin = new Thickness(0, 0, 0, 0);
            status.Margin = new Thickness(0, 0, 0, 0);
        }
        void unlockForm()
        {
            blackPanel.Margin = new Thickness(1200, 0, 0, 0);
            progress.Margin = new Thickness(1200, 0, 0, 0);
            status.Margin = new Thickness(1200, 0, 0, 0);
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
            }
        }

        async void startSynchronization()
        {
            lockForm();
            var temp = await Task.Run(() => useSynch());
            showFolders();
            unlockForm();
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

        bool useSynch()
        {
            try
            {
                // TODO: выполнять подключение и загрузку писем в отдельном потоке
                if (CurrentData.curMail.ImapConnection())
                {
                    CurrentData.curMail.StartResync(); //CurrentData.curMail.DownloadLetters();////
                }
                else
                {
                    Dispatcher.Invoke((Action)(() =>
                    {
                        Utility.MsgBox("Что-то помешало подключению данного почтового ящика! " +
                        "Будут отображены только письма, которые были синхронизированны во время " +
                        "последнего сеанса", "Ошибка", this);
                    }));
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    Utility.MsgBox(ex.Message, "Ошибка", this);
                }));
            }
            return true;

        }

        private void listOfFolders_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (listOfFolders.SelectedItem != null)
            {
                refreshListOfLetters();
            }
        }

        void refreshListOfLetters()
        {
            TreeViewItem item = listOfFolders.SelectedItem as TreeViewItem;

            string lastPath = Utility.strFromPanelWithIcon(item);
            lastPath = Utility.CutEndOfPathFolder(lastPath);
            for (var i = Utility.GetParentItem(item); i != null; i = Utility.GetParentItem(i))
                lastPath = Utility.CutEndOfPathFolder(Utility.strFromPanelWithIcon(i)) + "\\" + lastPath;

            string fullPath = Account.GetAccMailDir() + "\\" + CurrentData.curMail.Address + "\\" + lastPath;

            List<HelpMimeMessage> msg = CurrentData.curMail.DisplayLetters(fullPath);

            //listOfLetters.DataContext = msg;
            listOfLetters.ItemsSource = msg;
        }

        private async void listOfLetters_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CurrentData.curLetter = (listOfLetters.SelectedItem as HelpMimeMessage).Msg;
            if (CurrentData.curLetter != null)
            {
                LetterWindow lw = new LetterWindow();
                lw.ShowDialog();

                lockForm();

                TreeViewItem item = listOfFolders.SelectedItem as TreeViewItem;
                string fullfolderPath = Utility.strFromPanelWithIcon(item);
                fullfolderPath = Utility.CutEndOfPathFolder(fullfolderPath);
                for (var i = Utility.GetParentItem(item); i != null; i = Utility.GetParentItem(i))
                    fullfolderPath = Utility.CutEndOfPathFolder(Utility.strFromPanelWithIcon(i)) + "\\" + fullfolderPath;
                var tempPath = (listOfLetters.SelectedItem as HelpMimeMessage).FullPath;
                var temp = await Task.Run(() => CurrentData.curMail.markLetter(tempPath, fullfolderPath));

                refreshListOfLetters();
                unlockForm();
                //startSynchronization();
            }
            this.Focus();
        }

        void clearLetFol()
        {
            listOfFolders.Items.Clear();
            listOfLetters.ItemsSource = null;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentData.curMail == null)
            {
                Utility.MsgBox("Вам стоит выбрать почтовый ящик!", "Уведомление", this);
                return;
            }
            WriteLetterWindow wlw = new WriteLetterWindow();
            if (wlw.ShowDialog().Value)
                startSynchronization();
            this.Focus();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            KeysManagerWindow kmw = new KeysManagerWindow();
            kmw.ShowDialog();
            this.Focus();
        }

        private void btn_reply_Click(object sender, RoutedEventArgs e)
        {
            if (listOfLetters.SelectedItem == null)
            {
                Utility.MsgBox("Вам стоит выбрать письмо, что бы ответить на него!", "Уведомление", this);
                return;
            }
            CurrentData.curLetter = (listOfLetters.SelectedItem as HelpMimeMessage).Msg;
            if (CurrentData.curLetter == null)
            {
                Utility.MsgBox("Что-то пошло не так, письмо всёравно не выбранно!", "Уведомление", this);
                return;
            }
            WriteLetterWindow wlw = new WriteLetterWindow(null,true);
            if (wlw.ShowDialog().Value)
                startSynchronization();
            this.Focus();
        }

        private void btn_dellet_Click(object sender, RoutedEventArgs e)
        {

        }


        
    }
}