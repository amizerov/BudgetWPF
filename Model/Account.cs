using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Budget.Model
{
    public class Account
    {
        public int ID { get; set; }
        public int Order { get; set; }
        public string Name { get; set; }
        public double Rest1 { get; set; }
        public int OperDay_ID1 { get; set; }
        public double Rest2 { get; set; }
        public int OperDay_ID2 { get; set; }
        public double cred { get; set; }
        public double deb { get; set; }

        public Account()
        {
            ID = 0;
            Order = 0;
            Name = String.Empty;
            Rest1 = 0;
            OperDay_ID1 = 0;
            Rest2 = 0;
            OperDay_ID2 = 0;
            cred = 0;
            deb = 0;
        }

        public Account(DataRow row) : base()
        {
            try { ID = Convert.ToInt32(row["ID"]); } catch { }
            try { Order = Convert.ToInt32(row["Order"]); } catch { }
            Name = row["Name"].ToString();
            try { Rest1 = Convert.ToDouble(row["Rest1"]); } catch {}
            try { OperDay_ID1 = Convert.ToInt32(row["OperDay_ID1"]); } catch {}
            try { OperDay_ID2 = Convert.ToInt32(row["OperDay_ID2"]); } catch {}

            var q1 = row["Rest2"].ToString();
            var q2 = q1;

            try { Rest2 = Convert.ToDouble(row["Rest2"]); } catch {}
            try { cred = Convert.ToDouble(row["Cred"]); } catch {}
            try { deb = Convert.ToDouble(row["Deb"]); } catch {}
        }
    }
}
