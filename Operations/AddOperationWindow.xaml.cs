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
using System.IO;

namespace Budget
{
    public partial class AddOperationWindow : DXWindow
    {
        private int _userID;
        private int _operationID;
        private int _accountID, _toAccountID;
        private int _creditRating, _transferRating;
        private Consts.OperationType _type;
        private InfoWindow _hintWindow;

        //private string _dbImgPath;      //первоначальный путь к файлу (временный - для изображения, которое хранилось в базе)
        private string _attachImgPath;  //путь к файлу для добавления/обновления изображения

        public AddOperationWindow(int userID, int operation, double top, double left, int account, Consts.OperationType type, InfoWindow hintWindow)
        {
            InitializeComponent();

            _userID = userID;
            _operationID = operation;
            _accountID = account;
            _toAccountID = 0;
            _hintWindow = hintWindow;
            _attachImgPath = String.Empty;

            _type = type;

            listOperationType.SelectedIndex = _type == Consts.OperationType.Credit ? 0 : 1;

            FillAccounts();
            FillCategories(Consts.OperationType.Credit, null);

            OperDayDatePicker.EditValue = DateTime.Now;

            this.Title = String.Format("Добавление операции. Счет \"{0}\"", G._S(G.db_select("exec GetAccountNameByID {1}", _accountID)));

            if (Properties.Settings.Default.ThemeIndex > 0 && Properties.Settings.Default.ThemeIndex <= Consts.DevExTheme.Count)
                ThemeManager.SetTheme(this, Consts.DevExTheme[Properties.Settings.Default.ThemeIndex]);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AmountTextBox.Focus();

            NoCategoriesShowHint();
        }

        //Если нет ни одной категории - показываем подсказу "Добавление категории"
        private void NoCategoriesShowHint()
        {
            if (Properties.Settings.Default.ShowHints)
            {
                if (Database.HintCheckNoCategories(_userID))
                {
                    _hintWindow.LoadHint("choosecategory");
                    _hintWindow.Show();
                }
            }
        }

        /// <summary>
        /// Загрузка счетов
        /// </summary>
        private void FillAccounts()
        {
            var dt = am.BL.G.db_select("exec GetAccountsListForAdding {1}, {2}", _accountID, _userID);
            CheckDB(G.LastError);
            AccountComboBox.ItemsSource = ((IListSource)dt).GetList();
            AccountComboBox.ValueMember = "ID";
            AccountComboBox.DisplayMember = "Name";
            if (AccountComboBox.ItemsSource != null)
                AccountComboBox.SelectedIndex = 0;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }

        private void FillCategories(Consts.OperationType type, object categoryID)
        {
            DataTable dt = new DataTable();
            List<Category> categories = DatabaseHelper.GetCategories(_userID, _creditRating, _transferRating, _accountID, null);
            
            CategoryComboBox.ItemsSource = categories;
            CategoryComboBox.ValueMember = "Id";
            CategoryComboBox.DisplayMember = "Name";

            if (CategoryComboBox.ItemsSource != null)
            {
                if (categoryID != null)
                    CategoryComboBox.EditValue = categoryID;
                else
                    CategoryComboBox.SelectedIndex = 0;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void Save()
        {
            double amount = -1;
            int type = -1;
            int account1 = -1, account2 = -1;

            try
            {
                amount = Convert.ToDouble(AmountTextBox.Text.Replace('.', ','));
                if (amount < 0)
                    amount *= -1;
            }
            catch
            {
                MessageBox.Show("Некорректный формат суммы. Невозможно сохранить операцию.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (amount <= 0)
            {
                MessageBox.Show("Сумма операции не может быть нулевой",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return;
            }

            if (OperDayDatePicker.EditValue != null)
            {
                if (CategoryComboBox.SelectedItemValue != null)
                {
                    var newDateStr = OperDayDatePicker.DateTime.ToString("yyyyMMdd");

                    if (Convert.ToInt32(AccountComboBox.EditValue) == -1)
                    {  //без счета (не является переводом со счета на счет)
                        if (listOperationType.SelectedIndex == 0)
                        {  //зачисление
                            type = 1;  //тип "Зачисление"
                            account2 = _accountID;
                        }
                        else
                        {  //списание
                            type = 2;  //тип "Списание"
                            account1 = _accountID;
                        }
                    }
                    else
                    {  //перевод со счета на счет
                        if (listOperationType.SelectedIndex == 0)  //зачисление
                            type = 1;
                        else  //списание
                            type = 2;

                        account1 = _accountID;
                        account2 = _toAccountID;
                    }

                    string amnt = amount.ToString().Replace(',', '.');
                    string acc1 = account1 != -1 ? account1.ToString() : "NULL";
                    string acc2 = account2 != -1 ? account2.ToString() : "NULL";
                    string desc = DescriptionTextBox.Text.Replace("'", "''");
                    string catg = CategoryComboBox.EditValue.ToString();

                    int res = G._I(G.db_select("exec SetOperation {1}, {2}, {3}, {4}, {5}, {6}, '{7}', {8}, '{9}'", _userID, type, amnt, "NULL", acc1, acc2, desc, catg, newDateStr));
                    CheckDB(G.LastError);

                    if (res > 0)  //успешно
                    {
                        //Проверяем, есть ли прикрепрепленное изображение к операции
                        if (!String.IsNullOrEmpty(_attachImgPath))
                        {
                            var imgStream = new FileStream(_attachImgPath, FileMode.Open);
                            var memoryStream = new MemoryStream();
                            imgStream.CopyTo(memoryStream);
                            Transport.SetImage(_userID, res,
                                               System.IO.Path.GetFileName(_attachImgPath),
                                               memoryStream.ToArray());
                        }

                        this.DialogResult = true;
                        Close();
                    }
                    else if (res == -3)  //уход в минус
                        MessageBox.Show("Невозможно добавить операцию, так как введенная вами сумма переводит остаток по счету в некоторые ОперДни в \"минус\"!",
                                        "Ошибка",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                    else if (res <= 0)  //остальные ошибки
                        MessageBox.Show("Возникла ошибка при обращении к базе данных. Повторите попытку позднее.",
                                        "Ошибка",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                }
                else
                    MessageBox.Show("Категория не выбрана. Добавление операции невозможно.",
                                    "Ошибка",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
            }
            else
                MessageBox.Show("ОперДень не выбран. Добавление операции невозможно.",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
        }

        private void EditCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            var selValue = CategoryComboBox.EditValue;

            WindowCatEdit w = null;
            if (listOperationType.SelectedIndex == 0)
                w = new WindowCatEdit(_userID, Consts.OperationType.Credit, _creditRating, _transferRating, _accountID, _hintWindow);
            else if (listOperationType.SelectedIndex == 1)
                w = new WindowCatEdit(_userID, Consts.OperationType.Debet, _creditRating, _transferRating, _accountID, _hintWindow);

            w.CategoryAdded += () => { FillCategories(_type, null); };
            w.CategoryEdited += () => { FillCategories(_type, null); };
            w.CategoryDeleted += () => { FillCategories(_type, null); };
            w.CategoryWasChosen += (int categoryID) => { CategoryComboBox.EditValue = categoryID; };
            w.Owner = this;
            w.ShowDialog();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
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

        private void AccountComboBox_OnSelectedIndexChanged(object sender, RoutedEventArgs routedEventArgs)
        {
            _toAccountID = Convert.ToInt32(AccountComboBox.EditValue);
            RecountRatingParameters();
            FillCategories(_creditRating == 1 ? Consts.OperationType.Credit : Consts.OperationType.Debet, null);
        }

        private void radBtnCredit_Checked(object sender, RoutedEventArgs e)
        {
            _type = Consts.OperationType.Credit;
            RecountRatingParameters();
            FillCategories(Consts.OperationType.Credit, null);
        }

        private void radBtnDebet_Checked(object sender, RoutedEventArgs e)
        {
            _type = Consts.OperationType.Debet;
            RecountRatingParameters();
            FillCategories(Consts.OperationType.Debet, null);
        }

        /// <summary>
        /// Перерассчитать параметры для сортировки по рейтингу
        /// </summary>
        private void RecountRatingParameters()
        {
            if (Convert.ToInt32(AccountComboBox.EditValue) == -1)
            {  //без счета (не является переводом со счета на счет)
                _transferRating = 0;

                if (listOperationType.SelectedIndex == 0)
                    _creditRating = 1;
                else
                    _creditRating = 0;
            }
            else  //перевод со счета на счет
                _transferRating = 1;
        } 

        private void DescriptionTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Save();
        }

        private void listOperationType_SelectedIndexChanged(object sender, RoutedEventArgs e)
        {
            _type = listOperationType.SelectedIndex == 0 ? _type = Consts.OperationType.Credit : Consts.OperationType.Debet;
            RecountRatingParameters();
            FillCategories(_type, null);
        }

        private void btnAttach_Click(object sender, RoutedEventArgs e)
        {
            var addImageForm = new AddImage(_attachImgPath);
            addImageForm.Owner = this;
            addImageForm.OnImageChosen += (imgPath) => { _attachImgPath = imgPath; };
            addImageForm.Show();
        }
    }
}
