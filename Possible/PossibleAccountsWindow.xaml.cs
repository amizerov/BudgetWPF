using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DevExpress.Xpf.Core;
using am.BL;
using System.ComponentModel;
using System.Data;

namespace Budget
{
    public partial class PossibleAccountsWindow : DXWindow
    {
        private int _userID;

        public delegate void AccountHandler();
        public event AccountHandler OnAccountsAdded;

        public PossibleAccountsWindow(int userID)
        {
            InitializeComponent();

            _userID = userID;

            if (Properties.Settings.Default.ThemeIndex > 0 && Properties.Settings.Default.ThemeIndex <= Consts.DevExTheme.Count)
                ThemeManager.SetTheme(this, Consts.DevExTheme[Properties.Settings.Default.ThemeIndex]);

            FillMyAccounts();
            FillPossibleAccounts();
        }

        private void FillPossibleAccounts()
        {
            var dt = G.db_select("exec GetPossibleAccounts {1}", _userID);
            listBoxPossibleAccounts.ItemsSource = ((IListSource)dt).GetList();
            listBoxPossibleAccounts.ValueMember = "Name";
            listBoxPossibleAccounts.DisplayMember = "Name";
        }

        private void FillMyAccounts()
        {
            var dt = G.db_select("exec GetAccountsList {1}", _userID);
            listBoxMyAccounts.ItemsSource = ((IListSource)dt).GetList();
            listBoxMyAccounts.ValueMember = "Name";
            listBoxMyAccounts.DisplayMember = "Name";
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            foreach (var cat in listBoxPossibleAccounts.SelectedItems)
            {
                var accName = (cat as DataRowView).Row.ItemArray.First();
                G.db_exec("AddAccount '{1}', {2}", accName, _userID);
            }

            if (listBoxPossibleAccounts.SelectedItems.Count > 0)
            {
                FillMyAccounts();
                FillPossibleAccounts();

                OnAccountsAdded();
            }
        }

        private void btnRemove_Click(object sender, MouseButtonEventArgs e)
        {
            foreach (var acc in listBoxMyAccounts.SelectedItems)
            {
                var accName = (acc as DataRowView).Row.ItemArray[1];
                G.db_exec("RemoveAccount '{1}', {2}", accName, _userID);
            }

            //if (listBoxPossibleAccounts.SelectedItems.Count > 0)
            {
                FillMyAccounts();
                FillPossibleAccounts();

                OnAccountsAdded();
            }
        }

        private void AddCheckButton_Click(object sender, RoutedEventArgs e)
        {
            var w = new SetAccountWindow(-1, _userID);
            w.Owner = this;
            if (w.ShowDialog() == true)
            {
                FillMyAccounts();

                OnAccountsAdded();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void listBoxPossibleAccounts_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            btnAdd_Click(null, null);
        }

        private void listBoxMyAccounts_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            btnRemove_Click(null, null);
        }
    }
}
