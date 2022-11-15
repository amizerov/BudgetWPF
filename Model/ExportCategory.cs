using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Budget.Model
{
    public class ExportCategory
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int CreditRating { get; set; }
        public int DebetRating { get; set; }
        public int TransferRating { get; set; }
        public int UserID { get; set; }
        public double Limit { get; set; }
        public double Plan { get; set; }
        public int FirstDay { get; set; }

        public bool IsChecked { get; set; }

        public ExportCategory()
        {
            ID = -1;
            Name = String.Empty;
            CreditRating = 0;
            DebetRating = 0;
            TransferRating = 0;
            UserID = 0;
            Limit = 0;
            Plan = 0;
            FirstDay = 0;

            IsChecked = false;
        }

        public ExportCategory(DataRow row)
            : base()
        {
            try { ID = Convert.ToInt32(row["ID"]); } catch { }
            Name = row["Name"].ToString();
            try { CreditRating = Convert.ToInt32(row["CreditRating"]); } catch { }
            try { DebetRating = Convert.ToInt32(row["DebetRating"]); } catch { }
            try { TransferRating = Convert.ToInt32(row["TransferRating"]); } catch { }
            try { UserID = Convert.ToInt32(row["UserID"]); } catch { }
            try { Limit = Convert.ToDouble(row["Limit"]); } catch { }
            try { Plan = Convert.ToDouble(row["Plan"]); } catch { }
            try { FirstDay = Convert.ToInt32(row["FirstDay"]); } catch { }
        }
    }
}
