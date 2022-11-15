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
    public partial class AddCategoryWindow : DXWindow
    {
        private int _userID;
        private int _categoryID;
        private bool _dontClose;

        public new delegate void AddHandler();
        public event AddHandler WasAdded;
        public event AddHandler WasEdited;

        public AddCategoryWindow(int userID, int categoryID)
        {
            InitializeComponent();

            _userID = userID;
            _categoryID = categoryID;
            if (_categoryID != -1)
            {  //обновление
                var dt = G.db_select("exec GetCategory {1}", _categoryID);
                txtName.Text = dt.Rows[0]["Name"].ToString();
                txtDebetLimit.Text = String.Format("{0:0.00}", dt.Rows[0]["Limit"]);
                txtCreditPlan.Text = String.Format("{0:0.00}", dt.Rows[0]["Plan"]);
                upDownFirstDay.Text = dt.Rows[0]["FirstDay"].ToString();
                if (String.IsNullOrEmpty(upDownFirstDay.Text))
                    upDownFirstDay.Value = 1;
            }

            _dontClose = false;
            CollapseAll();

            if (Properties.Settings.Default.ThemeIndex > 0 && Properties.Settings.Default.ThemeIndex <= Consts.DevExTheme.Count)
                ThemeManager.SetTheme(this, Consts.DevExTheme[Properties.Settings.Default.ThemeIndex]);
        }

        private void ExpandAll()
        {
            this.Height += 90;
        }

        private void CollapseAll()
        {
            this.Height -= 90;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddCategory();
        }

        /// <summary>
        /// Добавить категорию
        /// </summary>
        private void AddCategory()
        {
            _dontClose = false;

            if (!String.IsNullOrEmpty(txtName.Text))
            {
                if (_categoryID == -1)
                {
                    _categoryID = Convert.ToInt32(G._S(G.db_select("AddCategory '{1}', {2}", txtName.Text, _userID)));
                    CheckDB(G.LastError);

                    UpdateLimitPlanFirstDay();

                    if (!_dontClose)
                        WasAdded();
                }
                else
                {
                    G.db_exec("UpdateCategoryName {1}, '{2}'", _categoryID, txtName.Text);
                    CheckDB(G.LastError);

                    UpdateLimitPlanFirstDay();

                    if (!_dontClose)
                        WasEdited();
                }

                if (!_dontClose)
                    Close();
            }
        }

        /// <summary>
        /// Обновить лимит, план и первый день
        /// </summary>
        private void UpdateLimitPlanFirstDay()
        {
            //Обновление лимита расходов
            if (!String.IsNullOrEmpty(txtDebetLimit.Text))
            {
                double limit = default(double);
                Double.TryParse(txtDebetLimit.Text.Replace('.', ','), out limit);

                if (limit == default(double))
                {
                    MessageBox.Show("Неверный формат лимита расходов", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    _dontClose = true;
                }
                else
                {
                    G.db_exec("UpdateCategoryLimit {1}, {2}", _categoryID, limit.ToString().Replace(',', '.'));
                    CheckDB(G.LastError);
                }
            }
            else
            {
                G.db_exec("UpdateCategoryLimit {1}, NULL", _categoryID);
                CheckDB(G.LastError);
            }

            //Обновление плана доходов
            if (!String.IsNullOrEmpty(txtCreditPlan.Text))
            {
                double plan = default(double);
                Double.TryParse(txtCreditPlan.Text.Replace('.', ','), out plan);

                if (plan == default(double))
                {
                    MessageBox.Show("Неверный формат плана доходов", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    _dontClose = true;
                }
                else
                {
                    G.db_exec("UpdateCategoryPlan {1}, {2}", _categoryID, plan.ToString().Replace(',', '.'));
                    CheckDB(G.LastError);
                }
            }
            else
            {
                G.db_exec("UpdateCategoryPlan {1}, NULL", _categoryID);
                CheckDB(G.LastError);
            }

            //Обновление первого дня
            var firstDay = 1;
            if (!String.IsNullOrEmpty(upDownFirstDay.Text))
                firstDay = (int)upDownFirstDay.Value;
            G.db_exec("UpdateCategoryFirstDay {1}, {2}", _categoryID, firstDay);
            CheckDB(G.LastError);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
            else if (e.Key == Key.Enter)
                AddCategory();
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtName.Focus();
            txtName.SelectionStart = txtName.Text.Length;
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
