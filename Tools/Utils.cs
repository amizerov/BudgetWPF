using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.Xpf.Grid;
using System.IO;
using System.Security.Cryptography;
using am.BL;
using System.Drawing;
using DevExpress.Xpf.Core;
using System.Data.SqlClient;
using DevExpress.XtraLayout.Customization.Templates;
using System.Data;
using DevExpress.XtraPrinting.Native.WebClientUIControl;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Windows;

namespace Budget
{
    static class Utils
    {
        /// <summary>
        /// Инициализация строки подключения по умолчанию
        /// </summary>
        public static Tuple<bool, string> InitializeDefaultCS()
        {
            var res = new Tuple<bool, string>(true, String.Empty);

            SqlConnection con = null;
            try
            {
                string s = Encryption.DecryptString(Properties.Settings.Default.ConnectionString, "JPo7R75zgJyg315d");
                am.DB.DBManager.Instance.Init(s, 90, 90);

                s = Encryption.EncryptString(s, "JPo7R75zgJyg315d");

                con = am.DB.DBManager.Instance.CreateConnection();
            }
            catch (Exception ex)
            {
                res = new Tuple<bool, string>(false, ex.Message);
            }

            return res;
        }

        /// <summary>
        /// Получить внешний IP пользователя
        /// </summary>
        public static string GetExternalIP(out string error)
        {
            error = String.Empty;

            //var request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create("http://checkip.dyndns.org/");
            //var response = (System.Net.HttpWebResponse)request.GetResponse();

            //var myExternalIP = string.Empty;
            //using (System.IO.StreamReader reader = new StreamReader(response.GetResponseStream()))
            //{
            //    myExternalIP = reader.ReadToEnd();
            //    reader.Close();

            //    try
            //    {
            //        int stIndex = myExternalIP.IndexOf("Current IP Address:") + "Current IP Address:".Length + 1;
            //        int finIndex = myExternalIP.IndexOf("</body>");
            //        myExternalIP = myExternalIP.Substring(stIndex, finIndex - stIndex);
            //    }
            //    catch(Exception ex)
            //    {
            //        error = ex.Message;
            //    }
            //}
            //response.Close();

            //return myExternalIP;

            return String.Empty;
        }

        /// <summary>
        /// Сравнение версий
        /// </summary>
        // <0 - 1ая меньше второй, >0 - 1ая больше первой, 0 - одинаковые версии
        public static int CompareVersions(string version1, string version2)
        {
            var ver1 = version1.Split('.').ToList();
            var ver2 = version2.Split('.').ToList();
            var v1 = new List<int>();
            var v2 = new List<int>();

            try
            {
                foreach (var mean in ver1)
                    v1.Add(Convert.ToInt32(mean));
                foreach (var mean in ver2)
                    v2.Add(Convert.ToInt32(mean));
            }
            catch { }

            if (v1.Count() == 4 && v2.Count() == 4)
            {
                if (v1[0] < v2[0])
                    return -1;
                else if (v1[0] > v2[0])
                    return 1;
                else if (v1[0] == v2[0])
                {
                    if (v1[1] < v2[1])
                        return -1;
                    else if (v1[1] > v2[1])
                        return 1;
                    else if (v1[1] == v2[1])
                    {
                        if (v1[2] < v2[2])
                            return -1;
                        else if (v1[2] > v2[2])
                            return 1;
                        else if (v1[2] == v2[2])
                        {
                            if (v1[3] < v2[3])
                                return -1;
                            else if (v1[3] > v2[3])
                                return 1;
                            else if (v1[3] == v2[3])
                                return 0;
                        }
                    }
                }
            }

            return 0;
        }

        /// <summary>
        /// Скачать изображение с сервера и сохранить в виде массива Byte
        /// </summary>
        public static Byte[] DownloadImageInBytes(string imageURL)
        {
            Byte[] buf = null;

            try
            {
                System.Net.HttpWebRequest _HttpWebRequest = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(imageURL);
                _HttpWebRequest.AllowWriteStreamBuffering = true;
                _HttpWebRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1)";
                _HttpWebRequest.Referer = "http://www.google.com/";
                _HttpWebRequest.Timeout = 10000;

                System.Net.WebResponse _WebResponse = _HttpWebRequest.GetResponse();
                System.IO.Stream _WebStream = _WebResponse.GetResponseStream();
                Image tmpImage = Image.FromStream(_WebStream);

                var stream = new MemoryStream();
                tmpImage.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                buf = stream.ToArray();

                _WebResponse.Close();
            }
            catch (Exception _Exception)
            {
                Console.WriteLine("Возникло исключение: {0}", _Exception.ToString());
                return null;
            }

            return buf;
        }

        /// <summary>
        /// Активировать автоматический вход
        /// </summary>
        public static void EnableAutoLogin(string login, string password)
        {
            Properties.Settings.Default.RememberMe = true;
            Properties.Settings.Default.Login = login;
            Properties.Settings.Default.Password = password;

            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Деактивировать автоматический вход
        /// </summary>
        public static void DisableAutoLogin()
        {
            Properties.Settings.Default.RememberMe = false;
            Properties.Settings.Default.Login = String.Empty;
            Properties.Settings.Default.Password = String.Empty;

            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Включен ли автоматический вход
        /// </summary>
        public static bool IsAutoLogin(out string login, out string password)
        {
            login = Properties.Settings.Default.Login;
            password = Properties.Settings.Default.Password;

            return Properties.Settings.Default.RememberMe;
        }

        /// <summary>
        /// Выйти
        /// </summary>
        public static string AutoLoginExit()
        {
            Properties.Settings.Default.RememberMe = false;
            Properties.Settings.Default.Password = String.Empty;

            Properties.Settings.Default.Save();

            return Properties.Settings.Default.Login;
        }

        #region Save Restore Grid Layout
        static string defaultPath = G.GetCurDir2() + "Tools\\";
        static string userPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\UZ.Budget\\";
        static string accountsGridXml = "_accountsMainWnd.xml";
        static string defaultAccountsGridXml = defaultPath + accountsGridXml;
        static string userAccountsGridXml = userPath + accountsGridXml;
        static string operationsGridXml = "_operationsMainWnd.xml";
        static string defaultOperationsGridXml = defaultPath + operationsGridXml;
        static string userOperationsGridXml = userPath + operationsGridXml;
        /// <summary>
        /// Сохранить разметку таблицы аккаунтов
        /// </summary>
        public static void SaveAccountsLayout(GridControl aGridControl)
        {
            if(!Directory.Exists(userPath))
                Directory.CreateDirectory(userPath);

            aGridControl.SaveLayoutToXml(userAccountsGridXml);
        }

        /// <summary>
        /// Восстановить разметку таблиц аккаунтов
        /// </summary>
        public static void RestoreAccountsLayout(GridControl aGridControl)
        {
            string xml = File.Exists(userAccountsGridXml) ? userAccountsGridXml : defaultAccountsGridXml;

            if (File.Exists(xml))
                aGridControl.RestoreLayoutFromXml(xml);
        }

        /// <summary>
        /// Сохранить разметку таблицы операций
        /// </summary>
        public static void SaveOperationsLayout(GridControl oGridControl)
        {
            if (!Directory.Exists(userPath))
                Directory.CreateDirectory(userPath);

            oGridControl.SaveLayoutToXml(userOperationsGridXml);
        }

        /// <summary>
        /// Восстановить разметку таблиц операций
        /// </summary>
        public static void RestoreOperationsLayout(GridControl oGridControl)
        {
            string xml = File.Exists(userOperationsGridXml) ? userOperationsGridXml : defaultOperationsGridXml;

            if (File.Exists(xml))
                oGridControl.RestoreLayoutFromXml(xml);
        }
        #endregion

        /// <summary>
        /// Сохранить даты 
        /// </summary>
        public static void SaveDates(DateTime fromDate, DateTime toDate)
        {
            var serializedDates = String.Format("{0}-{1:00}-{2:00}|{3}-{4:00}-{5:00}",
                                            fromDate.Year, fromDate.Month, fromDate.Day,
                                            toDate.Year, toDate.Month, toDate.Day);
            Properties.Settings.Default.Dates = serializedDates;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Восстановить даты
        /// </summary>
        public static void RestoreDates(out DateTime fromDate, out DateTime toDate)
        {
            fromDate = default(DateTime);
            toDate = default(DateTime);

            var deserializedDates = Properties.Settings.Default.Dates;
            if (!String.IsNullOrEmpty(deserializedDates))
            {
                var datesArray = deserializedDates.Split('|');
                fromDate = Convert.ToDateTime(datesArray[0]);
                toDate = Convert.ToDateTime(datesArray[1]);
            }

           if (fromDate == default(DateTime))
                fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            if (toDate == default(DateTime) || toDate < DateTime.Now)
                toDate = DateTime.Now;
        }

        /// <summary>
        /// Сохранить даты для окна перерасчета остатков
        /// </summary>
        public static void SaveRecalcDates(DateTime fromDate, DateTime toDate)
        {
            var serializedDates = String.Format("{0}-{1:00}-{2:00}|{3}-{4:00}-{5:00}",
                                            fromDate.Year, fromDate.Month, fromDate.Day,
                                            toDate.Year, toDate.Month, toDate.Day);
            Properties.Settings.Default.RecalcDates = serializedDates;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Восстановить даты для окна перерасчета остатков
        /// </summary>
        public static void RestoreRecalcDates(out DateTime fromDate, out DateTime toDate)
        {
            fromDate = default(DateTime);
            toDate = default(DateTime);

            var deserializedDates = Properties.Settings.Default.RecalcDates;
            if (!String.IsNullOrEmpty(deserializedDates))
            {
                var datesArray = deserializedDates.Split('|');
                fromDate = Convert.ToDateTime(datesArray[0]);
                toDate = Convert.ToDateTime(datesArray[1]);
            }

            if (fromDate == default(DateTime))
                fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            if (toDate == default(DateTime) || toDate < DateTime.Now)
                toDate = DateTime.Now;
        }

        /// <summary>
        /// Сохранить даты для окна отчетов по аккаунтам
        /// </summary>
        public static void SaveReportAccountsDates(DateTime fromDate, DateTime toDate)
        {
            var serializedDates = String.Format("{0}-{1:00}-{2:00}|{3}-{4:00}-{5:00}",
                                            fromDate.Year, fromDate.Month, fromDate.Day,
                                            toDate.Year, toDate.Month, toDate.Day);
            Properties.Settings.Default.ReportAccountsDates = serializedDates;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Восстановить даты для окна отчетов по аккаунтам
        /// </summary>
        public static void RestoreReportAccountsDates(out DateTime fromDate, out DateTime toDate)
        {
            fromDate = default(DateTime);
            toDate = default(DateTime);

            var deserializedDates = Properties.Settings.Default.ReportAccountsDates;
            if (!String.IsNullOrEmpty(deserializedDates))
            {
                var datesArray = deserializedDates.Split('|');
                fromDate = Convert.ToDateTime(datesArray[0]);
                toDate = Convert.ToDateTime(datesArray[1]);
            }

            if (fromDate == default(DateTime))
                fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            if (toDate == default(DateTime) || toDate < DateTime.Now)
                toDate = DateTime.Now;
        }

        /// <summary>
        /// Сохранить даты для окна отчетов по категориям
        /// </summary>
        public static void SaveReportCategoriesDates(DateTime fromDate, DateTime toDate)
        {
            var serializedDates = String.Format("{0}-{1:00}-{2:00}|{3}-{4:00}-{5:00}",
                                            fromDate.Year, fromDate.Month, fromDate.Day,
                                            toDate.Year, toDate.Month, toDate.Day);
            Properties.Settings.Default.ReportCategoriesDates = serializedDates;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Восстановить даты для окна отчетов по категориям
        /// </summary>
        public static void RestoreReportCategoriesDates(out DateTime fromDate, out DateTime toDate)
        {
            fromDate = default(DateTime);
            toDate = default(DateTime);

            var deserializedDates = Properties.Settings.Default.ReportCategoriesDates;
            if (!String.IsNullOrEmpty(deserializedDates))
            {
                var datesArray = deserializedDates.Split('|');
                fromDate = Convert.ToDateTime(datesArray[0]);
                toDate = Convert.ToDateTime(datesArray[1]);
            }

            if (fromDate == default(DateTime))
                fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            if (toDate == default(DateTime) || toDate < DateTime.Now)
                toDate = DateTime.Now;
        }

        /// <summary>
        /// Сохранить размер главного окна и его положение
        /// </summary>
        public static void SaveFormCoords(DXWindow form)
        {
            Properties.Settings.Default.FormCoords = String.Format("{0}|{1}|{2}|{3}|{4}",
                                                                    form.Width, form.Height, form.Left, form.Top,
                                                                    form.WindowState == System.Windows.WindowState.Maximized ? true : false);
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Восстановить размер главного окна и его положение
        /// </summary>
        public static void RestoreFormCoords(DXWindow form)
        {
            var formCoordsStr = Properties.Settings.Default.FormCoords;

            if (!String.IsNullOrEmpty(formCoordsStr))
            {
                var means = formCoordsStr.Split('|');

                if (Convert.ToBoolean(means[4]))
                    form.WindowState = System.Windows.WindowState.Maximized;
                else
                {
                    form.Width = (int)Convert.ToDouble(means[0]);
                    form.Height = (int)Convert.ToDouble(means[1]);
                    form.Left = (int)Convert.ToDouble(means[2]);
                    form.Top = (int)Convert.ToDouble(means[3]);
                }
            }
            
        }

        /// <summary>
        /// Сохранить размер окна категорий
        /// </summary>
        public static void SaveCatFormCoords(DXWindow form)
        {
            Properties.Settings.Default.CatFormCoords = String.Format("{0}|{1}|{2}|{3}|{4}",
                                                                      form.Width, form.Height, form.Left, form.Top,
                                                                      form.WindowState == System.Windows.WindowState.Maximized ? true : false);
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Восстановить размер окна категорий
        /// </summary>
        public static void RestoreCatFormCoords(DXWindow form)
        {
            var formCoordsStr = Properties.Settings.Default.CatFormCoords;

            if (!String.IsNullOrEmpty(formCoordsStr))
            {
                var means = formCoordsStr.Split('|');

                if (Convert.ToBoolean(means[4]))
                    form.WindowState = System.Windows.WindowState.Maximized;
                else
                {
                    form.Width = Convert.ToInt32(means[0]);
                    form.Height = Convert.ToInt32(means[1]);
                    form.Left = Convert.ToInt32(means[2]);
                    form.Top = Convert.ToInt32(means[3]);
                }
            }
        }

        /// <summary>
        /// Сохранить дистанцию разделителя
        /// </summary>
        public static void SaveSplitterDist(double dist)
        {
            Properties.Settings.Default.SplitterDist = dist;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Восстановить дистанцию сплиттера
        /// </summary>
        public static double RestoreSplitterDist()
        {
            return Properties.Settings.Default.SplitterDist;
        }

        /// <summary>
        /// Захешировать строку
        /// </summary>
        public static string ToHash(string inputStr)
        {
            var md5 = new MD5CryptoServiceProvider();
            var original = ASCIIEncoding.Default.GetBytes(inputStr);
            var encoded = md5.ComputeHash(original);

            return BitConverter.ToString(encoded);
        }

        /// <summary>
        /// Обновить баннер с рейтингом
        /// </summary>
        public static void UpdateRatingBanner(Queue<Banner> banners, int userID)
        {
            var quantity = banners.Count;
            var counter = -1;

            var tempBanner = new Banner();
            Banner ratingBanner = null;

            while (tempBanner.Type != Banner.BannerType.Rating)
            {
                if (++counter >= quantity)
                    break;

                tempBanner = banners.Dequeue();
                banners.Enqueue(tempBanner);

                if (tempBanner.Type == Banner.BannerType.Rating)
                {
                    ratingBanner = tempBanner;
                    break;
                }
            }

            if (ratingBanner == null)
            {
                ratingBanner = new Banner(-1,
                                          Banner.BannerType.Rating,
                                          String.Format("На вашем счету {0} балла(ов). В дальнейшем вы можете использовать баллы для оплаты услуг", G._S(G.db_select("GetRating {1}", userID))),
                                          null, null);
                banners.Enqueue(ratingBanner);
            }
            
            Utils.RewindBannersToRating(banners);
        }

        /// <summary>
        /// Перемотать баннер до рейтингового
        /// </summary>
        private static void RewindBannersToRating(Queue<Banner> banners)
        {
            var tempBanner = new Banner();

            while (tempBanner.Type != Banner.BannerType.Rating)
            {
                tempBanner = banners.Dequeue();
                banners.Enqueue(tempBanner);
            }
            for (int i = 0; i < banners.Count - 1; i++)
            {
                tempBanner = banners.Dequeue();
                banners.Enqueue(tempBanner);
            }
        }

        /// <summary>
        /// Сделать полный бекап всех счетов
        /// TODO: переделать на понятный формат типа JSON
        /// </summary>
        public static void Backup(int userID, string filePath)
        {
            var bw = new BinaryWriter(File.Open(filePath, FileMode.OpenOrCreate));

            var dt = G.db_select("BackupGetAccounts {1}", userID);
            bw.Write(dt.Rows.Count);
            bw.Write(dt.Columns.Count);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                for (int j = 0; j < dt.Columns.Count; j++)
                    bw.Write(dt.Rows[i][j].ToString());
            }

            dt = G.db_select("BackupGetCategories {1}", userID);
            bw.Write(dt.Rows.Count);
            bw.Write(dt.Columns.Count);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                for (int j = 0; j < dt.Columns.Count; j++)
                    bw.Write(dt.Rows[i][j].ToString());
            }

            dt = G.db_select("BackupGetAccountsCategoriesRating {1}", userID);
            bw.Write(dt.Rows.Count);
            bw.Write(dt.Columns.Count);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                for (int j = 0; j < dt.Columns.Count; j++)
                    bw.Write(dt.Rows[i][j].ToString());
            }

            dt = G.db_select("BackupGetOperations {1}", userID);
            bw.Write(dt.Rows.Count);
            bw.Write(dt.Columns.Count);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    if (j == 7 || j == 8)  //даты сохраняем в специальном формате
                    {
                        var tempDate = Convert.ToDateTime(dt.Rows[i][j]);
                        bw.Write(tempDate.ToString("yyyy-MM-dd HH:mm:ss")); 
                    }
                    else
                        bw.Write(dt.Rows[i][j].ToString());

                }
            }

            bw.Close();
        }

        /// <summary>
        /// Восстановить из файла
        /// </summary>
        public static bool Restore(string filePath, int userID, out string error)
        {
            error = String.Empty;

            var br = new BinaryReader(File.Open(filePath, FileMode.Open));
            bool allAccountsExistInDB = true;
            bool allCategoriesExistInDB = true;

            //Проверка, существуют ли все аккаунты в базе данных
            var rCount = br.ReadInt32();
            var cCount = br.ReadInt32();
            var account = new List<string>();
            for (int i = 0; i < rCount; i++)
            {
                account.Clear();
                for (int j = 0; j < cCount; j++)
                    account.Add(br.ReadString());

                var checkAccRes = G._S(G.db_select("exec BackupCheckAccount {1}, '{2}', {3}", account[0], account[1], account[3] == "True" ? 1 : 0));
                if (checkAccRes == "0" || checkAccRes == "2")
                    allAccountsExistInDB = false;

                if (!String.IsNullOrEmpty(G.LastError))
                {
                    error = G.LastError;
                    return false;
                }
            }

            //Проверка, существуют ли все категории в базе данных
            rCount = br.ReadInt32();
            cCount = br.ReadInt32();
            var category = new List<string>();
            for (int i = 0; i < rCount; i++)
            {
                category.Clear();
                for (int j = 0; j < cCount; j++)
                    category.Add(br.ReadString());

                var checkCatRes = G._S(G.db_select("exec BackupCheckCategory {1}, '{2}'", category[0], category[1]));
                if (checkCatRes == "0" || checkCatRes == "2")
                    allCategoriesExistInDB = false;

                if (!String.IsNullOrEmpty(G.LastError))
                {
                    error = G.LastError;
                    return false;
                }
            }

            if (!allAccountsExistInDB && !allCategoriesExistInDB)
            {
                error = "Невозможно восстановить бекап из файла, так как некоторые счета и категории были модифицированы или удалены";
                return false;
            }
            else if (!allAccountsExistInDB && allCategoriesExistInDB)
            {
                error = "Невозможно восстановить бекап из файла, так как некоторые счета были модифицированы или удалены";
                return false;
            }
            else if (allAccountsExistInDB && !allCategoriesExistInDB)
            {
                error = "Невозможно восстановить бекап из файла, так как некоторые категории были модифицированы или удалены";
                return false;
            }
            else  //восстановление
            {
                br.Close();
                br = new BinaryReader(File.Open(filePath, FileMode.Open));

                var accountsID = new Dictionary<string, string>();    //сочетания "старый ID - новый ID" для аккаунтов
                var categoriesID = new Dictionary<string, string>();  //сочетания "старый ID - новый ID" для категорий

                //восстановление аккаунтов
                rCount = br.ReadInt32();
                cCount = br.ReadInt32();
                for (int i = 0; i < rCount; i++)
                {
                    account.Clear();
                    for (int j = 0; j < cCount; j++)
                        account.Add(br.ReadString());

                    var checkAccRes = G._S(G.db_select("exec BackupCheckAccount {1}, '{2}', {3}", account[0], account[1], account[3] == "True" ? 1 : 0));
                    if (checkAccRes == "1")
                    {  //аккаунт не модифицирован - обновляем все поля, кроме имени
                        G.db_exec("BackupSetAccount {1}, '{2}', {3}, {4}, {5}, {6}", account[0], account[1], account[2].Replace(',', '.'), account[3] == "False" ? 0 : 1, account[4], account[5]);
                        if (!String.IsNullOrEmpty(G.LastError))
                        {
                            error = G.LastError;
                            return false;
                        }

                        accountsID.Add(account[0], account[0]);
                    }
                    else if (checkAccRes == "2")
                    {  //аккаунт удален - добавляем в БД, обновляем ID локально
                        var newAccountID = G._S(G.db_select("exec AddAccount '{1}', {2}", account[1], account[4]));
                        G.db_exec("BackupSetAccount {1}, '{2}', {3}, {4}, {5}, {6}", newAccountID, account[1], account[2].Replace(',', '.'), account[3] == "False" ? 0 : 1, account[4], account[5]);
                        if (!String.IsNullOrEmpty(G.LastError))
                        {
                            error = G.LastError;
                            return false;
                        }

                        accountsID.Add(account[0], newAccountID);
                    }
                }

                //восстановление категорий
                rCount = br.ReadInt32();
                cCount = br.ReadInt32();
                for (int i = 0; i < rCount; i++)
                {
                    category.Clear();
                    for (int j = 0; j < cCount; j++)
                        category.Add(br.ReadString());

                    var checkCatRes = G._S(G.db_select("exec BackupCheckCategory {1}, '{2}'", category[0], category[1]));
                    if (checkCatRes == "1")
                    {  //категория не модифицирована - обновляем все поля, кроме имени
                        G.db_exec("BackupSetCategories {1}, '{2}', {3}, {4}, {5}, {6}", category[0], category[1], category[2], category[3], category[4], category[5]);
                        if (!String.IsNullOrEmpty(G.LastError))
                        {
                            error = G.LastError;
                            return false;
                        }

                        categoriesID.Add(category[0], category[0]);
                    }
                    else if (checkCatRes == "2")
                    {  //категория удалена - добавляем в БД, обновляем ID локально
                        var newCategoryID = G._S(G.db_select("exec AddCategory '{1}', {2}", category[1], category[5]));
                        G.db_exec("BackupSetCategories {1}, '{2}', {3}, {4}, {5}, {6}", newCategoryID, category[1], category[2], category[3], category[4], category[5]);
                        if (!String.IsNullOrEmpty(G.LastError))
                        {
                            error = G.LastError;
                            return false;
                        }

                        categoriesID.Add(category[0], newCategoryID);
                    }
                }

                //удаление всех рейтингов и операций
                G.db_exec("DeleteInfoByUserID {1}", userID);

                //восстановление рейтингов для сочетаний категорий и аккаунтов
                rCount = br.ReadInt32();
                cCount = br.ReadInt32();
                var catRating = new List<string>();
                for (int i = 0; i < rCount; i++)
                {
                    catRating.Clear();
                    for (int j = 0; j < cCount; j++)
                        catRating.Add(br.ReadString());

                    G.db_exec("BackupSetAccountsCategoriesRating {1}, {2}, {3}, {4}", catRating[0], accountsID[catRating[1]], categoriesID[catRating[2]], catRating[3]);
                    if (!String.IsNullOrEmpty(G.LastError))
                    {
                        error = G.LastError;
                        return false;
                    }
                }

                //восстановление операций
                rCount = br.ReadInt32();
                cCount = br.ReadInt32();
                var operation = new List<string>();
                for (int i = 0; i < rCount; i++)
                {
                    operation.Clear();
                    for (int j = 0; j < cCount; j++)
                        operation.Add(br.ReadString());

                    G.db_exec("BackupSetOperations {1}, '{2}', {3}, {4}, {5}, {6}, {7}, '{8}', '{9}', {10}",
                              operation[0],
                              operation[1],
                              operation[2].Replace(',', '.'),
                              categoriesID[operation[3]],
                              String.IsNullOrEmpty(operation[4]) ? "NULL" : accountsID[operation[4]],
                              String.IsNullOrEmpty(operation[5]) ? "NULL" : accountsID[operation[5]],
                              operation[6],
                              operation[7],
                              operation[8],
                              operation[9]);
                    if (!String.IsNullOrEmpty(G.LastError))
                    {
                        error = G.LastError;
                        return false;
                    }
                }

                br.Close();
                return true;
            }
        }

        public static string InsertHeaderInHtml(string html, string accountName, DateTime? stDate, DateTime? finDate)
        {
            int indStBody = html.IndexOf("<body");
            var temp = html.Substring(indStBody, html.Length - indStBody);
            int indFinBody = temp.IndexOf(">")+1 + indStBody;

            html = html.Insert(indFinBody, String.Format("\n<p align=\"right\"><a href=\"http://ultrazoom.ru\">http://www.ultrazoom.ru</a> * Бюджет * +7 (495) 514-3772</p>" +
                                                         "\n<h3>Выписка по счету \"{0}\" с {1} по {2}</h3>", 
                                                         accountName,
                                                         DtcOnlyDateToString(stDate),
                                                         DtcOnlyDateToString(finDate)));

            return html;
        }

        public static string DtcOnlyDateToString(DateTime? dtc)
        {
            var res = "?";
            if (dtc == null)
                return res;

            string[] months = { "января", "февраля", "марта", "апреля", "мая", "июня", "июля", "августа", "сентября", "октября", "ноября", "декабря" };
            res = String.Format("{0} {1} {2} г.", ((DateTime)dtc).Day, months[((DateTime)dtc).Month - 1], ((DateTime)dtc).Year);

            return res;
        }

        public static string LoadTempImage(int userID, int operationID)
        {
            var path = String.Empty;

            string fName;
            var img = Transport.GetImage(userID, operationID, out fName);
            var baseFolder = Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            var fileName = String.Format("{0}_{1}_{2}", userID, operationID, fName);
            path = String.Format("{0}\\{1}", baseFolder, fileName);

            try
            {
                if (File.Exists(path)) File.Delete(path);
                img.Save(path);
            }
            catch { }

            return path;
        }

        public static void DelTempImage(string path)
        {
            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch { }
        }
    }
}
