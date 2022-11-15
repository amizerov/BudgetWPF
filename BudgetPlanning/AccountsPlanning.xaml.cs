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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Timers;
using System.Windows.Threading;
using System.Threading;
using Budget.Model;
using am.BL;

namespace Budget.BudgetPlanningControls
{
    public partial class AccountsPlanning : UserControl
    {
        private int _userID;
        private int _selectedAccountID;
        private System.Timers.Timer _timer;

        public delegate void AccountHandler();
        public event AccountHandler OnAccountsEdited;

        public AccountsPlanning(int userID)
        {
            InitializeComponent();
            _userID = userID;
            _selectedAccountID = -1;

            _timer = new System.Timers.Timer(10000) { Interval = 200, Enabled = false };
            _timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);

            ResizeGridAndFillCategories();
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate
            {
                ResizeGridAndFillCategories();
            });

            _timer.Enabled = false;
        }

        private void ResizeGridAndFillCategories()
        {
            if (AccountsGridControl.ItemsSource != null)
            {
                AccountsGridControl.Columns["Name"].Width = AccountsGridControl.ActualWidth * 0.19;
                AccountsGridControl.Columns["CreditPercent"].Width = AccountsGridControl.ActualWidth * 0.39;
                AccountsGridControl.Columns["DebetText"].Width = AccountsGridControl.ActualWidth * 0.39;
            }

            FillAccounts();
        }

        private void FillAccounts()
        {
            var accounts = DatabaseHelper.GetAccounts(_userID, (int?)AccountsGridControl.ActualWidth);
            AccountsGridControl.ItemsSource = accounts;
        }

        private void AccountsGridControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged)
                _timer.Enabled = true;
        }

        private void AddAccountButton_Click(object sender, RoutedEventArgs e)
        {
            var w = new SetAccountWindow(-1, _userID);
            w.Owner = (this.Parent as BudgetPlanning);
            if (w.ShowDialog() == true)
            {
                FillAccounts();
                OnAccountsEdited();
            };
       }

        private void EditAccountButton_Click(object sender, RoutedEventArgs e)
        {
            EditAccount();
        }

        private void EditAccount()
        {
            if (Database.HintCheckNoAccounts(_userID))
            {
                var w = new SetAccountWindow(_selectedAccountID, _userID);
                w.Owner = (this.Parent as BudgetPlanning);
                if (w.ShowDialog() == true)
                {
                    FillAccounts();
                    OnAccountsEdited();
                };
            }
        }

        private void DeleteAccountButton_Click(object sender, RoutedEventArgs e)
        {
            int res = -1;
            Int32.TryParse(G._S(G.db_select("exec ExistOperationsInAccount {1}", _selectedAccountID)), out res);
            if (res == 2)
            {  //нет операций по данному счету
                int selAccountID = -1;
                var selRows = AccountsGridControl.GetSelectedRowHandles();
                if (selRows.Length > 0) selAccountID = selRows[0];

                if (selAccountID != -1)
                {
                    if (MessageBox.Show("Вы действительно хотите удалить выделенный счет?", "Подтверждение", MessageBoxButton.YesNoCancel, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        G.db_exec("DeleteAccount {1}", _selectedAccountID);
                        CheckDB(G.LastError);

                        if (String.IsNullOrEmpty(G.LastError))
                        {
                            FillAccounts();
                            OnAccountsEdited();
                        }
                    }
                }
            }
            else if (res == 1)
            {  //есть операции по данному счету
                MessageBox.Show("Невозможно удалить счет.\nУдалите операции, проведенные по счету, прежде, чем удалять счет.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AccountsGridControlView_FocusedRowChanged(object sender, DevExpress.Xpf.Grid.FocusedRowChangedEventArgs e)
        {
            if (e.NewRow != null)
                _selectedAccountID = ((PlanningAccount)e.NewRow).Id;
            else
                _selectedAccountID = -1;
        }

        private void AccountsGridControlView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selRows = AccountsGridControl.GetSelectedRowHandles();
            if (selRows.Length == 1)
            {
                var account = AccountsGridControl.GetRow(selRows[0]) as PlanningAccount;
                if (account != null)
                    EditAccount();
            }
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
