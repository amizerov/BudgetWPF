using am.BL;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Budget.EF
{
    public class Operation
    {
        public int ID { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public int? Category_ID { get; set; }
        public int? Debet_ID { get; set; }
        public int? Credit_ID { get; set; }
        public int OperDay_ID { get; set; }
        public DateTime DateCreate { get; set; }
        public DateTime DateEdit { get; set; }
        public string UserName { get; set; }
        public int Status { get; set; }
        public int? Transfer_ID { get; set; }
    }
    public static class Operatis
    {
        public static bool IsNewUser;
        public static Dictionary<int?, int?> accIdsMap;
        public static Dictionary<int, int> catIdsMap;
        public static bool Restore(JToken operations, out string error)
        {
            error = "";
            bool res = true;
            foreach (var oper in operations)
            {
                try
                {
                    Set(oper);
                }
                catch (Exception ex) { error = ex.Message; }
            }
            return res;
        }
        static void Set(JToken oper)
        {
            using (BudgetDb db = new BudgetDb())
            {
                int oid = (int)oper["ID"];
                Operation operation = db.Operations.Where(o => o.ID == oid).FirstOrDefault();

                if (IsNewUser || operation == null)
                {
                    operation = new Operation();
                    copy(operation, oper);
                    db.Operations.Add(operation);
                }
                else
                {
                    copy(operation, oper);
                }
                db.SaveChanges();
            }
        }
        static void copy(Operation o, JToken operToken)
        {
            o.Description   = (string)operToken["Description"];
            o.Amount        = (decimal)operToken["Amount"];
            o.Category_ID   = getCat(operToken["Category_ID"]);
            o.Debet_ID      = getAcc(operToken["Debet_ID"]);
            o.Credit_ID     = getAcc(operToken["Credit_ID"]);
            o.OperDay_ID    = (int)operToken["OperDay_ID"];
            o.DateCreate    = (DateTime)operToken["DateCreate"];
            o.DateEdit      = (DateTime)operToken["DateEdit"];
            o.UserName      = (string)operToken["UserName"];
            o.Status        = (int)operToken["Status"];
            o.Transfer_ID   = (int?)operToken["Transfer_ID"];
        }
        static int? getAcc(JToken aToken) 
        {
            int? acc = (int?)aToken;
            acc = accIdsMap.FirstOrDefault(a => a.Key == acc).Value;

            return acc;
        }
        static int? getCat(JToken cToken)
        {
            int? cat = (int?)cToken;
            cat = catIdsMap.FirstOrDefault(c => c.Key == cat).Value;

            return cat;
        }
    }
}
