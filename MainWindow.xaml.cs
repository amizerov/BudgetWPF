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
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Editors;
using am.BL;
using System.Data.SqlClient;
using System.Data;
using System.ComponentModel;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Editors.Settings;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Collections;
using Budget.ReportControls;
using System.Windows.Markup;
using Budget.Operations;
using Microsoft.Win32;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;
using DevExpress.Xpf.Bars;
using Budget.Model;

using DevExpress.Xpf.Printing;
using DevExpress.XtraPrinting;
using System.Windows.Forms.Integration;
using System.Threading.Tasks;
using DevExpress.Mvvm.Native;

namespace Budget
{
    public partial class MainWindow : DXWindow
    {
        public int _userID;
        private int _selectedAccountId = -1, _selectedOperationId = -1, _selectedOperationStatus = 0, _selectedOperationCredit = 0;
        //private DateTime _downDate, _upDate;
        public DateTime _BuildTime;
        private double _lastAccountsOperationsHeight;  //последняя высота сплиттера
        private Queue<Banner> _banners;  //очередь баннеров
        private Banner _nowBanner;  //текущий баннер
        private Timer _timer;  //таймер смены баннера
        private InfoWindow _hintWindow;  //окно подсказок
        private FileLoader _fileLoader;

        const string textToShare = "Программа \"Бюджет\"\n http://dev.ultrazoom.ru/downloads/BudgetSetup1302.exe \n\nПрограмма предназначена для ведения учета в небольшой компании или даже личных финансов.\nДля работы необязательно иметь знания бухгалтерии, интерфейс интуитивно понятен и практичен. Программа позволяет добавлять счета, проводить по ним операции, переводить средства со счета на счет, получать статистику.";

        #region Словарь статусов операций
        public ComboBoxEditSettings Statuses
        {
            get
            {
                var editSettingsItem = new ComboBoxEditSettings
                {
                    ItemsSource = Database.GetOperationStatuses(),
                    DisplayMember = "Name",
                    ValueMember = "ID",
                    NullText = "",
                    AllowNullInput = false
                };

                return editSettingsItem;
            }
        }
        #endregion  Словарь статусов операций

        #region Load
        public MainWindow(int userID, FileLoader fileLoader)
        {
            Cursor = Cursors.Wait;
            var splashScreen = new SplashScreen("/images/logo.png");
            splashScreen.Show(true, true);

            InitializeComponent();

            _userID = userID;
            _fileLoader = fileLoader;
            printWebBrowser.LoadCompleted += printWebBrowser_LoadCompleted;

            WFUcRestChart restChart = new WFUcRestChart();
            restChart.Height = 10;
            wfhBalanceRestChart.Child = restChart;

            balanceAccountsGrid.Children.Add(new BalanceAccountsReportControl(_userID));      //инициализация раздела отчета по балансу по счетам
            //balanceCategoriesGrid.Children.Add(new BalanceCategoriesReportControl(_userID));  //инициализация раздела отчета по балансу по категориям
            balanceCategoriesGrid.Children.Add(new WindowsFormsHost() { Child = new WFUcCategoriesReport() });

            lblLogin.Content = "Ваш логин: " + Database.GetUserName(_userID);

            Cursor = Cursors.Arrow;

            IPManager ipManager = new IPManager(_userID);  //получение внешнего IP в background-потоке
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ConnectToDatabase();

            Assembly assembly = Assembly.GetExecutingAssembly();
            FileInfo fileInfo = new FileInfo(assembly.Location);
            _BuildTime = fileInfo.LastWriteTime;
            Row1.MaxHeight = ActualHeight - 300;
            VersionTextBlock.Content = "Version: " + assembly.ToString() + " " + _BuildTime.ToString();

            //Восстановление разметки таблиц и дат периода
            Utils.RestoreAccountsLayout(AccountsGridControl);
            Utils.RestoreOperationsLayout(OperationsGridControl);
            DateTime fromDate, toDate;
            Utils.RestoreDates(out fromDate, out toDate);
            datePickerOperationsFrom.EditValue = AccountsDownDatePicker.EditValue = fromDate;
            datePickerOperationsTo.EditValue = AccountsUpDatePicker.EditValue = toDate;
            

            _lastAccountsOperationsHeight = MainGrid.RowDefinitions[0].Height.Value;

            _banners = LoadBanners(_userID);
            Utils.UpdateRatingBanner(_banners, _userID);
            if (_banners.Count > 1)  //rotator активен только тогда, когда баннеров > 1
                _timer = new Timer(Callback, null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(5));
            else  //иначе один раз загружаем баннер и не тратим трафик
                UpdateBanner();

            var curVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Database.SetVersion(_userID, curVersion);

            Utils.RestoreFormCoords(this);  //восстановить размеры и положение формы
            if (Utils.RestoreSplitterDist() != -1) MainGrid.RowDefinitions[0].Height = new GridLength(Utils.RestoreSplitterDist());  //восстановить сплиттер

            //Установить тему, которая была выбрана ранее пользователем
            SetSavedTheme();

            FillAccounts(null);
            this.Show();
        }

        //Если нет ни одного счета - показываем подсказу "Добавление счета" и окно добавления счетов
        private bool NoAccountsShowHint()
        {
            if (Properties.Settings.Default.ShowHints)
            {
                if (Database.HintCheckNoAccounts(_userID))
                {
                    _hintWindow.LoadHint("addaccount");

                    _hintWindow.Show();

                    var w = new PossibleAccountsWindow(_userID) { Owner = this };
                    if (w == null) return true;

                    w.OnAccountsAdded += () =>
                        {
                            FillAccounts(null);
                        };
                    w.Show();

                    return true;
                }
            }

            return false;
        }

        //Если нет ни одной операции - показываем подсказу "Добавление операции"
        private void NoOperationsShowHint()
        {
            if (Properties.Settings.Default.ShowHints)
            {
                if (Database.HintCheckNoOperations(_userID))
                {
                    _hintWindow.LoadHint("addoperation");
                    _hintWindow.ShowDialog();
                }
            }
        } 

        /// <summary>
        /// Callback-процедура таймера
        /// </summary>
        private void Callback(object param)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)UpdateBanner);
        }

        /// <summary>
        /// Обновление баннера
        /// </summary>
        void UpdateBanner()
        {
            _nowBanner = _banners.Dequeue();
            _banners.Enqueue(_nowBanner);

            if (_nowBanner.Type == Banner.BannerType.Picture)
            {
                rectangleBanner.Visibility = System.Windows.Visibility.Hidden;
                bannerTextBlock.Visibility = System.Windows.Visibility.Hidden;
                bannerImg.Visibility = System.Windows.Visibility.Visible;

                BitmapImage bmi = new BitmapImage();
                bmi.BeginInit();
                bmi.UriSource = new Uri(_nowBanner.Link);
                bmi.EndInit();
                bannerImg.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate()
                {
                    bannerImg.Source = bmi;
                });
            }
            else
            {
                rectangleBanner.Visibility = System.Windows.Visibility.Visible;
                bannerTextBlock.Visibility = System.Windows.Visibility.Visible;
                bannerImg.Visibility = System.Windows.Visibility.Hidden;

                bannerTextBlock.Text = _nowBanner.Content;
                if (!String.IsNullOrEmpty(_nowBanner.Url))
                    bannerTextBlock.Cursor = Cursors.Hand;
                else
                    bannerTextBlock.Cursor = Cursors.Arrow;
            }
        }

        /// <summary>
        /// Загрузить баннеры
        /// </summary>
        private Queue<Banner> LoadBanners(int userID)
        {
            var temp = new Queue<Banner>();

            var banners = Database.GetBannersWithAuthorization(_userID);
            foreach (var banner in banners)
                temp.Enqueue(banner);

            return temp;
        }
        #endregion

        #region Аккаунты
        private void FillAccounts(int? focusedRow)
        {
            AccountsGridControl.ItemsSource = null;
            AccountsGridControl.Columns.Clear();

            DateTime dtDown = default(DateTime), dtUp = default(DateTime);
            try
            {
                dtDown = (DateTime)AccountsDownDatePicker.EditValue;
                dtUp = (DateTime)AccountsUpDatePicker.EditValue;
            }
            catch { }

            if (dtDown != default(DateTime) && dtUp != default(DateTime))
            {
                var accounts = Database.GetAccountTable(dtDown, dtUp, _userID);
                CheckDB(G.LastError);

                if(accounts.Rows.Count == 0) return;

                AccountsGridControl.ItemsSource = accounts;

                try
                {
                    //Таблица счетов
                    AccountsGridControl.Columns["Order"].Visible = false;
                    AccountsGridControl.Columns["ID"].Visible = false;
                    AccountsGridControl.Columns["ID"].VisibleIndex = 0;

                    foreach (var col in AccountsGridControl.Columns)
                        col.AllowFocus = false;

                    AccountsGridControl.Columns["Deb"].Header = "Списание";
                    AccountsGridControl.Columns["Cred"].Header = "Зачисление";
                    AccountsGridControl.Columns["Name"].Header = "Название";
                    AccountsGridControl.Columns["Rest1"].Header = "Входящий остаток";
                    AccountsGridControl.Columns["Rest2"].Header = "Исходящий остаток";

                    AccountsGridControl.Columns["Rest1"].CellTemplate = AccountsGridControl.Resources["moneyCellTemplate1"] as DataTemplate;
                    AccountsGridControl.Columns["Rest2"].CellTemplate = AccountsGridControl.Resources["moneyCellTemplate1"] as DataTemplate;
                    AccountsGridControl.Columns["Deb"].CellTemplate = AccountsGridControl.Resources["moneyCellTemplate"] as DataTemplate;
                    AccountsGridControl.Columns["Cred"].CellTemplate = AccountsGridControl.Resources["moneyCellTemplate"] as DataTemplate;

                    if (focusedRow != null)
                        AccountsGridControl.SelectedItem = AccountsGridControl.GetRow((int)focusedRow);

                    //Таблица операций
                    OperationsGridControl.Columns["ID"].Visible = false;
                    OperationsGridControl.Columns["Debet_ID"].Visible = false;
                    OperationsGridControl.Columns["OperDay_ID"].Visible = false;
                    OperationsGridControl.Columns["Credit_ID"].Visible = false;
                    OperationsGridControl.Columns["Amount"].Visible = false;
                    OperationsGridControl.Columns["Status"].Visible = false;
                    OperationsGridControl.Columns["DateCreate"].Visible = false;
                    OperationsGridControl.Columns["HasImage"].Visible = false;

                    foreach (var col in OperationsGridControl.Columns)
                        col.AllowFocus = false;

                    OperationsGridControl.Columns["SecAccount"].Header = "2-ой счет";
                    OperationsGridControl.Columns["SecAccount"].VisibleIndex = 0;
                    OperationsGridControl.Columns["OperDay"].Header = "ОперДень";
                    OperationsGridControl.Columns["OperDay"].VisibleIndex = 1;
                    OperationsGridControl.Columns["Debet"].Header = "Списание";
                    OperationsGridControl.Columns["Debet"].VisibleIndex = 2;
                    OperationsGridControl.Columns["Credit"].Header = "Зачисление";
                    OperationsGridControl.Columns["Credit"].VisibleIndex = 3;
                    OperationsGridControl.Columns["Category"].Header = "Категория";
                    OperationsGridControl.Columns["Category"].VisibleIndex = 4;
                    OperationsGridControl.Columns["Description"].Header = "Описание";
                    OperationsGridControl.Columns["Description"].VisibleIndex = 5;

                    OperationsGridControl.Columns["Amount"].Header = "Сумма";
                    OperationsGridControl.Columns["DateCreate"].Header = "Дата";
                    OperationsGridControl.Columns["Status"].Header = "Статус";

                    OperationsGridControl.Columns["Status"].EditSettings = Statuses;

                    OperationsGridControl.Columns["Debet"].CellTemplate = OperationsGridControl.Resources["debetCellTemplate"] as DataTemplate;
                    OperationsGridControl.Columns["Credit"].CellTemplate = OperationsGridControl.Resources["creditCellTemplate"] as DataTemplate;

                    OperationsGridControl.Columns["Debet"].CellStyle = OperationsGridControl.Resources["debetCellStyle"] as Style;
                    OperationsGridControl.Columns["Credit"].CellStyle = OperationsGridControl.Resources["creditCellStyle"] as Style;
                    OperationsGridControl.Columns["Status"].CellStyle = OperationsGridControl.Resources["statusCellStyle"] as Style;
                }
                catch { }
            }

            Utils.RestoreAccountsLayout(AccountsGridControl);
            Utils.RestoreOperationsLayout(OperationsGridControl);
        }

        private void AddCheckButton_Click(object sender, RoutedEventArgs e)
        {
            AddAccount();
        }

        /// <summary>
        /// Добавить аккаунт
        /// </summary>
        private void AddAccount()
        {
            var w = new SetAccountWindow(-1, _userID);
            w.Owner = this;
            if (w.ShowDialog() == true)
            {
                FillAccounts(null);
                Utils.UpdateRatingBanner(_banners, _userID);
            }
       }

        private void AccountsGridControlView_FocusedRowChanged(object sender, DevExpress.Xpf.Grid.FocusedRowChangedEventArgs e)
        {
            if (e.NewRow != null)
            {
                var a = e.NewRow as DataRowView;
                _selectedAccountId = Convert.ToInt32(a["ID"]);

                FillOperations();
            }
            else
            {
                _selectedAccountId = -1;
            }
        }

        private void EditAccountButton_Click(object sender, RoutedEventArgs e)
        {
            EditAccount();
        }

        /// <summary>
        /// Редактирование счета
        /// </summary>
        private void EditAccount()
        {
            if (!Database.HintCheckNoAccounts(_userID))
            {
                int selOperationID = -1;
                var selRows = OperationsGridControl.GetSelectedRowHandles();
                if (selRows.Length > 0) selOperationID = selRows[0];

                int selAccountID = -1;
                selRows = AccountsGridControl.GetSelectedRowHandles();
                if (selRows.Length > 0) selAccountID = selRows[0];

                var w = new SetAccountWindow(_selectedAccountId, _userID);
                w.Owner = this;
                if (w.ShowDialog() == true)
                {
                    FillAccounts(null);
                    AccountsGridControlView.FocusedRowHandle = selAccountID;
                    OperationsGridControlView.FocusedRowHandle = selOperationID;
                }
            }
        }

        private void AccountsGridControlView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int selOperationID = -1;
            var selRows = OperationsGridControl.GetSelectedRowHandles();
            if (selRows.Length > 0) selOperationID = selRows[0];

            int selAccountID = -1;
            selRows = AccountsGridControl.GetSelectedRowHandles();
            if (selRows.Length > 0) selAccountID = selRows[0];

            DataGrid g = sender as DataGrid;
            String type = e.Device.Target.GetType().ToString();
            if (type.Equals("System.Windows.Controls.TextBlock"))
            {
                var w = new SetAccountWindow(_selectedAccountId, _userID);
                w.Owner = this;
                if (w.ShowDialog() == true)
                {
                    FillAccounts(null);
                }
            }

            AccountsGridControlView.FocusedRowHandle = selAccountID;
            OperationsGridControlView.FocusedRowHandle = selOperationID;
        }
        #endregion

        #region Operations

        private void EditOperationButton_Click(object sender, RoutedEventArgs e)
        {
            EditOperation();
        }

        private void OperationsGridControlView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditOperation();
        }

        /// <summary>
        /// Функция редактирования операции
        /// </summary>
        private void EditOperation()
        {
            int selOperationsIndex = -1;
            var selRows = OperationsGridControl.GetSelectedRowHandles();
            if (selRows.Length > 0) selOperationsIndex = selRows[0];

            int selAccountsIndex = -1;
            selRows = AccountsGridControl.GetSelectedRowHandles();
            if (selRows.Length > 0) selAccountsIndex = selRows[0];

            if (_selectedOperationId != -1)
            {
                EditOperationWindow w = new EditOperationWindow(_userID, _selectedOperationId, Margin.Top, Margin.Left, _selectedAccountId, _hintWindow);
                w.Owner = this;

                if (w.ShowDialog() == true)
                {
                    var selCheckID = _selectedAccountId;
                    _selectedAccountId = selCheckID;

                    FillAccounts(null);

                    AccountsGridControlView.FocusedRowHandle = selAccountsIndex;
                    OperationsGridControlView.FocusedRowHandle = selOperationsIndex;
                }
            }
        }

        private void OperationsGridControlView_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            if (e.NewRow != null)
            {
                DataRowView o = (DataRowView)e.NewRow;
                _selectedOperationId = G._I(o["ID"]);
                _selectedOperationStatus = G._I(o["Status"]);

                 if (G._I(o["Credit_ID"]) == _selectedAccountId)
                     _selectedOperationCredit = 1;
                 else
                     _selectedOperationCredit = 0;
            }
            else
                _selectedOperationId = -1;
        }

        private void ExecuteOperation_Click(object sender, RoutedEventArgs e)
        {
            int selOperationsIndex = -1;
            var selRows = OperationsGridControl.GetSelectedRowHandles();
            if (selRows.Length > 0) selOperationsIndex = selRows[0];

            int selAccountsIndex = -1;
            selRows = AccountsGridControl.GetSelectedRowHandles();
            if (selRows.Length > 0) selAccountsIndex = selRows[0];

            if (_selectedOperationStatus == 0)
            {
                if (_selectedOperationCredit == 1)
                {
                    Database.CarryCreditOperation(_selectedOperationId);
                    CheckDB(G.LastError);
                    FillAccounts(null);
                }
                else
                {
                    Database.CarryDebetOperation(_selectedOperationId);
                    CheckDB(G.LastError);
                    FillAccounts(null);
                }
            }

            AccountsGridControlView.FocusedRowHandle = selAccountsIndex;
            OperationsGridControlView.FocusedRowHandle = selOperationsIndex;
        }

        private void AddOperation_Click(object sender, RoutedEventArgs e)
        {
            AddOperation(Consts.OperationType.Credit);
        }

        private void EditOperation_Click(object sender, RoutedEventArgs e)
        {
            EditOperation();
        }

        private void DeleteOperation_Click(object sender, RoutedEventArgs e)
        {
            DeleteOperation();
        }

        private void Version_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new About(this);
            aboutWindow.ShowDialog();
        }

        private void Registration_Click(object sender, RoutedEventArgs e)
        {
            var regWindow = new RegistrationWindow();
            regWindow.Owner = this;
            regWindow.ShowDialog();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void FillOperations()
        {
            OperationsGridControl.ItemsSource = null;

            var dtFrom = (DateTime)AccountsDownDatePicker.EditValue;
            var dtTo = (DateTime)AccountsUpDatePicker.EditValue;

            var operations = Database.GetOperationTable(dtFrom, dtTo, _selectedAccountId);
            CheckDB(G.LastError);

            OperationsGridControl.ItemsSource = operations;
        }

        #endregion

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Row1.MaxHeight = ActualHeight - 300;
        }

        private void btnDeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelectedAccount();
        }

        private void AccountsGridControlView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
                DeleteSelectedAccount();
            else if (e.Key == Key.Enter)
                EditAccount();
        }

        /// <summary>
        /// Удалить выделенный счет
        /// </summary>
        private void DeleteSelectedAccount()
        {
            int res = -1;
            Int32.TryParse(G._S(G.db_select("exec ExistOperationsInAccount {1}", _selectedAccountId)), out res);
            if (res == 2)
            {  //нет операций по данному счету
                int selAccountID = -1;
                var selRows = AccountsGridControl.GetSelectedRowHandles();
                if (selRows.Length > 0) selAccountID = selRows[0];

                if (selAccountID != -1)
                {
                    if (MessageBox.Show("Вы действительно хотите удалить выделенный счет?", "Подтверждение", MessageBoxButton.YesNoCancel, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        G.db_exec("DeleteAccount {1}", _selectedAccountId);
                        CheckDB(G.LastError);
                        FillAccounts(null);
                    }
                }
            }
            else if (res == 1)
            {  //есть операции по данному счету
                MessageBox.Show("Невозможно удалить счет.\nУдалите операции, проведенные по счету, прежде, чем удалять счет.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DeleteOperation();
        }

        private void OperationsGridControlView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
                DeleteOperation();
            else if (e.Key == Key.Enter)
                EditOperation();
        }

        /// <summary>
        /// Удалить операцию
        /// </summary>
        private void DeleteOperation()
        {
            int selAccountsIndex = -1;
            var selRows = AccountsGridControl.GetSelectedRowHandles();
            if (selRows.Length > 0) selAccountsIndex = selRows[0];

            if (MessageBox.Show("Вы действительно хотите удалить операцию №"+_selectedOperationId+"?", 
                                "Удаление операции", 
                                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                if (_selectedOperationId != -1)
                {
                    G.db_exec("DeleteOperation {1}", _selectedOperationId);
                    CheckDB(G.LastError);
                }
                FillAccounts(null);

                AccountsGridControlView.FocusedRowHandle = selAccountsIndex;
                if (OperationsGridControl.VisibleRowCount > 0)
                    OperationsGridControlView.FocusedRowHandle = 0;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveGridToSomeFormat(OperationsGridControl);
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            var fileName = String.Format("{0}\\{1}.html", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                                          "temp_extract");
            OperationsGridControlView.ExportToHtml(fileName);

            var sr = new StreamReader(fileName);
            var html = sr.ReadToEnd();
            sr.Close();
            var account = Database.GetAccount(_selectedAccountId);
            html = Utils.InsertHeaderInHtml(html, account.Name, (DateTime?)AccountsDownDatePicker.EditValue, (DateTime?)AccountsUpDatePicker.EditValue);

            printWebBrowser.NavigateToString(html);

            this.Cursor = Cursors.Arrow;
        }

        void printWebBrowser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            try
            {
                var doc = printWebBrowser.Document as mshtml.IHTMLDocument2;
                const string keyName = @"Software\Microsoft\Internet Explorer\PageSetup";
                using (var key = Registry.CurrentUser.OpenSubKey(keyName, true))
                {
                    if (key != null)
                    {
                        var oldFooter = key.GetValue("footer");
                        var oldHeader = key.GetValue("header");
                        key.SetValue("footer", "");
                        key.SetValue("header", "");
                        doc.execCommand("Print", true, null);
                        key.SetValue("footer", oldFooter);
                        key.SetValue("header", oldHeader);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Напечатать документ не удалось.\n" + ex.Message, "Возникла ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveGridToSomeFormat(GridControl gridControl)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Выписка по счету";
            saveFileDialog.Filter = @"
Документ CSV (*.csv) | *.csv|
Web-страница (*.html) | *.html|
Изображение (*.bmp, *.jpg) | *.bmp;*.jpg|
Содержимое web-страницы (*.mht) | *.mht|
Документ PDF (*.pdf) | *.pdf|
Документ RTF (*.rtf) | *.rtf|
Текстовый документ (*.txt) | *.txt|
Лист Microsoft Excel (*.xls) | *.xls|
Документ Open XML Microsoft Excel (*.xlsx) | *.xlsx|
Документ XPS (*.xps) | *.xps
";

            if (saveFileDialog.ShowDialog() == true)
            {
                var file = new FileInfo(saveFileDialog.FileName);

                try
                {
                    switch (file.Extension.ToLower())
                    {
                        case ".csv":
                            OperationsGridControlView.ExportToCsv(saveFileDialog.FileName);
                            break;
                        case ".html":
                            OperationsGridControlView.ExportToHtml(saveFileDialog.FileName);
                            break;
                        case ".bmp":
                            OperationsGridControlView.ExportToImage(saveFileDialog.FileName);
                            break;
                        case ".jpg":
                            OperationsGridControlView.ExportToImage(saveFileDialog.FileName);
                            break;
                        case ".mht":
                            OperationsGridControlView.ExportToMht(saveFileDialog.FileName);
                            break;
                        case ".pdf":
                            OperationsGridControlView.ExportToPdf(saveFileDialog.FileName);
                            break;
                        case ".rtf":
                            OperationsGridControlView.ExportToRtf(saveFileDialog.FileName);
                            break;
                        case ".txt":
                            OperationsGridControlView.ExportToText(saveFileDialog.FileName);
                            break;
                        case ".xls":
                            OperationsGridControlView.ExportToXls(saveFileDialog.FileName, new DevExpress.XtraPrinting.XlsExportOptions(DevExpress.XtraPrinting.TextExportMode.Text));
                            break;
                        case ".xlsx":
                            OperationsGridControlView.ExportToXlsx(saveFileDialog.FileName, new DevExpress.XtraPrinting.XlsxExportOptions(DevExpress.XtraPrinting.TextExportMode.Text));
                            break;
                        case ".xps":
                            OperationsGridControlView.ExportToXps(saveFileDialog.FileName);
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Возникла ошибка во время генерации выписки.\n" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddOperation(Consts.OperationType.Credit);
        }

        /// <summary>
        /// Обновить список счетов и операций
        /// </summary>
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void Refresh()
        {
            int selAccountID = -1;
            var selRows = AccountsGridControl.GetSelectedRowHandles();
            if (selRows.Length > 0) selAccountID = selRows[0];

            FillAccounts(null);

            AccountsGridControlView.FocusedRowHandle = selAccountID;
        }

        /// <summary>
        /// Добавить операцию
        /// </summary>
        private void AddOperation(Consts.OperationType type)
        {
            if (!Database.HintCheckNoAccounts(_userID))
            {
                int selAccountsIndex = -1;
                var selRows = AccountsGridControl.GetSelectedRowHandles();
                if (selRows.Length > 0) selAccountsIndex = selRows[0];

                var w = new AddOperationWindow(_userID, -1, Margin.Top, Margin.Left, _selectedAccountId, type, _hintWindow);
                w.Owner = this;
                
                if (w.ShowDialog() == true)
                {
                    var selCheckID = _selectedAccountId;
                    _selectedAccountId = selCheckID;

                    Utils.UpdateRatingBanner(_banners, _userID); 

                    FillAccounts(null);

                    AccountsGridControlView.FocusedRowHandle = selAccountsIndex;
                }
            }
        }

        /// <summary>
        /// Добавить аккаунт
        /// </summary>
        private void AccountAddOperation_Click(object sender, RoutedEventArgs e)
        {
            AddAccount();
        }

        /// <summary>
        /// Редактировать аккаунт
        /// </summary>
        private void AccountEditOperation_Click(object sender, RoutedEventArgs e)
        {
            EditAccount();
        }

        /// <summary>
        /// Удалить аккаунт
        /// </summary>
        private void AccountDeleteOperation_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelectedAccount();
        }

        /// <summary>
        /// Списать средства
        /// </summary>
        private void TransferDebetOperation_Click(object sender, RoutedEventArgs e)
        {
            AddOperation(Consts.OperationType.Debet);
        }

        /// <summary>
        /// Зачислить средства
        /// </summary>
        private void TransferCreditOperation_Click(object sender, RoutedEventArgs e)
        {
            AddOperation(Consts.OperationType.Credit);
        }

        #region Запоминание разметки таблиц и дат периода

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Utils.SaveAccountsLayout(AccountsGridControl);
            Utils.SaveOperationsLayout(OperationsGridControl);

            Utils.SaveDates((DateTime)AccountsDownDatePicker.EditValue, (DateTime)AccountsUpDatePicker.EditValue);
            Utils.SaveFormCoords(this);

            if (MainGrid.RowDefinitions[0].Height.Value > MainGrid.RowDefinitions[0].MinHeight)
                Utils.SaveSplitterDist(MainGrid.RowDefinitions[0].Height.Value);

            var balAccRepCtrl = (balanceAccountsGrid.Children[0] as BalanceAccountsReportControl);
            //var balCatRepCtrl = (balanceCategoriesGrid.Children[0] as BalanceCategoriesReportControl);
            Utils.SaveReportAccountsDates((DateTime)balAccRepCtrl.BalanceDownDatePicker.EditValue, (DateTime)balAccRepCtrl.BalanceUpDatePicker.EditValue);
            //Utils.SaveReportCategoriesDates((DateTime)balCatRepCtrl.BalanceDownDatePicker.EditValue, (DateTime)balCatRepCtrl.BalanceUpDatePicker.EditValue);

            if (_fileLoader != null)
            {
                if (_fileLoader.State != FileLoader.LoaderState.Finished)
                    this.Hide();

                while (_fileLoader.State != FileLoader.LoaderState.Finished)
                { }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Database.UserLogOut(_userID);
        }

        #endregion Запоминание разметки таблиц и дат периода

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
                Refresh();
            if (e.Key == Key.F1)
                Version_Click(null, null);
        }

        private void ConnectToDatabase()
        {
            SqlConnection con = null;
            try
            {
                am.DB.DBManager.Instance.Init(Encryption.DecryptString(Properties.Settings.Default.ConnectionString, "JPo7R75zgJyg315d"), 90, 90);
                con = am.DB.DBManager.Instance.CreateConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                                "Ошибка соединения с базой данных",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                Close();
                return;
            }
        }

        private void StatusPlannedMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ChangeOperationStatus(1);
        }

        private void StatusEditedMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ChangeOperationStatus(0);
        }

        private void StatusCarriedOutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ChangeOperationStatus(8);
        }

        private void ChangeOperationStatus(int newStatusID)
        {
            int selOperationsIndex = -1;
            var selRows = OperationsGridControl.GetSelectedRowHandles();
            if (selRows.Length > 0) selOperationsIndex = selRows[0];

            int selAccountsIndex = -1;
            selRows = AccountsGridControl.GetSelectedRowHandles();
            if (selRows.Length > 0) selAccountsIndex = selRows[0];

            G.db_exec("ChangeOperationStatus {1}, {2}", _selectedOperationId, newStatusID);
            CheckDB(G.LastError);
            FillAccounts(null);

            AccountsGridControlView.FocusedRowHandle = selAccountsIndex;
            OperationsGridControlView.FocusedRowHandle = selOperationsIndex;
        }

#region Backup
        Splash _bac_splash;
        /// <summary>
        /// Полное резервное копирование счетов и операций
        /// </summary>
        private void backupMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.DefaultExt = ".bgt";
            dlg.Filter = "Budget backup documents (.bgt)|*.bgt";
            if (dlg.ShowDialog() == true)
            {
                _bac_splash = new Splash("Идет создание бэкапа ...");
                _bac_splash.Show();
                _bac_splash.Top = Top + Height / 2 - _bac_splash.Height / 2;
                _bac_splash.Left = Left + Width / 2 - _bac_splash.Width / 2;

                Tools.Backup.OnProgress += BackupProgress;
                Task.Run(() =>
                {
                    Tools.Backup.SaveToJson(dlg.FileName);
                });
            }
        }
        void BackupProgress(string msg)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (msg == "3") _bac_splash.Show();
                if (msg == "2") _bac_splash.Hide();
                if (msg == "1") Refresh();
                if (msg == "0")
                {
                    Tools.Backup.OnProgress -= BackupProgress;

                    _bac_splash.Close();
                    _bac_splash = null;
                    return;
                }
                _bac_splash.SetText(msg);
            }));
        }

        /// <summary>
        /// Восстановить счета и операции из файла
        /// </summary>
        private void restoreMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.DefaultExt = ".bgt";
            dlg.Filter = "Budget backup files (.bgt)|*.bgt";
            if (dlg.ShowDialog() == true)
            {
                if (MessageBox.Show(@"
                        Вы действительно хотите восстановить все счета и операции из бэкапа?
                        Внимание! Какие-то данные могут быть потеряны!", 
                        "Подтверждение", 
                        MessageBoxButton.YesNoCancel, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    _bac_splash = new Splash("Идет восстановление...");
                    _bac_splash.Show();

                    Tools.Backup.OnProgress += BackupProgress;
                    Task.Run(() =>
                    {
                        Tools.Backup.RestoreFromJson(dlg.FileName);
                    });
                }
            }
        }
        #endregion

        /// <summary>
        /// Поделиться в ВКонтакте
        /// </summary>
        private void imgShareVK_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var vkLoginWindow = new VKLogin(textToShare);
            vkLoginWindow.Owner = this;
            vkLoginWindow.ShowDialog();
        }

        /// <summary>
        /// Поделиться в Facebook
        /// </summary>
        private void imgShareFacebook_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var facebookLoginWindow = new FacebookLogin(textToShare);
            facebookLoginWindow.Owner = this;
            facebookLoginWindow.ShowDialog();
        }

        /// <summary>
        /// Поделиться по емейл
        /// </summary>
        private void imgShareEmail_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var emailShareWindow = new SendEmail(textToShare);
            emailShareWindow.Owner = this;
            emailShareWindow.ShowDialog();
        }

        private void bannerImg_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start(_nowBanner.Url);
        }

        /// <summary>
        /// Клик меню "Поддержка"
        /// </summary>
        private void supportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://dev.ultrazoom.ru/ProductsDescription/BudgetHistory");
        }

        /// <summary>
        /// Клик меню "Зарегистрироваться"
        /// </summary>
        private void registerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new Register(_userID);
            registerWindow.Owner = this;
            registerWindow.OnRegistered += () => { Utils.UpdateRatingBanner(_banners, _userID); };
            registerWindow.ShowDialog();
        }

        private void restReCalcMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var recalcWindow = new RecalcWindow(_userID);
            recalcWindow.Owner = this;
            recalcWindow.ShowDialog();
        }

        private void AccountsDownDatePicker_SelectedDateChanged(object sender, EditValueChangedEventArgs editValueChangedEventArgs)
        {
            if (AccountsDownDatePicker.EditValue != null && AccountsUpDatePicker.EditValue != null)
            {
                var d1 = (DateTime)AccountsDownDatePicker.EditValue;
                var d2 = (DateTime)AccountsUpDatePicker.EditValue;
                if (d1 > d2) AccountsUpDatePicker.EditValue = d1;

                datePickerOperationsFrom.EditValue = AccountsDownDatePicker.EditValue;
                datePickerOperationsTo.EditValue = AccountsUpDatePicker.EditValue;

                btnRefresh_Click(null, null);
            }
        }

        private void AccountsUpDatePicker_SelectedDateChanged(object sender, EditValueChangedEventArgs editValueChangedEventArgs)
        {
            if (AccountsDownDatePicker.EditValue != null && AccountsUpDatePicker.EditValue != null)
            {
                var d1 = (DateTime)AccountsDownDatePicker.EditValue;
                var d2 = (DateTime)AccountsUpDatePicker.EditValue;
                if (d1 > d2) AccountsDownDatePicker.EditValue = d2;

                datePickerOperationsFrom.EditValue = AccountsDownDatePicker.EditValue;
                datePickerOperationsTo.EditValue = AccountsUpDatePicker.EditValue;

                btnRefresh_Click(null, null);
            }
        }

#region Все операции tab
        private void datePickerOperationsFrom_SelectedDateChanged(object sender, EditValueChangedEventArgs e)
        {
            if (datePickerOperationsFrom.EditValue != null && datePickerOperationsTo.EditValue != null)
            {
                var d1 = (DateTime)datePickerOperationsFrom.EditValue;
                var d2 = (DateTime)datePickerOperationsTo.EditValue;
                if (d1 > d2) datePickerOperationsTo.EditValue = d1;

                //btnRefreshOperations_Click(null, null);
            }
        }

        private void datePickerOperationsTo_SelectedDateChanged(object sender, EditValueChangedEventArgs e)
        {
            if (datePickerOperationsFrom.EditValue != null && datePickerOperationsTo.EditValue != null)
            {
                var d1 = (DateTime)datePickerOperationsFrom.EditValue;
                var d2 = (DateTime)datePickerOperationsTo.EditValue;
                if (d1 > d2) datePickerOperationsFrom.EditValue = d2;

                //btnRefreshOperations_Click(null, null);
            }
        }

        private void btnRefreshOperations_Click(object sender, RoutedEventArgs e)
        {
            gridControlAllOperations.ItemsSource = null;

            var dtFrom = (DateTime)datePickerOperationsFrom.EditValue;
            var dtTo = (DateTime)datePickerOperationsTo.EditValue;

            var operations = Database.GetOperationTable(dtFrom, dtTo, 0);
            CheckDB(G.LastError);

            gridControlAllOperations.ItemsSource = operations;

            gridControlAllOperations.Columns["ID"].Visible = false;
            gridControlAllOperations.Columns["Debet_ID"].Visible = false;
            gridControlAllOperations.Columns["OperDay_ID"].Visible = false;
            gridControlAllOperations.Columns["Credit_ID"].Visible = false;
            gridControlAllOperations.Columns["Amount"].Visible = false;
            //gridControlAllOperations.Columns["Status"].Visible = false;
            gridControlAllOperations.Columns["DateCreate"].Visible = false;

            foreach (var col in gridControlAllOperations.Columns)
                col.AllowFocus = false;

            gridControlAllOperations.Columns["FirstAccount"].Header = "1-ый счет";
            gridControlAllOperations.Columns["FirstAccount"].VisibleIndex = 0;
            gridControlAllOperations.Columns["SeconAccount"].Header = "2-ой счет";
            gridControlAllOperations.Columns["SeconAccount"].VisibleIndex = 1;
            gridControlAllOperations.Columns["OperDay"].Header = "ОперДень";
            gridControlAllOperations.Columns["OperDay"].VisibleIndex = 2;
            gridControlAllOperations.Columns["Debet"].Header = "Списание";
            gridControlAllOperations.Columns["Debet"].VisibleIndex = 3;
            gridControlAllOperations.Columns["Credit"].Header = "Зачисление";
            gridControlAllOperations.Columns["Credit"].VisibleIndex = 4;
            gridControlAllOperations.Columns["Category"].Header = "Категория";
            gridControlAllOperations.Columns["Category"].VisibleIndex = 5;
            gridControlAllOperations.Columns["Description"].Header = "Описание";
            gridControlAllOperations.Columns["Description"].VisibleIndex = 6;

            gridControlAllOperations.Columns["Amount"].Header = "Сумма";
            gridControlAllOperations.Columns["DateCreate"].Header = "Дата создания";
            gridControlAllOperations.Columns["Status"].Header = "Статус";
            gridControlAllOperations.Columns["Status"].VisibleIndex = 7;

            gridControlAllOperations.Columns["Status"].EditSettings = Statuses;

            gridControlAllOperations.Columns["Debet"].CellTemplate = gridControlAllOperations.Resources["debetCellTemplate"] as DataTemplate;
            gridControlAllOperations.Columns["Credit"].CellTemplate = gridControlAllOperations.Resources["creditCellTemplate"] as DataTemplate;

            gridControlAllOperations.Columns["Debet"].CellStyle = gridControlAllOperations.Resources["debetCellStyle"] as Style;
            gridControlAllOperations.Columns["Credit"].CellStyle = gridControlAllOperations.Resources["creditCellStyle"] as Style;
            gridControlAllOperations.Columns["Status"].CellStyle = gridControlAllOperations.Resources["statusCellStyle"] as Style;

            gridControlViewOperations.BestFitColumns();

            //Utils.RestoreAllOperationsLayout(gridControlAllOperations);

        }

        private void gridControlOperations_LayoutUpdated(object sender, EventArgs e)
        {

        }

        private void gridControlViewOperations_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {

        }

        private void gridControlViewOperations_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void gridControlViewOperations_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        #endregion

        private void bannerTextBlock_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!String.IsNullOrEmpty(_nowBanner.Url))
                Process.Start(_nowBanner.Url);
        }

        /// <summary>
        /// Открыть подсказки
        /// </summary>
        private void hintsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_hintWindow != null)
            {
                _hintWindow.LoadNext();
                _hintWindow.ShowDialog();
            }
        }

        /// <summary>
        /// Перезайти под другим аккаунтом
        /// </summary>
        private void ReloginMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var login = Utils.AutoLoginExit();
            var w = new EntryWindow();
            w.SetInitLogin(login);
            this.Close();
            w.Show();
        }

        #region Тема общего вида

        /// <summary>
        /// Тема "VS2019Dark"
        /// </summary>
        private void bViewVS2019Dark_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            UncheckAllViewMenuItemsBut(bViewVS2019Dark);
            SetTheme(Theme.VS2019Dark);
            rectangleBanner.Fill = new SolidColorBrush(Consts.BannerColor[Theme.VS2019Dark]);
            bannerTextBlock.Foreground = new SolidColorBrush(Consts.BannerTextFontColor[Theme.VS2019Dark]);

            Properties.Settings.Default.ThemeIndex = 1;
        }

        /// <summary>
        /// Тема "DXStyle"
        /// </summary>
        private void bViewDXStyle_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            UncheckAllViewMenuItemsBut(bViewDXStyle);
            SetTheme(Theme.DXStyle);
            rectangleBanner.Fill = new SolidColorBrush(Consts.BannerColor[Theme.DXStyle]);
            bannerTextBlock.Foreground = new SolidColorBrush(Consts.BannerTextFontColor[Theme.DXStyle]);

            Properties.Settings.Default.ThemeIndex = 2;
        }

        /// <summary>
        /// Тема "LightGray"
        /// </summary>
        private void bViewLightGray_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            UncheckAllViewMenuItemsBut(bViewLightGray);
            SetTheme(Theme.LightGray);
            rectangleBanner.Fill = new SolidColorBrush(Consts.BannerColor[Theme.LightGray]);
            bannerTextBlock.Foreground = new SolidColorBrush(Consts.BannerTextFontColor[Theme.LightGray]);

            Properties.Settings.Default.ThemeIndex = 3;
        }

        /// <summary>
        /// Тема "Office2007Black"
        /// </summary>
        private void bViewOffice2007Black_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            UncheckAllViewMenuItemsBut(bViewOffice2007Black);
            SetTheme(Theme.Office2007Black);
            rectangleBanner.Fill = new SolidColorBrush(Consts.BannerColor[Theme.Office2007Black]);
            bannerTextBlock.Foreground = new SolidColorBrush(Consts.BannerTextFontColor[Theme.Office2007Black]);

            Properties.Settings.Default.ThemeIndex = 4;
        }

        /// <summary>
        /// Тема "Office2007Blue"
        /// </summary>
        private void bViewOffice2007Blue_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            UncheckAllViewMenuItemsBut(bViewOffice2007Blue);
            SetTheme(Theme.Office2007Blue);
            rectangleBanner.Fill = new SolidColorBrush(Consts.BannerColor[Theme.Office2007Blue]);
            bannerTextBlock.Foreground = new SolidColorBrush(Consts.BannerTextFontColor[Theme.Office2007Blue]);

            Properties.Settings.Default.ThemeIndex = 5;
        }

        /// <summary>
        /// Тема "Office2007Silver"
        /// </summary>
        private void bViewOffice2007Silver_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            UncheckAllViewMenuItemsBut(bViewOffice2007Silver);
            SetTheme(Theme.Office2007Silver);
            rectangleBanner.Fill = new SolidColorBrush(Consts.BannerColor[Theme.Office2007Silver]);
            bannerTextBlock.Foreground = new SolidColorBrush(Consts.BannerTextFontColor[Theme.Office2007Silver]);

            Properties.Settings.Default.ThemeIndex = 6;
        }

        /// <summary>
        /// Тема "Seven"
        /// </summary>
        private void bViewSeven_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            UncheckAllViewMenuItemsBut(bViewSeven);
            SetTheme(Theme.Seven);
            rectangleBanner.Fill = new SolidColorBrush(Consts.BannerColor[Theme.Seven]);
            bannerTextBlock.Foreground = new SolidColorBrush(Consts.BannerTextFontColor[Theme.Seven]);
        
            Properties.Settings.Default.ThemeIndex = 7;
        }

        /// <summary>
        /// Тема "VS2010"
        /// </summary>
        private void bViewVS2010_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            UncheckAllViewMenuItemsBut(bViewVS2010);
            SetTheme(Theme.Office2019Colorful);
            rectangleBanner.Fill = new SolidColorBrush(Consts.BannerColor[Theme.Office2019Colorful]);
            bannerTextBlock.Foreground = new SolidColorBrush(Consts.BannerTextFontColor[Theme.Office2019Colorful]);

            Properties.Settings.Default.ThemeIndex = 8;
        }
        private void bViewWin10_ItemClick(object sender, ItemClickEventArgs e)
        {
            UncheckAllViewMenuItemsBut(bViewWin10);
            SetTheme(Theme.Win10Light);
            rectangleBanner.Fill = new SolidColorBrush(Consts.BannerColor[Theme.Win10Light]);
            bannerTextBlock.Foreground = new SolidColorBrush(Consts.BannerTextFontColor[Theme.Win10Light]);

            Properties.Settings.Default.ThemeIndex = 9;
        }


        /// <summary>
        /// Установить тему при входе в программу
        /// </summary>
        private void SetSavedTheme()
        {
            int themeIdx = Properties.Settings.Default.ThemeIndex; if (themeIdx == 0) themeIdx = 1;
            var theme = Consts.DevExTheme[themeIdx];

            if (themeIdx > 0 && themeIdx <= Consts.DevExTheme.Count)
                SetTheme(Consts.DevExTheme[themeIdx]);

            if (theme == Theme.VS2019Dark)
                UncheckAllViewMenuItemsBut(bViewVS2019Dark);
            else if (theme == Theme.DXStyle)
                UncheckAllViewMenuItemsBut(bViewDXStyle);
            else if (theme == Theme.LightGray)
                UncheckAllViewMenuItemsBut(bViewLightGray);
            else if (theme == Theme.Office2007Black)
                UncheckAllViewMenuItemsBut(bViewOffice2007Black);
            else if (theme == Theme.Office2007Blue)
                UncheckAllViewMenuItemsBut(bViewOffice2007Blue);
            else if (theme == Theme.Office2007Silver)
                UncheckAllViewMenuItemsBut(bViewOffice2007Silver);
            else if (theme == Theme.Seven)
                UncheckAllViewMenuItemsBut(bViewSeven);
            else if (theme == Theme.Office2019Colorful)
                UncheckAllViewMenuItemsBut(bViewVS2010);

            rectangleBanner.Fill = new SolidColorBrush(Consts.BannerColor[theme]);
            bannerTextBlock.Foreground = new SolidColorBrush(Consts.BannerTextFontColor[theme]);
        }

        /// <summary>
        /// Установить определенную тему
        /// </summary>
        private void SetTheme(Theme devExTheme)
        {
            int ind = Consts.DevExTheme.IndexOf(t => t.Value == devExTheme) + 1;

            ThemeManager.SetTheme(this, devExTheme);
            if (_hintWindow != null)
                ThemeManager.SetTheme(_hintWindow, devExTheme);
            if (balanceAccountsGrid.Children != null)
            {
                ThemeManager.SetTheme(balanceAccountsGrid.Children[0], devExTheme);
                ThemeManager.SetTheme(balanceCategoriesGrid.Children[0], devExTheme);
            }
            if (ind != -1)
            {
                Properties.Settings.Default.ThemeIndex = ind;
                Properties.Settings.Default.Save();
            }

            CommandBroker.SendCommand(1);
        }

        /// <summary>
        /// Снять галочки со всех тем и установить новую
        /// </summary>
        private void UncheckAllViewMenuItemsBut(BarCheckItem bChItem)
        {
            bViewVS2019Dark.IsChecked = false;
            bViewDXStyle.IsChecked = false;
            bViewLightGray.IsChecked = false;
            bViewOffice2007Black.IsChecked = false;
            bViewOffice2007Blue.IsChecked = false;
            bViewOffice2007Silver.IsChecked = false;
            bViewSeven.IsChecked = false;
            bViewVS2010.IsChecked = false;
            bViewWin10.IsChecked = false;

            bChItem.IsChecked = true;
        }

        #endregion Тема общего вида

        private void DXWindow_ContentRendered(object sender, EventArgs e)
        {
          //Окно подсказок
          _hintWindow = new InfoWindow() { Owner = this, Topmost = true };
          if (!NoAccountsShowHint())  //Добавление счета
            NoOperationsShowHint();  //Добавление операции
        }

        /// <summary>
        /// Оставить комментарий
        /// </summary>
        private void bComment_ItemClick(object sender, ItemClickEventArgs e)
        {
            var w = new AddComment(_userID);
            w.Owner = this;
            
            if (w.ShowDialog() == true)
            {
                Utils.UpdateRatingBanner(_banners, _userID);
            }
        }

        /// <summary>
        /// Сохранение разметки таблицы аккаунтов
        /// </summary>
        private void AccountsGridControl_LayoutUpdated(object sender, EventArgs e)
        {
            //Utils.SaveAccountsLayout(AccountsGridControl);
        }

        /// <summary>
        /// Сохранение разметки таблицы операций
        /// </summary>
        private void OperationsGridControl_LayoutUpdated(object sender, EventArgs e)
        {
            //Utils.SaveOperationsLayout(OperationsGridControl);
        }

        /// <summary>
        /// Планирование бюджета
        /// </summary>
        private void bPlanning_Click(object sender, ItemClickEventArgs e)
        {
            var w = new BudgetPlanning(_userID);
            w.Owner = this;
            w.OnAccountsEdited += () =>
                {
                    FillAccounts(null);
                };
            w.Show();
        }

        /// <summary>
        /// Дублировать операцию
        /// </summary>
        private void DuplicateOperationMenuItem_Click(object sender, ItemClickEventArgs e)
        {
            if (_selectedOperationId != -1)
            {
                var res = Database.DuplicateOperation(_selectedOperationId);
                CheckDB(G.LastError);

                if (res > 0)  //успешно
                    FillAccounts(AccountsGridControlView.FocusedRowHandle);
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
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            var w = new PossibleCategoriesWindow(_userID);
            w.Owner = this;
            w.Show();
        }

        #region Возможные категории и аккаунты

        private void possibleCategoriesMenuItem_Click(object sender, ItemClickEventArgs e)
        {
            new PossibleCategoriesWindow(_userID) { Owner = this }.Show();
        }


        /// <summary>
        /// Открыть редактор категорий
        /// </summary>
        private void categoriesMenuItem_Click(object sender, ItemClickEventArgs e)
        {
            var w = new WindowCatEdit(_userID, Consts.OperationType.None, 1, 0, -1, _hintWindow);
            w.Owner = this;
            w.ShowDialog();
        }

        private void possibleAccountsMenuItem_Click(object sender, ItemClickEventArgs e)
        {
            var w = new PossibleAccountsWindow(_userID) { Owner = this };
            w.OnAccountsAdded += () =>
            {
                FillAccounts(null);
            };
            w.Show();
        }

        #endregion Возможные категории и аккаунты

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

        private void mnuAddHelper_Click(object sender, ItemClickEventArgs e)
        {
            var w = new Users.DlgRegisterHelper();
            w.Owner = this;
            w.ShowDialog();
        }

    }

    /// <summary>
    /// Раскраска ячеек грида в зависимости от типа состояния
    /// </summary>
    public class StateConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var statusInd = (int)value;

            if (statusInd == 0)
                return Brushes.White;
            else if (statusInd == 1)
                return Brushes.Yellow;
            else if (statusInd == 8)
                return Brushes.Green;

            return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        { return null; }  //не используется

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}

