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
using System.Windows.Markup;
using System.Globalization;
using am.BL;
using System.Data;
using System.Collections;
using System.ComponentModel;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Core;

namespace Budget.Operations
{
    public partial class OperationsControl : UserControl
    {
        private int _userID;

        public OperationsControl(int userID)
        {
            InitializeComponent();

            _userID = userID;

            LoadMinAndMaxOperDay();
            FillOperations();

            if (Properties.Settings.Default.ThemeIndex > 0 && Properties.Settings.Default.ThemeIndex <= Consts.DevExTheme.Count)
                ThemeManager.SetTheme(this, Consts.DevExTheme[Properties.Settings.Default.ThemeIndex]);
        }

        /// <summary>
        /// Загрузить мин и макс ОперДень
        /// </summary>
        private void LoadMinAndMaxOperDay()
        {
            var downDate = default(DateTime);
            DateTime.TryParse(G._S(G.db_select("exec GetMinOperDay {1}", _userID)), out downDate);
            CheckDB(G.LastError);
            var upDate = default(DateTime);
            DateTime.TryParse(G._S(G.db_select("exec GetMaxOperDay {1}", _userID)), out upDate);
            CheckDB(G.LastError);
            if (upDate != default(DateTime) && downDate != default(DateTime))
            {
                OperationsDownDatePicker.EditValue = downDate;
                OperationsUpDatePicker.EditValue = upDate;
            }
            else
            {
                OperationsDownDatePicker.EditValue = DateTime.Now;
                OperationsUpDatePicker.EditValue = DateTime.Now;
            }
        }

        #region Словарь статусов операций
        public ComboBoxEditSettings Statuses
        {
            get
            {
                var editSettingsItem = new ComboBoxEditSettings
                {
                    ItemsSource = ((IListSource)am.BL.G.db_select("GetOperationStatuses")).GetList(),
                    DisplayMember = "Name",
                    ValueMember = "ID",
                    NullText = "",
                    AllowNullInput = false
                };

                return editSettingsItem;
            }
        }
        #endregion  Словарь статусов операций

        private void FillOperations()
        {
            OperationsGridControl.ItemsSource = null;

            var dtDown = (DateTime)OperationsDownDatePicker.EditValue;
            var dtUp = (DateTime)OperationsUpDatePicker.EditValue;
            var dateDown = String.Format("{0}-{1:00}-{2:00} 00:00:00", dtDown.Year, dtDown.Month, dtDown.Day);
            var dateUp = String.Format("{0}-{1:00}-{2:00} 00:00:00", dtUp.Year, dtUp.Month, dtUp.Day);

            DataTable dt = G.db_select("GetOperationTableByUserID '{1}', '{2}', {3}", dateDown, dateUp, _userID);
            CheckDB(G.LastError);
            OperationsGridControl.ItemsSource = ((IListSource)dt).GetList();

            OperationsGridControl.Columns["ID"].Visible = false;
            OperationsGridControl.Columns["FirstAccount"].AllowFocus = false;
            OperationsGridControl.Columns["SecAccount"].AllowFocus = false;
            OperationsGridControl.Columns["Status"].AllowFocus = false;
            OperationsGridControl.Columns["Debet"].AllowFocus = false;
            OperationsGridControl.Columns["Credit"].AllowFocus = false;
            OperationsGridControl.Columns["OperDay_ID"].Visible = false;
            OperationsGridControl.Columns["Description"].AllowFocus = false;
            OperationsGridControl.Columns["Amount"].AllowFocus = false;
            OperationsGridControl.Columns["Amount"].Visible = false;
            OperationsGridControl.Columns["Category"].AllowFocus = false;
            OperationsGridControl.Columns["OperDay"].AllowFocus = false;
            OperationsGridControl.Columns["DateCreate"].AllowFocus = false;

            OperationsGridControl.Columns["Status"].EditSettings = Statuses;

            OperationsGridControl.Columns["FirstAccount"].Header = "1-ый счет";
            OperationsGridControl.Columns["SecAccount"].Header = "2-ой счет";
            OperationsGridControl.Columns["Description"].Header = "Описание";
            OperationsGridControl.Columns["Amount"].Header = "Сумма";
            OperationsGridControl.Columns["Category"].Header = "Категория";
            OperationsGridControl.Columns["OperDay"].Header = "ОперДень";
            OperationsGridControl.Columns["DateCreate"].Header = "Дата";
            OperationsGridControl.Columns["Debet"].Header = "Списание";
            OperationsGridControl.Columns["Credit"].Header = "Зачисление";
            OperationsGridControl.Columns["Status"].Header = "Статус";

            OperationsGridControl.Columns["Debet"].CellTemplate = OperationsGridControl.Resources["debetCellTemplate"] as DataTemplate;
            OperationsGridControl.Columns["Credit"].CellTemplate = OperationsGridControl.Resources["creditCellTemplate"] as DataTemplate;

            OperationsGridControl.Columns["Status"].CellStyle = OperationsGridControl.Resources["statusCellStyle"] as Style;
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

        private void btnOperationsRefresh_Click(object sender, RoutedEventArgs e)
        {
            FillOperations();
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
