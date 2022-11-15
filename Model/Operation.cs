using am.BL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Budget.Model
{
    public class Operation
    {
        public int ID { get; set; }
        public string SecAccount { get; set; }
        public string Category { get; set; }
        public DateTime OperDay { get; set; }
        public double Debet { get; set; }
        public double Credit { get; set; }
        public double Amount { get; set; }
        public int Credit_ID { get; set; }
        public int Debet_ID { get; set; }
        public int OperDay_ID { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }
        public DateTime DateCreate { get; set; }
        public bool HasImage { get; set; }

        public Operation(DataRow row) : base()
        {
            ID          = G._I(row["ID"]);
            SecAccount  = G._S(row["SecAccount"]);
            Category    = G._S(row["Category"]);
            OperDay     = G._D(row["OperDay"]);
            try { Debet = Convert.ToDouble(row["Debet"]); } catch{}
            try { Credit = Convert.ToDouble(row["Credit"]); } catch{}
            try { Amount = Convert.ToDouble(row["Amount"]); } catch{}
            Credit_ID   = G._I(row["Credit_ID"]);
            Debet_ID    = G._I(row["Debet_ID"]);
            OperDay_ID  = G._I(row["OperDay_ID"]);
            Status      = G._I(row["Status"]);
            Description = G._S(row["Description"]);
            DateCreate  = G._D(row["DateCreate"]);
        }
    }
}
