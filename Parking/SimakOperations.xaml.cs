using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Data;
using DevExpress.Xpf.Core;
using am.BL;

namespace Budget.Parking
{
    /// <summary>
    /// Interaction logic for SimakOperations.xaml
    /// </summary>
    public partial class SimakOperations : DXWindow
    {
        private string _defaultConnectionString;

        public SimakOperations()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FillOperations();
        }

        private void gcvSimakOperations_RowDoubleClick(object sender, DevExpress.Xpf.Grid.RowDoubleClickEventArgs e)
        {
            if (gcSimakOperations.SelectedItems.Count == 0) return;

            AddOperation();
        }

        private void gcvSimakOperations_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F8)
            {
                AddOperarionsSilent();
            }
            else if (e.Key == Key.F5)
            {
                FillOperations();
            }
            else if (e.Key == Key.Delete)
            {
                ClearOperation();
            }
        }

        private void ConnectToParking()
        {
            _defaultConnectionString = am.DB.DBManager.Instance.ConnectionString;
            am.DB.DBManager.Instance.Init("parking.svr.vc", "cs", "utest", "Gh0l@kbIr@a4br");
        }

        private void ConnectToBudget()
        {
            am.DB.DBManager.Instance.Init(_defaultConnectionString, 30, 90);
        }

        private void FillOperations()
        {
            ConnectToParking();
            var dt = G.db_select("am_GetSimakOperations");
            ConnectToBudget();

            gcSimakOperations.ItemsSource = ((IListSource)dt).GetList();
            try
            {
                foreach (var c in gcSimakOperations.Columns)
                    c.AllowFocus = false;
            }
            catch (Exception ex)
            {
                string sex = ex.Message;
            }
        }

        private void AddOperation()
        {
            var w = new AddOperationWindow(34, -1, Margin.Top, Margin.Left, 91, Consts.OperationType.Credit, null);
            w.Owner = this;

            DataRow r = ((DataRowView)gcSimakOperations.SelectedItems[0]).Row;


            w.AmountTextBox.Text = r["Amount"] + "";
            w.DescriptionTextBox.Text = r["Descr"] + "";
            w.OperDayDatePicker.DateTime = G._D(r["Date"]);
            w.listOperationType.SelectedIndex = G._I(r["IsDebit"]);

            if (w.ShowDialog() == true)
            {
                ConnectToParking();
                var dt = G.db_exec("update Transfer set Transfer_Description = 'InBudget' where Transfer_Id = " + r["ID"]);
                ConnectToBudget();

                FillOperations();
            }
        }

        private void AddOperarionsSilent()
        {
            int cnt = 0;

            foreach (DataRowView rv in gcSimakOperations.SelectedItems)
            {
                DataRow r = rv.Row;

                int type = G._I(r["IsDebit"]) + 1;
                int uid = 34;
                string amnt = G._S(r["Amount"]).Replace(',', '.');
                string acc1, acc2;
                string descr = G._S(r["Descr"]);
                int categ = -1;
                string operDay = G._D(r["Date"]).ToString("yyyy-MM-dd");

                if (type == 1)
                {  //зачисление
                    acc1 = "NULL";
                    acc2 = "91";
                    categ = G._I(G.db_select("SELECT top 1 ID FROM Categories where userid=34 order by CreditRating desc"));
                }
                else
                {  //списание
                    acc1 = "91";
                    acc2 = "NULL";
                    categ = G._I(G.db_select("SELECT top 1 ID FROM Categories where userid=34 order by DebetRating desc"));
                }

                int res = G._I(G.db_select("exec SetOperation {1}, {2}, {3}, {4}, {5}, {6}, '{7}', {8}, '{9}'", uid, type, amnt, "NULL", acc1, acc2, descr, categ, operDay));

                if (res > 0)
                {
                    cnt++;

                    ConnectToParking();
                    var dt = G.db_exec("update Transfer set Transfer_Description = 'InBudget' where Transfer_Id = " + r["ID"]);
                    ConnectToBudget();
                }
                else if (res == -3)  //уход в минус
                {
                    MessageBox.Show("Невозможно добавить операцию, так как введенная вами сумма переводит остаток по счету в некоторые ОперДни в \"минус\"!",
                                    "Ошибка",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
                else if (res <= 0)  //остальные ошибки
                {
                    MessageBox.Show("Возникла ошибка при обращении к базе данных. Повторите попытку позднее.",
                                    "Ошибка",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
            }

            if (cnt > 0) FillOperations();

        }

        private void ClearOperation()
        {
            DataRow r = ((DataRowView)gcSimakOperations.SelectedItems[0]).Row;
            ConnectToParking();
            var dt = G.db_exec("update Transfer set Transfer_Description = 'InBudget' where Transfer_Id = " + r["ID"]);
            ConnectToBudget();

            FillOperations();
        }
    }
}
