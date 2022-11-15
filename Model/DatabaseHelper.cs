using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using am.BL;

namespace Budget.Model
{
  internal class DatabaseHelper
  {
    internal static List<Category> GetCategories(int userId, int creditRating, int transferRating, int accountId, int? gridWidth)
    {
      List<Category> ret = new List<Category>();
      DataTable dataTable;
      if (accountId != -1)
        dataTable = G.db_select("exec GetCategories {1}, {2}, {3}, {4}", userId, creditRating, transferRating, accountId);
      else
        dataTable = G.db_select("exec GetAllCategories {1}", userId);

      CheckDB(G.LastError);

            if (dataTable != null && dataTable.Rows.Count > 0)
            {
                foreach (DataRow r in dataTable.Rows)
                {
                    int id = G._I(r["ID"]);
                    string name = G._S(r["Name"]);
                    string cred = G._S(r["Credit"]);
                    string debe = G._S(r["Debet"]);

                    var category = new Category(id, name, cred, debe, gridWidth != null ? (int)gridWidth : 0);
                    ret.Add(category);
                }
            }
            return ret;
    }

    internal static List<PlanningAccount> GetAccounts(int userId, int? gridWidth)
    {
        List<PlanningAccount> ret = new List<PlanningAccount>();
        var dt = G.db_select("exec GetBudgetAccounts {1}", userId);

        CheckDB(G.LastError);

        if (dt != null && dt.Rows.Count > 0)
        {
            foreach (DataRow row in dt.Rows)
            {
                var account = new PlanningAccount((int)row[0], ReadRowValue(row[1]), ReadRowValue(row[2]), ReadRowValue(row[3]),
                                            gridWidth != null ? (int)gridWidth : 0);
                ret.Add(account);
            }
        }
        return ret;
    }

    private static string ReadRowValue(object rowValue)
    {
        if (rowValue is DBNull)
            return null;
        return rowValue.ToString();
    }


      private static void CheckDB(string error)
    {
        if (error.Length > 0)
        {
            MessageBox.Show(error,
                            "Ошибка в базе данных",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
        }
    }
  }
}
