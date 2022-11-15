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
using Budget.Model;
using System.Timers;
using System.Windows.Threading;
using System.Threading;
using am.BL;

namespace Budget.BudgetPlanningControls
{
    public partial class CategoriesPlanning : UserControl
    {
        private int _userID;
        private int _selectedCategoryID;
        private System.Timers.Timer _timer;

        public CategoriesPlanning(int userID)
        {
            InitializeComponent();
            _userID = userID;
            _selectedCategoryID = -1;

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
            if (CategoriesGridControl.ItemsSource != null)
            {
                CategoriesGridControl.Columns["Name"].Width = CategoriesGridControl.ActualWidth * 0.19;
                CategoriesGridControl.Columns["CreditPercent"].Width = CategoriesGridControl.ActualWidth * 0.39;
                CategoriesGridControl.Columns["DebetText"].Width = CategoriesGridControl.ActualWidth * 0.39;
            }

            FillCategories();
        }

        private void FillCategories()
        {
            var categories = DatabaseHelper.GetCategories(_userID, 0, 0, -1, (int?)CategoriesGridControl.ActualWidth);
            CategoriesGridControl.ItemsSource = categories;
        }

        private void CategoriesGridControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged)
                _timer.Enabled = true;
        }

        private void AddCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            var w = new AddCategoryWindow(_userID, -1);
            w.Title = "Добавление категории";
            w.WasAdded += () => { FillCategories(); };
            w.Owner = (this.Parent as BudgetPlanning);
            w.ShowDialog();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            EditCategory();
        }

        private void EditCategory()
        {
            var w = new AddCategoryWindow(_userID, _selectedCategoryID);
            w.Title = "Редактирование категории";
            w.WasEdited += () => { FillCategories(); };
            w.Owner = (this.Parent as BudgetPlanning);
            w.ShowDialog();
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCategoryID != -1)
            {
                var exist = Convert.ToInt32(G._S(G.db_select("CheckCategoryReferences {1}", _selectedCategoryID)));

                if (exist == 0)
                {
                    G.db_exec("DeleteCategory {1}", _selectedCategoryID);
                    FillCategories();
                }
                else
                    MessageBox.Show("Существуют некоторые операции, относящиеся к данной категории. Удалите их прежде, чем удалять категорию.",
                                    "Ошибка",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
            }
            if (G.LastError.Length > 0)
            {
                MessageBox.Show(G.LastError,
                                "Ошибка в базе данных",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return;
            }
        }

        private void CategoriesGridControlView_FocusedRowChanged(object sender, DevExpress.Xpf.Grid.FocusedRowChangedEventArgs e)
        {
            if (e.NewRow != null)
                _selectedCategoryID = ((Category)e.NewRow).Id;
            else
                _selectedCategoryID = -1;
        }

        private void CategoriesGridControlView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selRows = CategoriesGridControl.GetSelectedRowHandles();
            if (selRows.Length == 1)
            {
                var category = CategoriesGridControl.GetRow(selRows[0]) as Category;
                if (category != null)
                    EditCategory();
            }
        }
    }
}
