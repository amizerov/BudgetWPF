using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DevExpress.Xpf.Core;
using am.BL;

namespace Budget
{
    public partial class Register : DXWindow
    {
        private int _userID;

        public delegate void RegisterHandler();
        public event RegisterHandler OnRegistered;

        public Register(int userID)
        {
            InitializeComponent();

            _userID = userID;

            txtEmail.Text = G._S(G.db_select("exec GetEmail {1}", _userID)).ToString();

            if (Properties.Settings.Default.ThemeIndex > 0 && Properties.Settings.Default.ThemeIndex <= Consts.DevExTheme.Count)
                ThemeManager.SetTheme(this, Consts.DevExTheme[Properties.Settings.Default.ThemeIndex]);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtEmail.Focus();
            txtEmail.SelectionStart = txtEmail.Text.Length;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void Save()
        {
            if (!String.IsNullOrEmpty(txtEmail.Text))
                G.db_exec("exec SetEmail {1}, '{2}'", _userID, txtEmail.Text);

            if (String.IsNullOrEmpty(G.LastError))
            {
                OnRegistered();
                this.Close();
            }
            else
                MessageBox.Show(G.LastError, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void txtEmail_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
                Save();
        }
    }
}
