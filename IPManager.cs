using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using am.BL;
using System.Data.SqlClient;

namespace Budget
{
    class IPManager
    {
        private int _userID;
        private string _externalIP;

        public IPManager(int userID)
        {
            _externalIP = String.Empty;
            _userID = userID;

            this.StartLoad();
        }

        private void StartLoad()
        {
            var ipThread = new Thread(new ThreadStart(BackgroundIPLoader));
            ipThread.Start();
        }

        public void BackgroundIPLoader()
        {
            string error = String.Empty;
            _externalIP = Utils.GetExternalIP(out error);

            if (String.IsNullOrEmpty(error))   //нет ошибок
            {
                ConnectToDatabase();
                try
                {
                    G.db_select("exec SetUserIP {1}, '{2}'", _userID, _externalIP);
                }
                catch { }
            }
        }

        private void ConnectToDatabase()
        {
            SqlConnection con = null;
            try
            {
                am.DB.DBManager.Instance.Init(Encryption.DecryptString(Properties.Settings.Default.ConnectionString, "JPo7R75zgJyg315d"), 90, 90);
                con = am.DB.DBManager.Instance.CreateConnection();
            }
            catch
            {
            }
        }
    }
}
