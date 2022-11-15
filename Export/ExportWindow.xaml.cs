using Budget.Model;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;


namespace Budget
{
    public partial class ExportWindow : Window
    {
        private int _userID;

        private List<ExportAccount> _accounts;
        private List<ExportOperation> _operations;
        private List<ExportCategory> _categories;

        public ExportWindow(int userID)
        {
            InitializeComponent();

            _userID = userID;

            _accounts = new List<ExportAccount>();
            _operations = new List<ExportOperation>();
            _categories = new List<ExportCategory>();

            LoadAllAccounts();
            LoadAllCategories();
        }

        private void LoadAllAccounts()
        {
            _accounts = Database.GetAccountsForExport(_userID);

            listAccounts.ItemsSource = _accounts;
        }

        private void LoadAllCategories()
        {
            _categories = Database.GetCategoriesForExport(_userID);

            listCategories.ItemsSource = _categories;
        }

        private void LoadAllOperations()
        {
            _operations.Clear();

            foreach(ExportAccount account in _accounts)
            {
                var tempOperations = Database.GetOperationsForExport(account.ID);

                foreach (var operation in tempOperations)
                {
                    if (_operations.FirstOrDefault(o => o.ID == operation.ID) == null)
                        _operations.Add(operation);
                }
            }
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            var acCount = _accounts.Where(a => a.IsChecked == true).Count();
            var catCount = _categories.Where(a => a.IsChecked == true).Count();
            if (acCount == 0 && catCount == 0)
            {
                MessageBox.Show("Вы не выбрали ни одного аккаунта и ни одной категории", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "Выберите папку для экспорта";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var tempViewAc = new TableView() { AutoWidth = true };
                var tempGridAc = new GridControl() { View = tempViewAc, AllowDrop = false, AutoGenerateColumns = AutoGenerateColumnsMode.RemoveOld };
                var tempViewOp = new TableView() { AutoWidth = true };
                var tempGridOp = new GridControl() { View = tempViewOp, AllowDrop = false, AutoGenerateColumns = AutoGenerateColumnsMode.RemoveOld };
                var tempViewCat = new TableView() { AutoWidth = true };
                var tempGridCat = new GridControl() { View = tempViewCat, AllowDrop = false, AutoGenerateColumns = AutoGenerateColumnsMode.RemoveOld };

                try
                {
                    //Сохранение выбранных счетов
                    var tempAccounts = _accounts.Where(a => a.IsChecked == true).ToList<ExportAccount>();
                    tempGridAc.ItemsSource = tempAccounts;
                    tempViewAc.ExportToXls(String.Format("{0}\\{1}.xls", dialog.SelectedPath, "__all_accounts"),
                                         new DevExpress.XtraPrinting.XlsExportOptions(DevExpress.XtraPrinting.TextExportMode.Value));

                    //Сохранение всех операций для выбранных счетов
                    LoadAllOperations();
                    tempGridOp.ItemsSource = _operations;
                    tempViewOp.ExportToXls(String.Format("{0}\\{1}.xls", dialog.SelectedPath, "__all_operations"),
                                         new DevExpress.XtraPrinting.XlsExportOptions(DevExpress.XtraPrinting.TextExportMode.Value));

                    //Сохранение выбранных категорий
                    var tempCategories = _categories.Where(c => c.IsChecked == true).ToList<ExportCategory>();
                    tempGridCat.ItemsSource = tempCategories;
                    tempViewCat.ExportToXls(String.Format("{0}\\{1}.xls", dialog.SelectedPath, "__all_categories"),
                                         new DevExpress.XtraPrinting.XlsExportOptions(DevExpress.XtraPrinting.TextExportMode.Value));

                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Возникла ошибка во время экпорта данных в Excel.\n" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Выделить все счета/Снять выделение
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBoxAcSelectAll_Checked(object sender, RoutedEventArgs e)
        {
            if (checkBoxAcSelectAll.Content.ToString() == "выбрать все")
            {
                foreach (ExportAccount item in listAccounts.Items)
                    item.IsChecked = true;
                listAccounts.Items.Refresh();

                checkBoxAcSelectAll.IsChecked = false;
                checkBoxAcSelectAll.Content = "снять все";
            }
            else if (checkBoxAcSelectAll.Content.ToString() == "снять все")
            {
                foreach (ExportAccount item in listAccounts.Items)
                    item.IsChecked = false;
                listAccounts.Items.Refresh();

                checkBoxAcSelectAll.IsChecked = false;
                checkBoxAcSelectAll.Content = "выбрать все";
            }
        }

        /// <summary>
        /// Выделить все категории/Снять выделение
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBoxCatSelectAll_Checked(object sender, RoutedEventArgs e)
        {
            if (checkBoxCatSelectAll.Content.ToString() == "выбрать все")
            {
                foreach (ExportCategory item in listCategories.Items)
                    item.IsChecked = true;
                listCategories.Items.Refresh();

                checkBoxCatSelectAll.IsChecked = false;
                checkBoxCatSelectAll.Content = "снять все";
            }
            else if (checkBoxCatSelectAll.Content.ToString() == "снять все")
            {
                foreach (ExportCategory item in listCategories.Items)
                    item.IsChecked = false;
                listCategories.Items.Refresh();

                checkBoxCatSelectAll.IsChecked = false;
                checkBoxCatSelectAll.Content = "выбрать все";
            }
        }
    }
}
