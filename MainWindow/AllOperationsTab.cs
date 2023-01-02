using am.BL;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Editors;
using System;
using System.Windows.Input;
using System.Windows;
using DevExpress.Xpf.Grid;

namespace Budget
{
    public partial class MainWindow : DXWindow
    {
        #region Все операции tab
        private void DXTabItem_GotFocus(object sender, RoutedEventArgs e)
        {
            if(((DXTabItem)sender).TabIndex == 1)
                btnRefreshOperations_Click(sender, e);
        }

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
            gridControlAllOperations.Columns["Description"].Width = 150;
            gridControlAllOperations.Columns["Status"].Width = 50;
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

    }
}
