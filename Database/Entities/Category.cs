using am.BL;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Budget.EF
{
    public class Category
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int CreditRating { get; set; }
        public int DebetRating { get; set; }
        public int? TransferRating { get; set; }
        public int UserID { get; set; }
        public decimal? Limit { get; set; }
        public decimal? Plan { get; set; }
        public int? FirstDay { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }



    public static class Categors
    {
        public static Dictionary<int, int> idsMap = new Dictionary<int, int>();
        public static bool Restore(JToken categors, out string error)
        {
            error = "";
            bool res = true;
            foreach (var cat in categors)
            {
                try
                {
                    Set(cat);
                }
                catch (Exception ex) { error = ex.Message; }
            }
            return res;
        }
        static void Set(JToken cat)
        {
            BudgetDb db = new BudgetDb();

            int cid = (int)cat["ID"];
            Category category = db.Categories.Where(c => c.UserID == U.Cur.ID && c.ID == cid).FirstOrDefault();
            if (category != null)
            {
                copy(category, cat);
            }
            else
            {
                category = new Category();
                copy(category, cat);
                db.Categories.Add(category);
            }
            db.SaveChanges();

            idsMap.Add(cid, category.ID);
        }
        static void copy(Category c, JToken catToken)
        {
            c.Name = (string)catToken["Name"];
            c.UserID = U.Cur.ID;
            c.CreditRating = (int)catToken["CreditRating"];
            c.DebetRating = (int)catToken["DebetRating"];
            c.TransferRating = ToInt(catToken["TransferRating"]);
            c.Limit = ToDecimal(catToken["Limit"]);
            c.Plan = ToDecimal(catToken["Plan"]);
            c.FirstDay = ToInt(catToken["FirstDay"]);
    
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
        static int? ToInt(JToken b)
        {
            int? result = null;

            try
            {
                result = (int)b;
            }
            catch { }

            return result;
        }

    }
}
