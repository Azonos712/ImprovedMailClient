using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyMailClient
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Account curAcc;
        public MainWindow(Account profile)
        {
            InitializeComponent();
            curAcc = new Account(profile);

            UpdateItemsInComboBox(listOfMails, curAcc.MlBxs);
            //listOfMails.ItemsSource = curAcc.MlBxs;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MailWindow mw = new MailWindow(curAcc);
            mw.ShowDialog();
            this.Focus();
            curAcc.Srlz();
            UpdateItemsInComboBox(listOfMails, curAcc.MlBxs);
            //listOfMails.ItemsSource = curAcc.MlBxs;
        }

        //TODO:Возможно переделать
        private void UpdateItemsInComboBox(ComboBox cb, List<MailBox> items)
        {
            var selItem = cb.SelectedItem;
            cb.Items.Clear();
            foreach (MailBox m in items)
            {
                cb.Items.Add(m);
            }
            if (selItem != null && cb.Items.Contains(selItem))
                cb.SelectedItem = selItem;

            CountingBadge.Badge = items.Count;
        }
    }
}
