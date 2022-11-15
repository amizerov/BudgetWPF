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

namespace Budget
{
    public partial class TransferWindow : DXWindow
    {
        public enum Action
        {
            Debet = 0,
            Credit
        }

        private int _userID;
        private int _accountID;
        private Action _action;

        public TransferWindow(int userID, int accountID, Action action)
        {
            InitializeComponent();

            _userID = userID;
            _accountID = accountID;
            _action = action;

            if (_action == Action.Credit)
            {
                lblFromAccount.Content = "Счет зачисления:";
                this.Title = "Зачисление средств";
                lblTransfer.Content = "Счет для списания:";
                txtExecute.Text = "Зачислить";
            }
            else if (_action == Action.Debet)
            {
                lblFromAccount.Content = "Счет списания:";
                this.Title = "Списание средств";
                lblTransfer.Content = "Счет для зачисления:";
                txtExecute.Text = "Списать";
            }

            lblFromAccountName.Content = G._S(G.db_select("exec GetAccountNameByID {1}", _accountID));

            var dt = am.BL.G.db_select("exec GetAccountsListButCurrent {1}, {2}", _accountID, _userID);
            if (G.LastError.Length > 0)
            {
                MessageBox.Show(G.LastError,
                                "Ошибка в базе данных",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return;
            }

            accountsComboBox.ItemsSource = ((IListSource)dt).GetList();
            accountsComboBox.SelectedText = "ID";
            accountsComboBox.DisplayMember = "Name";

            if (accountsComboBox.Items.Count > 0)
                accountsComboBox.SelectedIndex = 0;

            if (Properties.Settings.Default.ThemeIndex > 0 && Properties.Settings.Default.ThemeIndex <= Consts.DevExTheme.Count)
                ThemeManager.SetTheme(this, Consts.DevExTheme[Properties.Settings.Default.ThemeIndex]);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }

        private void btnCarryOut_Click(object sender, RoutedEventArgs e)
        {
            CarryOut();
        }

        /// <summary>
        /// Провести перевод
        /// </summary>
        private void CarryOut()
        {
            double sum = -1;
            try
            { sum = Convert.ToDouble(txtSum.Text); }
            catch { }

            if (sum > 0)
            {
                //провести операцию
                if (_action == Action.Credit)
                {
                    G.db_exec("Transfer {1}, {2}, {3}, NULL, NULL", accountsComboBox.SelectedItem, _accountID, sum);
                    CheckDB(G.LastError);
                }
                else if (_action == Action.Debet)
                {
                    G.db_exec("Transfer {1}, {2}, {3}, NULL, NULL", _accountID, accountsComboBox.SelectedItem, sum);
                    CheckDB(G.LastError);
                }

                if (G.LastError.Length > 0)
                {
                    MessageBox.Show(G.LastError,
                                    "Ошибка в базе данных",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                    return;
                }

                this.DialogResult = true;
                Close();
            }
            else if (sum == 0)
            {
                MessageBox.Show("Сумма должна быть больше 0.",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
            else
                MessageBox.Show("Указана неверная сумма. Невозможно провести  операцию.",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
        }

        private void txtSum_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                CarryOut();
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
    }
}
