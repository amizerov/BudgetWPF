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
using Budget.BudgetPlanningControls;
using DevExpress.Xpf.Core;

namespace Budget
{
    public partial class BudgetPlanning : DXWindow
    {
        private int _userID;

        public delegate void AccountsHandler();
        public event AccountsHandler OnAccountsEdited;

        public BudgetPlanning(int userID)
        {
            InitializeComponent();

            _userID = userID;

            if (Properties.Settings.Default.ThemeIndex > 0 && Properties.Settings.Default.ThemeIndex <= Consts.DevExTheme.Count)
                ThemeManager.SetTheme(this, Consts.DevExTheme[Properties.Settings.Default.ThemeIndex]);
        }

        private void tabControlPlanning_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source == tabControlPlanning)
            {
                if (tabControlPlanning.SelectedItem == tabItemAccounts)
                {
                    var accountsControl = new AccountsPlanning(_userID);
                    accountsControl.OnAccountsEdited += () =>
                        {
                            OnAccountsEdited();
                        };
                    gridAccountsPlanning.Children.Clear();
                    gridAccountsPlanning.Children.Add(accountsControl);
                }
                else if (tabControlPlanning.SelectedItem == tabItemCategories)
                {
                    var categoriesControl = new CategoriesPlanning(_userID);
                    gridCategoriesPlanning.Children.Clear();
                    gridCategoriesPlanning.Children.Add(categoriesControl);
                }
            }
        }
    }
}
