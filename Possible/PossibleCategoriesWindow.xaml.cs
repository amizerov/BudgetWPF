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
    public partial class PossibleCategoriesWindow : DXWindow
    {
        private int _userID;

        public PossibleCategoriesWindow(int userID)
        {
            InitializeComponent();
            _userID = userID;

            if (Properties.Settings.Default.ThemeIndex > 0 && Properties.Settings.Default.ThemeIndex <= Consts.DevExTheme.Count)
                ThemeManager.SetTheme(this, Consts.DevExTheme[Properties.Settings.Default.ThemeIndex]);

            FillMyCategories();
            FillPossibleCategories();
        }

        private void FillMyCategories()
        {
            var dt = G.db_select("exec GetMyCategories {1}", _userID);
            listBoxMyCategories.ItemsSource = ((IListSource)dt).GetList();
            listBoxMyCategories.ValueMember = "ID";
            listBoxMyCategories.DisplayMember = "Name";
        }

        private void FillPossibleCategories()
        {
            var dt = G.db_select("exec GetPossibleCategories {1}", _userID);
            listBoxPossibleCategories.ItemsSource = ((IListSource)dt).GetList();
            listBoxPossibleCategories.ValueMember = "Name";
            listBoxPossibleCategories.DisplayMember = "Name";
        }

        private void imgToMyCategories_MouseUp(object sender, MouseButtonEventArgs e)
        {
            foreach (var cat in listBoxPossibleCategories.SelectedItems)
            {
                var catName = (cat as DataRowView).Row.ItemArray.First();
                G.db_exec("AddCategory '{1}', {2}", catName, _userID);
            }

            if (listBoxPossibleCategories.SelectedItems.Count > 0)
            {
                FillMyCategories();
                FillPossibleCategories();
            }
        }

        private void imgFromMyCategories_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (listBoxMyCategories.SelectedItem != null)
            {
                var catID = (listBoxMyCategories.SelectedItem as DataRowView).Row.ItemArray.First();

                var exist = Convert.ToInt32(G._S(G.db_select("CheckCategoryReferences {1}", catID)));

                    if (exist == 0)
                    {
                        G.db_exec("DeleteCategory {1}", catID);

                        if (String.IsNullOrEmpty(G.LastError))
                        {
                            FillMyCategories();
                            FillPossibleCategories();
                        }
                    }
                    else
                        MessageBox.Show("Существуют некоторые операции, относящиеся к данной категории. Невозможно удалить категорию",
                                        "Ошибка",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
            }
        }

        private void AddCheckButton_Click(object sender, RoutedEventArgs e)
        {
            var w = new AddCategoryWindow(_userID, -1);
            w.Title = "Добавление категории";
            w.WasAdded += () => { FillMyCategories(); };
            w.Owner = this;
            w.ShowDialog();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void listBoxPossibleCategories_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            imgToMyCategories_MouseUp(null, null);
        }

        private void listBoxMyCategories_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            imgFromMyCategories_MouseUp(null, null);
        }
    }
}
