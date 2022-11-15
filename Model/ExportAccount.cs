using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Budget.Model
{
    public class ExportAccount
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public double Rest { get; set; }
        public bool IsDeleted { get; set; }
        public int UserID { get; set; }
        public int Order { get; set; }
        public double Limit { get; set; }
        public double Plan { get; set; }
        public int FirstDay { get; set; }

        public bool IsChecked { get; set; }

        public ExportAccount()
        {
            ID = -1;
            Name = String.Empty;
            Rest = 0;
            IsDeleted = false;
            UserID = 0;
            Order = 0;
            Limit = 0;
            Plan = 0;
            FirstDay = 0;

            IsChecked = false;
        }

        public ExportAccount(DataRow row) : base()
        {
            try { ID = Convert.ToInt32(row["ID"]); } catch { }
            try { Order = Convert.ToInt32(row["Order"]); } catch { }
            Name = row["Name"].ToString();
            try { Rest = Convert.ToDouble(row["Rest"]); } catch {}
            try { IsDeleted = Convert.ToBoolean(row["IsDeleted"]); } catch {}
            try { UserID = Convert.ToInt32(row["UserID"]); } catch {}
            try { Limit = Convert.ToDouble(row["Limit"]); } catch {}
            try { Plan = Convert.ToDouble(row["Plan"]); } catch {}
            try { FirstDay = Convert.ToInt32(row["FirstDay"]); } catch {}
        }
    }
}
