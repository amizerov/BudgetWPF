using am.BL;
using DevExpress.LookAndFeel;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Editors.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Forms;

namespace Budget.ReportControls
{
    public partial class WFUcRestChart : UserControl
    {
        public WFUcRestChart()
        {
            InitializeComponent();

            CommandBroker.OnCommand += doCommand;
            ChangeStyle();
        }

        private void WFUcRestChart_Load(object sender, EventArgs e)
        {

            deFrom.DateTime = DateTime.Now.AddDays(-10);
            deTo.DateTime = DateTime.Now;

            btnReload_Click(sender, e);
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
            DateTime dFrom = deFrom.DateTime, dTo = deTo.DateTime;
            string from = dFrom.ToString("yyyyMMdd"), to = dTo.ToString("yyyyMMdd");

            chart.Series["Rest"].ArgumentDataMember = "Date";
            chart.Series["Rest"].ValueDataMembers.AddRange("Rest");
            //chart.Series["Rest"].DataSource = DataPoint.GetRestPoints(from, to);
            chart.Series["Rest"].DataSource = DataPoint.GetRestPoints2(dFrom, dTo);

            chart.Series["Pers"].ArgumentDataMember = "Date";
            chart.Series["Pers"].ValueDataMembers.AddRange("Rest");
            chart.Series["Pers"].DataSource = DataPoint.GetPersPoints(from, to);

            chart.Series["Busn"].ArgumentDataMember = "Date";
            chart.Series["Busn"].ValueDataMembers.AddRange("Rest");
            chart.Series["Busn"].DataSource = DataPoint.GetBusnPoints(from, to);
        }
    }
    public class DataPoint
    {
        public DateTime Date { get; set; }
        public double Rest { get; set; }
        public DataPoint(DateTime date, double value)
        {
            this.Date = date;
            this.Rest = value;
        }
        public static List<DataPoint> GetRestPoints(string from, string to)
        {
            DataTable dt = G.db_select("report_DailyUserRests {1}, '{2}', '{3}'", U.Cur.ID, from, to);
            List<DataPoint> data = new List<DataPoint>();
            foreach (DataRow r in dt.Rows)
            {
                string sDate = r["Date"].ToString();
                DateTime dDate = G._D(sDate);
                string sRest = r["Rest"].ToString();
                double dRest = sRest.TryConvertToDouble();
                data.Add(new DataPoint(dDate, dRest));
            }
            return data;
        }
        public static List<DataPoint> GetRestPoints2(DateTime from, DateTime to)
        {
            DataTable dt = db.select("report_DailyUserRests @UserID, @FromDate, @ToDate", U.Cur.ID, from, to);
            List<DataPoint> data = new List<DataPoint>();
            foreach (DataRow r in dt.Rows)
            {
                string sDate = r["Date"].ToString();
                DateTime dDate = G._D(sDate);
                string sRest = r["Rest"].ToString();
                double dRest = sRest.TryConvertToDouble();
                data.Add(new DataPoint(dDate, dRest));
            }
            return data;
        }
        public static List<DataPoint> GetPersPoints(string from, string to)
        {
            DataTable dt = G.db_select("report_DailyPersonalExpenses {1}, '{2}', '{3}'", U.Cur.ID, from, to);
            List<DataPoint> data = new List<DataPoint>();
            foreach (DataRow r in dt.Rows)
            {
                string sDate = r["Date"].ToString();
                DateTime dDate = G._D(sDate);
                string sRest = r["Rest"].ToString();
                double dRest = sRest.TryConvertToDouble();
                data.Add(new DataPoint(dDate, dRest));
            }
            return data;
        }
        public static List<DataPoint> GetBusnPoints(string from, string to)
        {
            DataTable dt = G.db_select("report_DailyBusinessExpenses {1}, '{2}', '{3}'", U.Cur.ID, from, to);
            List<DataPoint> data = new List<DataPoint>();
            foreach (DataRow r in dt.Rows)
            {
                string sDate = r["Date"].ToString();
                DateTime dDate = G._D(sDate);
                string sRest = r["Rest"].ToString();
                double dRest = sRest.TryConvertToDouble();
                data.Add(new DataPoint(dDate, dRest));
            }
            return data;
        }
    }
}

public class CommandBroker
{
    public static event Action<int> OnCommand;
    public static void SendCommand(int cmd) => OnCommand?.Invoke(cmd);
}