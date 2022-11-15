using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Editors;
using am.BL;

namespace Budget
{
    public partial class RecalcWindow : DXWindow
    {
        private int _userID;
        //private int _accountID;

        public RecalcWindow(int userID)
        {
            InitializeComponent();
            _userID = userID;

            FillAccounts();
            dpFrom.BeginInit();
            dpTo.BeginInit();
            dpFrom.EditValue = DateTime.Now;
            dpTo.EditValue = DateTime.Now;
            dpFrom.EndInit();
            dpTo.EndInit();

            DateTime fromDate, toDate;
            Utils.RestoreRecalcDates(out fromDate, out toDate);
            dpFrom.EditValue = fromDate;
            dpTo.EditValue = toDate;

            if (Properties.Settings.Default.ThemeIndex > 0 && Properties.Settings.Default.ThemeIndex <= Consts.DevExTheme.Count)
                ThemeManager.SetTheme(this, Consts.DevExTheme[Properties.Settings.Default.ThemeIndex]);
        }

        private void FillAccounts()
        {
            var dt = G.db_select("exec GetAccountsList {1}", _userID);
            CheckDB(G.LastError);
            comboBoxAccount.ItemsSource = ((IListSource)dt).GetList();
            comboBoxAccount.ValueMember = "ID";
            comboBoxAccount.DisplayMember = "Name";

            if (comboBoxAccount.ItemsSource != null)
                comboBoxAccount.SelectedIndex = 0;
        }

        private void CheckDB(string LE)
        {
            if (LE.Length > 0)
            {
                MessageBox.Show(LE,
                                "Ошибка в базе данных",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return;
            }
        }

        private void btnRecalc_Click(object sender, RoutedEventArgs e)
        {
            var fromDate = dpFrom.DateTime.ToString("yyyyMMdd");
            var toDate = dpTo.DateTime.ToString("yyyyMMdd");

            G.db_exec("am_CloseAccOperDay {1}, '{2}', '{3}'", comboBoxAccount.EditValue, fromDate, toDate);
            CheckDB(G.LastError);

            if (String.IsNullOrEmpty(G.LastError))
                this.Close();
        }

        private void AccountsDownDatePicker_SelectedDateChanged(object sender, EditValueChangedEventArgs editValueChangedEventArgs)
        {
            var d1 = (DateTime)dpFrom.EditValue;
            var d2 = (DateTime)dpTo.EditValue;
            if (d1 > d2) dpTo.EditValue = d1;
        }

        private void AccountsUpDatePicker_SelectedDateChanged(object sender, EditValueChangedEventArgs editValueChangedEventArgs)
        {
            var d1 = (DateTime)dpFrom.EditValue;
            var d2 = (DateTime)dpTo.EditValue;
            if (d1 > d2) dpFrom.EditValue = d2;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DXWindow_Closing(object sender, CancelEventArgs e)
        {
            Utils.SaveRecalcDates((DateTime)dpFrom.EditValue, (DateTime)dpTo.EditValue);
        }
    }
}
