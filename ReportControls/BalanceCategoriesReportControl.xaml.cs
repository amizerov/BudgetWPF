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
using am.BL;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Windows.Markup;
using System.Globalization;
using DevExpress.Xpf.Core;

namespace Budget.ReportControls
{
    public partial class BalanceCategoriesReportControl : UserControl
    {
        private int _userID;

        public BalanceCategoriesReportControl(int userID)
        {
            InitializeComponent();
            _userID = userID;

            DateTime fromDate, toDate;
            Utils.RestoreReportCategoriesDates(out fromDate, out toDate);
            BalanceDownDatePicker.EditValue = fromDate;
            BalanceUpDatePicker.EditValue = toDate;

            if (Properties.Settings.Default.ThemeIndex > 0 && Properties.Settings.Default.ThemeIndex <= Consts.DevExTheme.Count)
                ThemeManager.SetTheme(this, Consts.DevExTheme[Properties.Settings.Default.ThemeIndex]);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ConnectToDatabase();
        }

        private void btnGetReport_Click(object sender, RoutedEventArgs e)
        {
            FillReport();
        }

        private void FillReport()
        {
            var dtDown = (DateTime)BalanceDownDatePicker.EditValue;
            var dtUp = (DateTime)BalanceUpDatePicker.EditValue;
            var dateDown = String.Format("{0}-{1:00}-{2:00} 00:00:00", dtDown.Year, dtDown.Month, dtDown.Day);
            var dateUp = String.Format("{0}-{1:00}-{2:00} 00:00:00", dtUp.Year, dtUp.Month, dtUp.Day);

            var dt = G.db_select("GetBalanceCategoriesReport '{1}','{2}', {3}", dateDown, dateUp, _userID);
            CheckDB(G.LastError);

            BalanceGridControl.ItemsSource = ((IListSource)dt).GetList();
            try
            {
                BalanceGridControl.Columns["Name"].AllowFocus = false;
                BalanceGridControl.Columns["Credit"].AllowFocus = false;
                BalanceGridControl.Columns["Debet"].AllowFocus = false;
                BalanceGridControl.Columns["Rest"].AllowFocus = false;

                BalanceGridControl.Columns["Number"].Visible = false;
                BalanceGridControl.Columns["IsCategory"].Visible = false;

                BalanceGridControl.Columns["Name"].Header = "Категория/счет";
                BalanceGridControl.Columns["Credit"].Header = "Приход";
                BalanceGridControl.Columns["Debet"].Header = "Расход";
                BalanceGridControl.Columns["Rest"].Header = "Прибыль";

                BalanceGridControl.Columns["Name"].AllowColumnFiltering = DevExpress.Utils.DefaultBoolean.False;
                BalanceGridControl.Columns["Credit"].AllowColumnFiltering = DevExpress.Utils.DefaultBoolean.False;
                BalanceGridControl.Columns["Debet"].AllowColumnFiltering = DevExpress.Utils.DefaultBoolean.False;
                BalanceGridControl.Columns["Rest"].AllowColumnFiltering = DevExpress.Utils.DefaultBoolean.False;

                BalanceGridControl.Columns["Name"].CellStyle = BalanceGridControl.Resources["categoryAccountCellStyle"] as Style;
                BalanceGridControl.Columns["Credit"].CellStyle = BalanceGridControl.Resources["categoryAccountCellStyle"] as Style;
                BalanceGridControl.Columns["Debet"].CellStyle = BalanceGridControl.Resources["categoryAccountCellStyle"] as Style;
                BalanceGridControl.Columns["Rest"].CellStyle = BalanceGridControl.Resources["categoryAccountCellStyle"] as Style;
            }
            catch { }

            BalanceGridControlView.AllowSorting = false;
            BalanceGridControlView.AllowFilterEditor = DevExpress.Utils.DefaultBoolean.False;
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
                return;
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

    /// <summary>
    /// Раскраска заднего фона отчетов
    /// </summary>
    public class BalanceCategoriesBackgroundConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isCategory = (bool)value;

            if (isCategory)
                return Brushes.Gray;

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        { return null; }  //не используется

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }

    /// <summary>
    /// Раскраска текста заголовков отчетов
    /// </summary>
    public class BalanceCategoriesForegroundConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isCategory = (bool)value;

            if (isCategory)
                return Brushes.White;

            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        { return null; }  //не используется

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
