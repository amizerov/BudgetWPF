using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Budget.Model;
using am.BL;
using System.Data;

namespace Budget
{
    public class Database
    {

        #region Настройки

        public static string GetSetting(string key)
        {
            return G._S(db.select("exec GetSetting @key", key)).Trim();
        }

        #endregion Настройки

        #region Вход/выход

        public static LogInResult UserLogIn(string login, string hashPass)
        {
            var res = new LogInResult();

            U.Cur._user_id = 
            res.UserID = G._I(db.select("exec UserLogIn @login, @password", login, hashPass));

            if (res.UserID == -1)
                res.Status = LogInStatus.NonRegistered;
            else if (res.UserID == -2)
                res.Status = LogInStatus.WrongPassword;
            else if (res.UserID > 0)
                res.Status = LogInStatus.OK;

            return res;
        }

        public static void UserLogOut(int userID)
        {
            db.exec("exec UserLogOut @userID", userID);
        }

        #endregion Вход/выход

        #region Пользователь

        public static string GetUserName(int userID)
        {
            return G._S(db.select("exec GetUserName @userID", userID));
        }

        #endregion Пользователь

        #region Версия программы

        public static void SetVersion(int userID, string version)
        {
            db.exec("exec SetVersion @userID, @version", userID, version);
        }

        #endregion Версия программы

        #region Регистрация

        public static bool IsLoginFree(string login)
        {
            return G._B(db.select("exec IsLoginFree @login", login));
        }

        public static RegResult RegisterUser(string login, string hashPass, int ParentID = 0)
        {
            var res = RegResult.None;
            if (ParentID == 0)
            {
                var regRes = G._I(db.select("exec RegisterUser @login, @password", login, hashPass));
                if (regRes == 1)
                    res = RegResult.UserAlreadyExists;
                else if (regRes == 0)
                    res = RegResult.OK;
            }
            else
            {
                var regRes = G._I(db.select("exec SetUser @LoginName, @LoginEMail, @Password, @ParentID", login, login, hashPass, ParentID));
                if (regRes == -1)
                    res = RegResult.UserAlreadyExists;
                else if (regRes > 0)
                    res = RegResult.OK;
            }

            return res;
        }

        public static RegResult RegisterUser(string login, string hashPass, string ip, string email, string version)
        {
            var res = RegResult.None;

            var regRes = G._I(db.select("exec RegisterUser @login, @password, @ip, @email, @version", login, hashPass, ip, email, version));
            if (regRes == 0)
                res = RegResult.UserAlreadyExists;
            else if (regRes == 1)
                res = RegResult.OK;

            return res;
        }

        #endregion Регистрация

        #region Статусы операций

        public static List<OperationStatus> GetOperationStatuses()
        {
            var list = new List<OperationStatus>();

            var dt = db.select("exec GetOperationStatuses");
            foreach (DataRow row in dt.Rows)
                list.Add(new OperationStatus(row));

            return list;
        }

        #endregion Статусы операций

        #region Min, max OperDay

        public static DateTime GetMinOperDay(int userID)
        {
            return G._D(db.select_scalar("exec GetMinOperDay @userID", userID));
        }

        public static DateTime GetMaxOperDay(int userID)
        {
            return G._D(db.select_scalar("exec GetMaxOperDay @userID", userID));
        }

        #endregion Min, max OperDay

        #region Аккаунты

        public static DataTable GetAccountTable(DateTime dateDown, DateTime dateUp, int userID)
        {
            return db.select("exec GetAccountTableAM22 @beginDate, @endDate, @userID", dateDown, dateUp, userID);
        }

        public static List<ExportAccount> GetAccountsForExport(int userID)
        {
            var list = new List<ExportAccount>();

            var dt = db.select("exec GetAccountsForExport @userID", userID);
            foreach (DataRow row in dt.Rows)
                list.Add(new ExportAccount(row));

            return list;
        }

        public static ExportAccount GetAccount(int accountID)
        {
            var account = new ExportAccount();

            var dt = db.select("exec GetAccount @accountID", accountID);
            if (dt.Rows.Count > 0)
                account = new ExportAccount(dt.Rows[0]);

            return account;
        }

        #endregion Аккаунты

        #region Категории

        public static List<Category> GetAllCategories(int userID)
        {
            var list = new List<Category>();

            var dt = db.select("exec GetAllCategories @userID", userID);
            foreach (DataRow row in dt.Rows)
                list.Add(new Category(row));

            return list;
        }

        public static List<ExportCategory> GetCategoriesForExport(int userID)
        {
            var list = new List<ExportCategory>();

            var dt = db.select("exec GetCategoriesForExport @userID", userID);
            foreach (DataRow row in dt.Rows)
                list.Add(new ExportCategory(row));

            return list;
        }

        #endregion Категории

        #region Операции

        public static DataTable GetOperationTable(DateTime beginDate, DateTime endDate, int accountID)
        {
            if(accountID > 0)
                return db.select("GetOperationTable @beginDate, @endDate, @accountID", beginDate, endDate, accountID); 
            
            return db.select("GetOperationTableByUserID @beginDate, @endDate, @userID", beginDate, endDate, U.Cur.ID);
        }

        public static List<ExportOperation> GetOperationsForExport(int accountID)
        {
            var list = new List<ExportOperation>();

            var dt = db.select("exec GetOperationsForExport @accountID", accountID);
            foreach (DataRow row in dt.Rows)
                list.Add(new ExportOperation(row));

            return list;
        }

        public static int DuplicateOperation(int operationID)
        {
            return G._I(db.select("exec DuplicateOperation @operationID", operationID));
        }

        public static void CarryCreditOperation(int operationID)
        {
            db.exec("exec CarryCreditOperation @operationId", operationID);
        }

        public static void CarryDebetOperation(int operationID)
        {
            db.exec("exec CarryDebetOperation @operationId", operationID);
        }

        #endregion Операции

        #region Баннеры

        public static List<Banner> GetBannersWithAuthorization(int userID)
        {
            var list = new List<Banner>();

            var dt = db.select("exec GetBannersWithAuthorization @userID", userID);
            foreach (DataRow row in dt.Rows)
                list.Add(new Banner(row));

            return list;
        }

        #endregion Баннеры

        #region Подсказки

        public static bool HintCheckNoAccounts(int userID)
        {
            return G._B(db.select("exec HintCheckNoAccounts @userID", userID));
        }

        public static bool HintCheckNoCategories(int userID)
        {
            return G._B(db.select("exec HintCheckNoAccounts @userID", userID));
        }

        public static bool HintCheckNoOperations(int userID)
        {
            return G._B(db.select("exec HintCheckNoOperations @userID", userID));
        }

        #endregion Подсказки

        #region Добавить комментарий, ошибку, предложение, вопрос

        public static void AddComment(int typeID, string comment, int userID)
        {
            db.exec("exec AddComment @typeID, @comment, @userID", typeID, comment, userID);
        }

        #endregion

    }

    #region Enums

    public enum LogInStatus
    {
        None = 0,
        NonRegistered,
        WrongPassword,
        OK
    }

    public enum RegResult
    {
        None = 0,
        UserAlreadyExists,
        OK
    }

    #endregion Enums

}