using am.BL;
using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Budget.EF
{
    public class Account
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public decimal Rest { get; set; }
        public bool IsDeleted { get; set; }
        public int UserID { get; set; }
        public int? Order { get; set; }
        public decimal? Limit { get; set; }
        public decimal? Plan { get; set; }
        public int? FirstDay { get; set; }
        public bool AssistNeeded { get; set; }
        public bool? IsMinusAllowed { get; set; }
        public DateTime dtc { get; set; }
    }

    public static class Accounts
    {
        public static Dictionary<int?, int?> idsMap = new Dictionary<int?, int?>();
        public static bool Restore(JToken accounts, out string error)
        {
            error = "";
            bool res = true;
            foreach (var acc in accounts)
            {
                try
                {
                    Set(acc);
                }
                catch (Exception ex) { error = ex.Message; }
            }
            return res;
        }
        static void Set(JToken acc)
        {
            BudgetDb db = new BudgetDb();

            int aid = (int)acc["ID"];
            Account account = db.Accounts.Where(a => a.UserID == U.Cur.ID && a.ID == aid).FirstOrDefault();
            if (account != null)
            {
                copy(account, acc);
            }
            else
            {
                account = new Account();
                copy(account, acc);
                db.Accounts.Add(account);
            }
            db.SaveChanges();

            idsMap.Add(aid, account.ID);
        }
        static void copy(Account a, JToken accToken)
        {
            a.Name = (string)accToken["Name"];
            a.Rest = (decimal)accToken["Rest"];
            a.IsDeleted = (bool)accToken["IsDeleted"];
            a.UserID = U.Cur.ID;
            a.Order = (int)accToken["Order"];
            a.Limit = ToDecimal(accToken["Limit"]);
            a.Plan = ToDecimal(accToken["Plan"]);
            a.FirstDay = (int)accToken["FirstDay"];
            a.AssistNeeded = (bool)accToken["AssistNeeded"];
            a.IsMinusAllowed = ToBool(accToken["IsMinusAllowed"]);
            a.dtc = (DateTime)accToken["dtc"];

        }
        static decimal? ToDecimal(JToken d)
        {
            decimal? result = null;

            try
            {
                result = (decimal)d;
            }
            catch { }

            return result;
        }
        static bool? ToBool(JToken b)
        {
            bool? result = null;

            try
            {
                result = (bool)b;
            }
            catch { }

            return result;
        }
        public static bool RecalcRests(out string error)
        {
            error = "";
            foreach(var a in idsMap)
            {
                G.db_exec("am_CloseAccOperDay " + a.Value);
                error = G.LastError;
                if(error.Length > 0) 
                    return false;
            }
            return true;
        }
    }
}
