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
    public partial class EditOperationWindow : DXWindow
    {
        private int _userID;
        private int _operationID;
        private string _amount, _description;
        private int _debet, _credit, _categoryID, _accountID, _toAccountID;
        private int _creditRating, _transferRating;
        private DateTime _operDay;
        private Consts.OperationType _type;
        private InfoWindow _hintWindow;

        private string _dbImgPath;      //первоначальный путь к файлу (временный - для изображения, которое хранилось в базе)
        private string _attachImgPath;  //путь к файлу для добавления/обновления изображения

        public EditOperationWindow(int userID, int operationID, double top, double left, int account, InfoWindow hintWindow)
        {
            InitializeComponent();

            _userID = userID;
            _operationID = operationID;
            _accountID = account;
            _toAccountID = 0;

            _creditRating = 0;
            _transferRating = 0;

            _hintWindow = hintWindow;

            _dbImgPath = String.Empty;
            _attachImgPath = String.Empty;

            if (Properties.Settings.Default.ThemeIndex > 0 && Properties.Settings.Default.ThemeIndex <= Consts.DevExTheme.Count)
                ThemeManager.SetTheme(this, Consts.DevExTheme[Properties.Settings.Default.ThemeIndex]);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(_dbImgPath))
                Utils.DelTempImage(_dbImgPath);

            this.DialogResult = false;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_operationID != -1)
            {
                //Заполнение категорий
                var isCredit = Convert.ToBoolean(G._S(G.db_select("GetCreditTypeByOperationID {1}, {2}", _operationID, _accountID)));
                if (isCredit)
                {
                    _type = Consts.OperationType.Credit;
                    listOperationType.SelectedIndex = 0;
                }
                else
                {
                    _type = Consts.OperationType.Debet;
                    listOperationType.SelectedIndex = 1;
                }

                DataTable r1 = am.BL.G.db_select("exec GetOperationByID {1}", _operationID);
                if (G.LastError.Length > 0)
                {
                    MessageBox.Show(G.LastError,
                                    "Ошибка в базе данных",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                    return;
                }
                DataRow r = r1.Rows[0];

                _amount = r["Amount"].ToString();
                _description = r["Description"].ToString();
                _debet = ((r["Debet_ID"].ToString()) != "") ? Convert.ToInt32(r["Debet_ID"]) : -1;
                _credit = ((r["Credit_ID"]).ToString() != "") ? Convert.ToInt32(r["Credit_ID"]) : -1;

                Int32.TryParse(r["Category_ID"].ToString(), out _categoryID);
                FillCategories(_type, r["Category_ID"]);

                DescriptionTextBox.Text = _description;
                AmountTextBox.Text = r["Amount"].ToString().Replace(',', '.').Replace(".0000", "");

                _operDay = Convert.ToDateTime(G._S(G.db_select("GetOperDayDateByID {1}", r["OperDay_ID"].ToString())));
                OperDayDatePicker.EditValue = _operDay;

                //Заполнение combobox аккаунтов
                FillAccounts();
                int secAccount = 0;
                Int32.TryParse(G._S(G.db_select("GetSecondAccount {1}, {2}", _operationID, isCredit ? 1 : 0)), out secAccount);
                if (secAccount == 0) secAccount = -1;
                AccountComboBox.EditValue = secAccount;
                _toAccountID = secAccount;

                //Проверка наличия изображения к операции
                var res = G._I(G.db_select("HasOperationImage {1}, {2}", _userID, _operationID));
                if (res == 1)
                    _attachImgPath = _dbImgPath = Utils.LoadTempImage(_userID, _operationID);
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
        }

        private void FillCategories(Consts.OperationType type, object categoryID)
        {
            List<Category> categories = DatabaseHelper.GetCategories(_userID, _creditRating, _transferRating, _accountID, null);

            var source = categories;
            CategoryComboBox.ItemsSource = source;
            CategoryComboBox.ValueMember = "Id";
            CategoryComboBox.DisplayMember = "Name";

            if (source.Count > 0)
            {
                if (categoryID != null)
                    CategoryComboBox.EditValue = categoryID;
                else
                    CategoryComboBox.SelectedIndex = 0;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Edit();
        }

        private void Edit()
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
                MessageBox.Show("Вы ввели неверную сумму. Невозможно сохранить операцию.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                if (CategoryComboBox.SelectedIndex > -1)
                {
                    if (_operationID != -1)
                    {
                        var newDate = (DateTime)OperDayDatePicker.EditValue;
                        var newDateStr = String.Format("{0}-{1:00}-{2:00} 00:00:00", newDate.Year, newDate.Month, newDate.Day);

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
                        
                        int res = G._I(G.db_select("exec SetOperation {1}, {2}, {3}, {4}, {5}, {6}, '{7}', {8}, '{9}'", _userID, type, amnt, _operationID, acc1, acc2, desc, catg, newDateStr));
                        CheckDB(G.LastError);

                        if (res > 0)  //успешно
                        {
                            if (!String.IsNullOrEmpty(_attachImgPath))
                            {  //есть какое - то изображение к операции
                                if (_dbImgPath != _attachImgPath)  //изображение к операции поменялось => обновляем в БД
                                {
                                    var imgStream = new FileStream(_attachImgPath, FileMode.Open);
                                    var memoryStream = new MemoryStream();
                                    imgStream.CopyTo(memoryStream);
                                    Transport.SetImage(_userID, _operationID,
                                                       System.IO.Path.GetFileName(_attachImgPath),
                                                       memoryStream.ToArray());
                                }
                            }
                            else
                            {  //изображения к операции нет
                                if (!String.IsNullOrEmpty(_dbImgPath))  //но раньше было => удаляем изображение из БД
                                    Transport.DelImage(_userID, _operationID);
                            }

                            this.DialogResult = true;
                            Close();
                        }
                        else if (res == -3)  //уход в минус
                            MessageBox.Show("Невозможно изменить операцию, так как введенная вами сумма переводит остаток по счету в некоторые ОперДни в \"минус\"!",
                                            "Ошибка",
                                            MessageBoxButton.OK,
                                            MessageBoxImage.Error);
                        else if (res <= 0)  //остальные ошибки
                            MessageBox.Show("Возникла ошибка при обращении к базе данных. Повторите попытку позднее.",
                                            "Ошибка",
                                            MessageBoxButton.OK,
                                            MessageBoxImage.Error);

                        if (!String.IsNullOrEmpty(_dbImgPath))
                            Utils.DelTempImage(_dbImgPath);
                    }
                }
                else
                    MessageBox.Show("Категория не выбрана. Добавление операции невозможно.",
                                    "Ошибка",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
            }
            else
                MessageBox.Show("ОперДень не выбран. Редактирование операции невозможно.",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
        }

        private void EditCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            var selValue = CategoryComboBox.EditValue;

            WindowCatEdit w = null;
            if (_type == Consts.OperationType.Credit)
                w = new WindowCatEdit(_userID, Consts.OperationType.Credit, _creditRating, _transferRating, _accountID, _hintWindow);
            else if (_type == Consts.OperationType.Debet)
                w = new WindowCatEdit(_userID, Consts.OperationType.Debet, _creditRating, _transferRating, _accountID, _hintWindow);

            w.CategoryAdded += () => { FillCategories(_type, null); };
            w.CategoryEdited += () => { FillCategories(_type, null); };
            w.CategoryDeleted += () => { FillCategories(_type, null); };
            w.CategoryWasChosen += (int categoryID) => { CategoryComboBox.EditValue = categoryID; };
            w.Owner = this;
            w.ShowDialog();
        }

        private void AccountComboBox_OnSelectedIndexChanged(object sender, RoutedEventArgs e)
        {
            _toAccountID = Convert.ToInt32(AccountComboBox.EditValue);
            RecountRatingParameters();
            FillCategories(_creditRating == 1 ? Consts.OperationType.Credit : Consts.OperationType.Debet, _categoryID);
        }

        /// <summary>
        /// Перерассчитать параметры для сортировки по рейтингу
        /// </summary>
        private void RecountRatingParameters()
        {
            if (Convert.ToInt32(AccountComboBox.EditValue) == -1 || AccountComboBox.EditValue == null)
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

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }

        private void DescriptionTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Edit();
        }

        private void listOperationType_SelectedIndexChanged(object sender, RoutedEventArgs e)
        {
            _type = listOperationType.SelectedIndex == 0 ? Consts.OperationType.Credit : Consts.OperationType.Debet;
            RecountRatingParameters();
            FillCategories(_type, _categoryID);
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
