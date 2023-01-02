using System;
using System.IO;
using System.Web;
using System.Data;
using System.Configuration;

[Flags]
public enum State : 
int
{    
	Date=1, Comm=2, Prod=4
}

namespace am.BL
{
	/// <summary>
	/// Summary description for G.
	/// </summary>
	public class G
	{
		public static string LastError = "";

		public G()
		{
		}

		public static string GetSqlServer()
		{
			string srv = DB.DBManager.Instance.Server;
			return srv;
		}
		public static string GetDatabase()
		{
			string dbe = DB.DBManager.Instance.Database;
			return dbe;
		}
		public static string GetSqlLogin()
		{
			string uid = DB.DBManager.Instance.Username;
			return uid;
		}
		public static string GetSqlPassword()
		{
			string pwd = DB.DBManager.Instance.Password;
			return pwd;
		}

		public static bool db_exec(string sql, params object[] par)
		{
			LastError = "";
			DB.Connection con = new DB.Connection();
			bool res = con.exec(sql, par);
			LastError = con.LastError;
			return res;
		}

		public static DataTable db_select(string sql, params object[] par)
		{
			LastError = "";
			DB.Connection con = new DB.Connection();
			DataTable res = con.Select(sql, par);
			LastError = con.LastError;
			return res;
		}

		public static DateTime _D(object p)
		{
			return _D(p, DateTime.Now);
		}
	
		public static DateTime _D(object p, DateTime def)
		{
			DateTime r = def;
			if(p != null)
			{
				string s = p.ToString();
				if(s.Length > 0)
				{
					try
					{
						r = DateTime.Parse(s);
					}
					catch{r = def;}
				}
			}
			return r;
		}

		public static int _I(object p)
		{
			return _I(p, 0);
		}

		public static int _I(object p, int def)
		{
			int r = def;
			try
			{
				if(p.GetType().Name == "DataTable") 
				{
					DataTable dt = (DataTable)p;
					if(dt.Rows.Count > 0) r = int.Parse(dt.Rows[0][0]+"");
				}
				else if(p.GetType().Name == "DataRow") 
				{
					DataRow dr = (DataRow)p;
					r = int.Parse(dr[0]+"");
				}
				else
				{
					if(p.ToString().Length > 0)
						r = int.Parse(p.ToString());
				}
			}
			catch{r = def;}

			return r;
		}

		public static int _I(DataTable d, string f)
		{
			int r = 0;
			try
			{
				if(d.Rows.Count > 0) 
				{
					r = int.Parse(d.Rows[0][f]+"");
				}
			} catch{}

			return r;
		}

		public static int _I(DataRow dr, string f)
		{
			int r = 0;
			try
			{
				r = int.Parse(dr[f]+"");
			} 
			catch{}

			return r;
		}

        public static bool _B(object p)
        {
            return _B(p, false);
        }

        public static bool _B(object p, bool def)
        {
            bool r = def;
            try
            {
                if (p.GetType().Name == "DataTable")
                {
                    DataTable dt = (DataTable)p;
                    if (dt.Rows.Count > 0)
                    {
                        var s = dt.Rows[0][0] + "";
                        r = (s == bool.TrueString || s == "1") ? true : false;
                    }
                }
                else if (p.GetType().Name == "DataRow")
                {
                    DataRow dr = (DataRow)p;

                    var s = dr[0] + "";
                    r = (s == bool.TrueString || s == "1") ? true : false;
                }
                else
                {
                    var s = p.ToString();
                    if (!String.IsNullOrEmpty(s))
                        r = (s == bool.TrueString || s == "1") ? true : false;
                }
            }
            catch { r = def; }

            return r;
        }

        public static bool _B(DataTable d, string f)
        {
            bool r = false;
            try
            {
                if (d.Rows.Count > 0)
                {
                    var s = d.Rows[0][f] + "";
                    r = (s == bool.TrueString || s == "1") ? true : false;
                }
            }
            catch { }

            return r;
        }

        public static bool _B(DataRow dr, string f)
        {
            bool r = false;
            try
            {
                var s = dr[f] + "";
                r = (s == bool.TrueString || s == "1") ? true : false;
            }
            catch { }

            return r;
        }

		public static string _S(object p)
		{
			return _S(p, "");
		}

		public static string _S(object p, string def)
		{
			string r = def;
			try
			{
				if(p.GetType().Name == "DataTable") 
				{
					DataTable dt = (DataTable)p;
					if (dt.Rows.Count > 0)
					{
						if (def.Length > 0)
							r = dt.Rows[0][def] + "";
						else
							r = dt.Rows[0][0] + "";
					}
					else
						r = "";
				}
				else if(p.GetType().Name == "DataRow") 
				{
					DataRow dr = (DataRow)p;
					if(def.Length > 0)
						r = dr[def]+"";
					else
						r = dr[0]+"";
				}
				else
				{
					r = p.ToString();
					if(r == "") r = def;
				}
			}
			catch{r = def;}

			return r;
		}

		public static bool CheckDB()
		{
			DB.Connection con = new DB.Connection();
			try
			{
				con.GetConnection();
			}
			catch(Exception exc)
			{ 
				LastError = con.LastError+" ["+exc.Message+"]";
				return false;
			}
			LastError = con.LastError;
			return LastError.Length == 0;
		}

		public static void WriteLog(string src, string txt)
		{
			txt = "<br>\r\n--["+src+"]-------------<br>\r\n"+txt;
			WriteLog(txt);
		}
			
		public static void WriteLog(string txt)
		{
			try
			{
				string sp = "D:\\Work\\Release\\BudgetService\\bin";

                string sPath = sp; // GetCurDir();
				sPath += "\\___log";

				if(!Directory.Exists(sPath))
				{
					Directory.CreateDirectory(sPath);
				}
				DateTime dd = DateTime.Now;
				sPath += "\\log"+dd.Year+"_"+dd.Month+"_"+dd.Day+".html";

				if(!File.Exists(sPath))
				{
					File.Create(sPath).Close();
				}
				StreamWriter sw = new StreamWriter(sPath, true);
				sw.Write(txt);

				sw.WriteLine("<br>\r\n--["+DateTime.Now+"]--------------<br>\r\n");
				sw.Close();
			}
			catch(Exception ex){
				string sErr = ex.Message;
			}
		}

		public static string GetCurDir()
		{
			string cur_dir = System.Reflection.Assembly.GetCallingAssembly().Location;
			int e = cur_dir.LastIndexOf("\\");
			cur_dir = cur_dir.Remove(e, cur_dir.Length-e);
			cur_dir = cur_dir.ToLower().Replace("\\bin\\debug", "");

			return cur_dir;
		}

		public static string GetCurDir2()
		{
			string cur_dir = System.Reflection.Assembly.GetCallingAssembly().Location;
			int e = cur_dir.LastIndexOf("\\");
			cur_dir = cur_dir.Remove(e, cur_dir.Length-e);
			//cur_dir = cur_dir.ToLower().Replace("\\bin\\debug", "");

			return cur_dir+"\\";
		}

	}
}
