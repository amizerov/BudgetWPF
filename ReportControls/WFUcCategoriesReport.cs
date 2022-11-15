using am.BL;
using DevExpress.Data;
using DevExpress.Data.ODataLinq.Helpers;
using DevExpress.LookAndFeel;
using DevExpress.XtraGrid;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace Budget.ReportControls
{
    public partial class WFUcCategoriesReport : UserControl
    {
        public WFUcCategoriesReport()
        {
            InitializeComponent();

            CommandBroker.OnCommand += doCommand;
            ChangeStyle();
        }

        private void WFUcCategoriesReport_Load(object sender, EventArgs e)
        {
            DateTime fromDate, toDate;
            Utils.RestoreReportCategoriesDates(out fromDate, out toDate);
            deFrom.EditValue = fromDate;
            deTo.EditValue = toDate;

            using (var db = new BudgetDb())
            {
                List<EF.Category> cats = db.Categories.Where(c => c.UserID == U.Cur.ID).ToList();
                cbCategory.Properties.Items.AddRange(cats);
                cbCategory.SelectedIndex = 0;
            }
            FillReport();
        }

        void doCommand(int cmd)
        {
            if (cmd == 1) ChangeStyle();
        }

        public void ChangeStyle()
        {
            int themeIdx = Properties.Settings.Default.ThemeIndex;
            if (themeIdx > 0 && themeIdx <= Consts.DevExTheme.Count)
            {
                UserLookAndFeel.Default.SetSkinStyle(Consts.WinFormsSkin[themeIdx]);
            }
        }
        private void btnReload_Click(object sender, EventArgs e)
        {
            Utils.SaveReportCategoriesDates(deFrom.DateTime, deTo.DateTime);
            FillReport();
        }

        private void FillReport()
        {
            var from = deFrom.DateTime.ToString("yyyyMMdd");
            var to = deTo.DateTime.ToString("yyyyMMdd");
            var cid = ((EF.Category)cbCategory.SelectedItem).ID;

            var dt = G.db_select("report_GetCategoryOperations '{1}','{2}', {3}", from, to, cid);
            CheckDB(G.LastError);

            gv.Columns.Clear();
            gc.DataSource = dt;

            var colRash = gv.Columns["Расход"];
            var colPrih = gv.Columns["Приход"];
            colRash.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Custom;
            colRash.DisplayFormat.FormatString = "c";
            colPrih.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Custom;
            colPrih.DisplayFormat.FormatString = "c";

            GridColumnSummaryItem siRash = new GridColumnSummaryItem();
            siRash.SummaryType = SummaryItemType.Sum;
            siRash.DisplayFormat = "{0:c}";
            GridColumnSummaryItem siPrih = new GridColumnSummaryItem();
            siPrih.SummaryType = SummaryItemType.Sum;
            siPrih.DisplayFormat = "{0:c}";
            colRash.Summary.Add(siRash);
            colPrih.Summary.Add(siPrih);

            gv.BestFitColumns();
        }

        private void CheckDB(string LE)
        {
            if (LE.Length > 0)
            {
                MessageBox.Show(LE, "Ошибка в базе данных");
                return;
            }
        }
    }
}
