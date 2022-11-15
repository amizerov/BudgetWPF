using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Budget.Model
{
    public class ExportOperation
    {
        public int ID { get; set; }
        public string Description { get; set; }
        public double Amount { get; set; }
        public int Category_ID { get; set; }
        public int? Debet_ID { get; set; }
        public int? Credit_ID { get; set; }
        public int OperDay_ID { get; set; }
        public DateTime DateCreate { get; set; }
        public DateTime DateEdit { get; set; }
        public int Status { get; set; }

        public ExportOperation()
        {
            ID = -1;
            Description = String.Empty;
            Amount = 0;
            Category_ID = -1;
            Debet_ID = null;
            Credit_ID = null;
            OperDay_ID = -1;
            DateCreate = default(DateTime);
            DateEdit = default(DateTime);
            Status = 0;
        }

        public ExportOperation(DataRow row)
            : base()
        {
            try { ID = Convert.ToInt32(row["ID"]); } catch { }
            Description = row["Description"].ToString();
            try { Amount = Convert.ToDouble(row["Amount"]); } catch { }
            try { Category_ID = Convert.ToInt32(row["Category_ID"]); } catch {}
            try { Debet_ID = Convert.ToInt32(row["Debet_ID"]); } catch {}
            try { Credit_ID = Convert.ToInt32(row["Credit_ID"]); } catch {}
            try { OperDay_ID = Convert.ToInt32(row["Credit_ID"]); } catch {}
            try { DateCreate = Convert.ToDateTime(row["DateCreate"]); } catch {}
            try { DateEdit = Convert.ToDateTime(row["DateEdit"]); } catch {}
            try { Status = Convert.ToInt32(row["Status"]); } catch {}
        }
    }
}
