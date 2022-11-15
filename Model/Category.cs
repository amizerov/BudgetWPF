using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Budget.Model
{
    public class Category
    {
        private int _id;
        private string _name;
        private string _creditText;
        private string _debetText;

        private double _creditPercent;
        private double _debetPercent;

        private double _creditColWidth;
        private double _debetColWidth;

        public Category()
        {
            _id = -1;
            _name = String.Empty;
            _creditText = String.Empty;
            _debetText = String.Empty;
            _creditPercent = 0;
            _debetPercent = 0;
            _creditColWidth = 0;
            _debetColWidth = 0;
        }

        public Category(int id, string name, string credit, string debet, int gridWidth) : base()
        {
            _id = id;
            _name = name;
            _creditText = credit;
            _debetText = debet;
            _creditPercent = 0;
            _debetPercent = 0;

            var cred = new List<string>();
            var deb = new List<string>();
            if (!String.IsNullOrEmpty(_creditText))
            {
                cred = _creditText.Replace('.', ',').Split('*').ToList<string>();
                if (!String.IsNullOrEmpty(cred[1])) _creditPercent = Convert.ToDouble(cred[1]);

                _creditText = String.Format("{0} руб. ({1}%)", cred[0], cred[1]);
            }
            if (!String.IsNullOrEmpty(_debetText))
            {
                deb = _debetText.Replace('.', ',').Split('*').ToList<string>();
                if (!String.IsNullOrEmpty(deb[1])) _debetPercent = Convert.ToDouble(deb[1]);
                
                _debetText = String.Format("{0} руб. ({1}%)", deb[0], deb[1]);
            }

            if (_creditPercent > 100) _creditPercent = 100;
            if (_debetPercent > 100) _debetPercent = 100;

            _creditColWidth = (_creditPercent / 100) * gridWidth * 0.39;
            _debetColWidth = (_debetPercent / 100) * gridWidth * 0.39;
        }

        public Category(DataRow row) : base()
        {
            try { _id = Convert.ToInt32(row["ID"]); } catch { }
            Name = row["Name"].ToString();
            try { _creditText = row["Credit"].ToString(); } catch {}
            try { _debetText = row["Debet"].ToString(); } catch {}
        }

        public int Id
        {
            [DebuggerStepThrough]
            get { return _id; }
            [DebuggerStepThrough]
            set { _id = value; }
        }

        public string Name
        {
            [DebuggerStepThrough]
            get { return _name; }
            [DebuggerStepThrough]
            set { _name = value; }
        }

        public string CreditText
        {
            [DebuggerStepThrough]
            get { return _creditText; }
            [DebuggerStepThrough]
            set { _creditText = value; }
        }

        public string DebetText
        {
            [DebuggerStepThrough]
            get { return _debetText; }
            [DebuggerStepThrough]
            set { _debetText = value; }
        }

        public double CreditPercent
        {
            get { return _creditPercent; }
        }

        public double DebetPercent
        {
            get { return _debetPercent; }
        }

        public double CreditColWidth
        {
            get { return _creditColWidth; }
        }

        public double DebetColWidth
        {
            get { return _debetColWidth; }
        }
    }
}
