using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Budget.Model
{
    public class LogInResult
    {
        public LogInStatus Status { get; set; }
        public int UserID { get; set; }

        public LogInResult()
        {
            Status = LogInStatus.None;
            UserID = 0;
        }

        public LogInResult(LogInStatus status, int userID)
        {
            Status = status;
            UserID = userID;
        }
    }
}
