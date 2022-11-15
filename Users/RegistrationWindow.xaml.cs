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
using System.Data.SqlClient;
using DevExpress.Xpf.Core;
using am.BL;

namespace Budget
{
    public partial class RegistrationWindow : DXWindow
    {
        public delegate void RegisterHandler(string login);
        public event RegisterHandler OnSuccessfullyRegistered;

        public RegistrationWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lblError.Visibility = System.Windows.Visibility.Hidden;

            if (Properties.Settings.Default.ThemeIndex > 0 && Properties.Settings.Default.ThemeIndex <= Consts.DevExTheme.Count)
                ThemeManager.SetTheme(this, Consts.DevExTheme[Properties.Settings.Default.ThemeIndex]);

            txtLogin.Focus();
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            Register();
        }

        /// <summary>
        /// Процедура регистрации
        /// </summary>
        private void Register()
        {
            if (!String.IsNullOrEmpty(txtLogin.Text) && !String.IsNullOrEmpty(txtPassword.Password) && !String.IsNullOrEmpty(txtPasswordConfirm.Password))
            {
                var loginIsFree = Database.IsLoginFree(txtLogin.Text);

                if (!loginIsFree)
                    lblError.Visibility = System.Windows.Visibility.Visible;
                else
                {
                    lblError.Visibility = System.Windows.Visibility.Hidden;

                    if (txtPassword.Password == txtPasswordConfirm.Password)
                    {
                        var res = Database.RegisterUser(txtLogin.Text, Utils.ToHash(txtPassword.Password));
                        if (!String.IsNullOrEmpty(G.LastError))
                        {
                            MessageBox.Show(G.LastError, "Ошибка в базе данных", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        MessageBox.Show("Вы успешно зарегистрированы в системе",
                                        "Информация",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Information);
                        OnSuccessfullyRegistered(txtLogin.Text);
                        Close();
                    }
                    else
                        MessageBox.Show("Пароли не совпадают",
                                        "Ошибка",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                }
            }
            else
                MessageBox.Show("Некоторые поля остались пустыми",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
        }

        private void txtPasswordConfirm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Register();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
