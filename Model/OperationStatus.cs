using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Budget.Model
{
    public class OperationStatus
    {
        public int ID { get; set; }
        public string Name { get; set; }

        public OperationStatus()
        {
            ID = -1;
            Name = String.Empty;
        }

        public OperationStatus(DataRow row) : base()
        {
            try { ID = Convert.ToInt32(row["ID"]); } catch { }
            Name = row["Name"].ToString();
        }
    }
}