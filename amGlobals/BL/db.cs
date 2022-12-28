using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using am.DB;

namespace am.BL
{
    public class db
    {
        private static string _lastError = "";
        private static Connection _con = new Connection();

        public static string LastError
        {
            set
            {
                _lastError = value;
                if (!String.IsNullOrEmpty(value))
                    _con.WriteLog(0, value);
            }
            get
            {
                return _lastError;
            }
        }

        /// <summary>
        /// Выполнить хранимую процедуру и вернуть DataTable в качестве результата
        /// </summary>
        /// <param name="stProcName">Имя хранимой процедуры</param>
        /// <param name="par">Параметры хранимой процедуры</param>
        public static DataTable select(string sql, params object[] par)
        {
            LastError = "";
            DataTable dt = null;

            var pars = GetParList(GetParNames(sql), par);

            if (sql.ToLower().IndexOf("exec") == 0)
            {  //процедура
                if (sql.ToLower().IndexOf("execute") == 0)
                    sql = sql.Substring("execute".Length, sql.Length - "execute".Length).Trim();
                else
                    sql = sql.Substring("exec".Length, sql.Length - "exec".Length).Trim();

                var procName = GetProcName(sql);
                if (pars == null)
                    LastError = String.Format("\"{0}\". Number of declared parameters names don't equal to number of parameters values.", sql);
                else
                    dt = _con.SelectStoredProc(procName, pars);
            }
            else
            {  //запрос
                dt = _con.SelectText(sql, pars);
            }

            LastError = _con.LastError;
            return dt;
        }

        /// <summary>
        /// Выполнить хранимую процедуру без возвращаемого результата
        /// </summary>
        /// <param name="stProcName">Имя хранимой процедуры</param>
        /// <param name="par">Параметры хранимой процедуры</param>
        public static void exec(string sql, params object[] par)
        {
            LastError = "";

            var pars = GetParList(GetParNames(sql), par);
            if (sql.ToLower().IndexOf("exec") == 0)
            {  //процедура
                if (sql.ToLower().IndexOf("execute") == 0)
                    sql = sql.Substring("execute".Length, sql.Length - "execute".Length).Trim();
                else
                    sql = sql.Substring("exec".Length, sql.Length - "exec".Length).Trim();

                var procName = GetProcName(sql);
                if (pars == null)
                    LastError = String.Format("\"{0}\". Number of declared parameters names don't equal to number of parameters values.", sql);
                else
                    _con.ExecStoredProc(procName, pars);
            }
            else
            {  //запрос
                _con.ExecText(sql, pars);
            }

            LastError = _con.LastError;
        }

        /// <summary>
        /// Выполнить хранимую процедуру c единственным возвращаемым значением
        /// </summary>
        /// <param name="stProcName">Имя хранимой процедуры</param>
        /// <param name="par">Параметры хранимой процедуры</param>
        /// <summary>
        public static object select_scalar(string sql, params object[] par)
        {
            LastError = "";
            object res = null;

            var pars = GetParList(GetParNames(sql), par);
            if (sql.ToLower().IndexOf("exec") == 0)
            {  //процедура
                if (sql.ToLower().IndexOf("execute") == 0)
                    sql = sql.Substring("execute".Length, sql.Length - "execute".Length).Trim();
                else
                    sql = sql.Substring("exec".Length, sql.Length - "exec".Length).Trim();

                var procName = GetProcName(sql);
                if (pars == null)
                    LastError = String.Format("\"{0}\". Number of declared parameters names don't equal to number of parameters values.", sql);
                else
                    res = _con.ExecScalarStoredProc(procName, pars);
            }
            else
            {  //запрос
                res = _con.ExecScalarText(sql, pars);
            }

            LastError = _con.LastError;
            return res;
        }

        private static string GetProcName(string sqlProc)
        {
            var procName = String.Empty;

            var indA = sqlProc.IndexOf('@');
            if ((indA) == -1)
                procName = sqlProc.Trim();
            else
                procName = sqlProc.Substring(0, sqlProc.IndexOf('@')).Trim();

            return procName;
        }

        private static List<string> GetParNames(string sql)
        {
            List<string> list = null;

            var pars = sql.Split('@');
            if (pars.Length > 0)
            {
                list = new List<string>(pars.Length+1);

                for (int i = 1; i < pars.Length; i++)
                {
                    var par = GetParNameFromString(pars[i]);
                    if (!list.Contains(par))
                        list.Add(par);
                }
            }

            return list;
        }

        private static string GetParNameFromString(string parString)
        {
            var par = parString.Trim();
            while (!IsParCorrect(par))
                CorrectPar(ref par);
            if (par.Contains(" "))
                par = par.Substring(0, par.IndexOf(" "));

            return par;
        }

        private static bool IsParCorrect(string par)
        {
            foreach(var ch in par)
            {
                if (!Char.IsLetterOrDigit(ch) && ch != ' ')
                    return false;
            }

            return true;
        }

        private static void CorrectPar(ref string par)
        {
            foreach (var ch in par)
            {
                if (!Char.IsLetterOrDigit(ch) && ch != ' ')
                {
                    par = par.Replace(ch.ToString(), " ");
                    break;
                }
            }
        }

        private static List<SqlParameter> GetParList(List<string> parNames, object[] pars)
        {
            if (parNames.Count != pars.Length)
                return null;

            var list = new List<SqlParameter>();

            for(int i = 0; i<parNames.Count; i++)
            {
                if (pars[i] is bool)
                    list.Add(new SqlParameter() { ParameterName = parNames[i], SqlDbType = SqlDbType.Bit, Direction = ParameterDirection.Input, Value = pars[i] } );
                else if (pars[i] is decimal)
                    list.Add(new SqlParameter() { ParameterName = parNames[i], SqlDbType = SqlDbType.Decimal, Direction = ParameterDirection.Input, Value = pars[i] });
                else if (pars[i] is double)
                    list.Add(new SqlParameter() { ParameterName = parNames[i], SqlDbType = SqlDbType.Float, Direction = ParameterDirection.Input, Value = pars[i] });
                else if (pars[i] is Int16)
                    list.Add(new SqlParameter() { ParameterName = parNames[i], SqlDbType = SqlDbType.SmallInt, Direction = ParameterDirection.Input, Value = pars[i] });
                else if (pars[i] is Int32)
                    list.Add(new SqlParameter() { ParameterName = parNames[i], SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input, Value = pars[i] } );
                else if (pars[i] is Int64)
                    list.Add(new SqlParameter() { ParameterName = parNames[i], SqlDbType = SqlDbType.BigInt, Direction = ParameterDirection.Input, Value = pars[i] });
                else if (pars[i] is double)
                    list.Add(new SqlParameter() { ParameterName = parNames[i], SqlDbType = SqlDbType.Float, Direction = ParameterDirection.Input, Value = pars[i] } );
                else if (pars[i] is DateTime)
                    list.Add(new SqlParameter() { ParameterName = parNames[i], SqlDbType = SqlDbType.DateTime, Direction = ParameterDirection.Input, Value = pars[i] });
                else if(pars[i] == null)
                    list.Add(new SqlParameter() { ParameterName = parNames[i], SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input, Value = DBNull.Value } );
                else
                    list.Add(new SqlParameter() { ParameterName = parNames[i], SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input, Value = pars[i].ToString() });
            }

            return list;
        }
    }
}
