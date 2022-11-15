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
using Budget.Model;
using am.BL;
using System.Data;
using System.ComponentModel;
using DevExpress.Xpf.Core;

namespace Budget
{
    public partial class WindowCatEdit : DXWindow
    {
        private int _userID;
        private int _selectedCategoryId = -1;
        private Consts.OperationType _type;
        private int _creditRating, _transferRating, _accountID;
        private InfoWindow _hintWindow;
        
        public delegate void ChangeEvent();
        public event ChangeEvent CategoryEdited;
        public event ChangeEvent CategoryDeleted;
        public event ChangeEvent CategoryAdded;

        public delegate void ChooseHandler(int categoryID);
        public event ChooseHandler CategoryWasChosen;

        public WindowCatEdit(int userID, Consts.OperationType type, int creditRating, int transferRating, int accountID, InfoWindow hintWindow)
        {
            InitializeComponent();

            _userID = userID;
            _type = type;
            _creditRating = creditRating;
            _transferRating = transferRating;
            _accountID = accountID;
            _hintWindow = hintWindow;

            FillCategories(_type);

            if (Properties.Settings.Default.ThemeIndex > 0 && Properties.Settings.Default.ThemeIndex <= Consts.DevExTheme.Count) 
                ThemeManager.SetTheme(this, Consts.DevExTheme[Properties.Settings.Default.ThemeIndex]);

            Utils.RestoreCatFormCoords(this);  //восстановить размеры и положение формы
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NoCategoriesShowHint();
        }

        //Если нет ни одной категории - показываем подсказу "Добавление категории"
        private void NoCategoriesShowHint()
        {
            if (Properties.Settings.Default.ShowHints)
            {
                if (Database.HintCheckNoCategories(_userID))
                {
                    _hintWindow.LoadHint("addcategory");
                    _hintWindow.ShowDialog();
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void FillCategories(Consts.OperationType type)
        {
            _type = type;

            List<Category> categories = DatabaseHelper.GetCategories(_userID, _creditRating, _transferRating, _accountID, (int?)CategoriesGridControl.ActualWidth);
            CategoriesGridControl.ItemsSource = categories;
        }

        private void AddCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            AddCategory();
        }

        private void AddCategory()
        {
            AddCategoryWindow w = new AddCategoryWindow(_userID, -1);
            w.Title = "Добавление категории";
            w.WasAdded += () => { FillCategories(_type); if (_accountID != -1) CategoryAdded(); };
            w.Owner = this;
            w.ShowDialog();
        }

        private void tableView1_FocusedRowChanged(object sender, DevExpress.Xpf.Grid.FocusedRowChangedEventArgs e)
        {
            if (e.NewRow != null)
            {
              _selectedCategoryId = ((Category) e.NewRow).Id;
            }
            else
            {
                _selectedCategoryId = -1;
            }
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCategoryId != -1)
            {
                var exist = Convert.ToInt32(G._S(G.db_select("CheckCategoryReferences {1}", _selectedCategoryId)));

                if (exist == 0)
                {
                    G.db_exec("DeleteCategory {1}", _selectedCategoryId);
                    FillCategories(_type);

                    if (_accountID != -1)
                        CategoryDeleted();
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

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            EditCategory();
        }

        private void tableView1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selRows = CategoriesGridControl.GetSelectedRowHandles();
            if (selRows.Length == 1)
            {
                Category category = CategoriesGridControl.GetRow(selRows[0]) as Category;
                if (category != null)
                {
                    if (_accountID != -1)
                    {
                        CategoryWasChosen(category.Id);
                        Close();
                    }
                    else
                        EditCategory();
                }
            }
        }

        /// <summary>
        /// Редактирование категории
        /// </summary>
        private void EditCategory()
        {
            var w = new AddCategoryWindow(_userID, _selectedCategoryId);
            w.Title = "Редактирование категории";
            w.WasEdited += () => { FillCategories(_type); if (_accountID != -1) CategoryEdited(); };
            w.Owner = this;
            w.ShowDialog();
            FillCategories(_type);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }

        private void DXWindow_Closing(object sender, CancelEventArgs e)
        {
            Utils.SaveCatFormCoords(this);
        }
    }
}
