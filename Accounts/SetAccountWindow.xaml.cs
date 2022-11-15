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
using am.BL;
using System.Data;
using DevExpress.Xpf.Core;

namespace Budget
{
    public partial class SetAccountWindow : DXWindow
    {
        private int _userID;
        private int _accountID;
        //private bool _accountWasAdded;

        public SetAccountWindow(int accountID, int userID)
        {
            InitializeComponent();
            _accountID = accountID;
            _userID = userID;
            //_accountWasAdded = false;

            if (_accountID > 0)
            {
                this.Title = "Редактирование счета";
                DataRow r;
                var dt = G.db_select("GetAccount {1}", _accountID);
                if (dt.Rows.Count == 0) {
                    MessageBox.Show("Error: " + G.LastError);
                    return;
                }

                r = dt.Rows[0];

                txtName.Text = r["Name"].ToString();
                txtDebetLimit.Text = r["Limit"].ToString() != String.Empty ? String.Format("{0:0.00}", r["Limit"]) : String.Empty;
                txtCreditPlan.Text = r["Plan"].ToString() != String.Empty ? String.Format("{0:0.00}", r["Plan"]) : String.Empty;
                upDownFirstDay.Text = r["FirstDay"].ToString();
                chbIsMinusAllowed.IsChecked = G._B(r["IsMinusAllowed"]);
            }
            else
                this.Title = "Добавление счета";

            if (Properties.Settings.Default.ThemeIndex > 0 && Properties.Settings.Default.ThemeIndex <= Consts.DevExTheme.Count)
                ThemeManager.SetTheme(this, Consts.DevExTheme[Properties.Settings.Default.ThemeIndex]);

            CollapseAll();
        }

        private void ExpandAll()
        {
            this.Height += 90;
        }

        private void CollapseAll()
        {
            this.Height -= 90;
        }

        //Добавить счет
        private void AddAccount()
        {
            if (!String.IsNullOrEmpty(txtName.Text))
            {
                double limit = default(double), plan = default(double);
                Double.TryParse(txtDebetLimit.Text.Replace('.', ','), out limit);
                Double.TryParse(txtCreditPlan.Text.Replace('.', ','), out plan);
                var firstDay = 1;
                if (!String.IsNullOrEmpty(upDownFirstDay.Text)) firstDay = (int)upDownFirstDay.Value;

                if (_accountID > 0)
                {  //редактирование
                    G.db_select("exec UpdateAccount {1}, '{2}', {3}, {4}, {5}, {6}",
                                _accountID,
                                txtName.Text,
                                limit != default(double) ? limit.ToString() : "NULL",
                                plan != default(double) ? plan.ToString() : "NULL",
                                firstDay, (bool)chbIsMinusAllowed.IsChecked ? 1 : 0);
                }
                else
                {  //добавление
                    G.db_select("exec AddAccount '{1}', {2}, {3}, {4}, {5}, {6}",
                                txtName.Text,
                                _userID,
                                limit != default(double) ? limit.ToString() : "NULL",
                                plan != default(double) ? plan.ToString() : "NULL",
                                firstDay, (bool)chbIsMinusAllowed.IsChecked ? 1 : 0);
                }

                CheckDB(G.LastError);
                if (String.IsNullOrEmpty(G.LastError))
                {
                    this.DialogResult = true;
                    this.Close();
                }
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
            else if (e.Key == Key.Enter)
                AddAccount();
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

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddAccount();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtName.Focus();
        }

        private void expanderExtraInfo_Collapsed(object sender, RoutedEventArgs e)
        {
            CollapseAll();
        }

        private void expanderExtraInfo_Expanded(object sender, RoutedEventArgs e)
        {
            ExpandAll();
        }
    }
}
