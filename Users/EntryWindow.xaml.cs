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
using System.Reflection;
using System.Diagnostics;
using DevExpress.Xpf.Core;
using System.IO;
using System.Threading;
using Budget.Model;

namespace Budget
{
    public partial class EntryWindow : DXWindow
    {
        //private bool _closedFlag;
        private bool _toAnotherFormFlag;  //флаг перехода на другую форму
        private bool _showChangeMessage;  //показывать ли сообщение об изменении версии
        private FileLoader _fileLoader;

        public EntryWindow()
        {
            InitializeComponent();

            //_closedFlag = false;
            _toAnotherFormFlag = false;
            _showChangeMessage = true;
            _fileLoader = null;

            var res = Utils.InitializeDefaultCS();
            if (!res.Item1)
                MessageBox.Show(res.Item2,
                                "Ошибка соединения с базой данных",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            CheckVersion();

            try
            {
                if (Properties.Settings.Default.ThemeIndex > 0 && Properties.Settings.Default.ThemeIndex <= Consts.DevExTheme.Count)
                    ThemeManager.SetTheme(this, Consts.DevExTheme[Properties.Settings.Default.ThemeIndex]);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            string login, password;
            if (Utils.IsAutoLogin(out login, out password))
            {  //автоматический вход
                checkBoxRememberMe.IsChecked = true;
                TryToLogIn(login, password);
            }
        }

        public void SetInitLogin(string login)
        {
            txtLogin.Text = login;
        }

        /// <summary>
        /// Проверка текущей версии
        /// </summary>
        private void CheckVersion()
        {
            var lastVersion = Database.GetSetting("LastVersion");
            var curVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            var link = Database.GetSetting("LastVersionLink");

            if (Utils.CompareVersions(curVersion, lastVersion) < 0)
            {
                if (Properties.Settings.Default.LastVersionSetupLoaded &&
                    File.Exists(String.Format("{0}\\{1}", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), System.IO.Path.GetFileName(link))))  //версия устаревшая, файл установки загружен и существует
                    ChangeVersion(lastVersion, link);
                else  //версия устаревшая, файл установки не загружен => загружаем
                {
                    _fileLoader = new FileLoader(lastVersion,
                                            String.Format("{0}\\{1}", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), System.IO.Path.GetFileName(link)),
                                            link);
                    _fileLoader.OnLoaded += () => { ChangeVersion(lastVersion, link); };
                }
            }
        }

        private void ChangeVersion(string lastVersion, string link)
        {
            if (_showChangeMessage)
            {
                if (MessageBox.Show(String.Format("Вышла новая версия программы Бюджет ({0}). Установочный файл уже у вас на компьютере. \n\nХотите обновить сейчас?", lastVersion),
                                "Информация",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    Process.Start(String.Format("{0}\\{1}", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), System.IO.Path.GetFileName(link)));
                    Process.GetCurrentProcess().Kill();
                }
            }
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            var w = new RegistrationWindow();
            w.Owner = this;
            w.OnSuccessfullyRegistered += (string login) => { txtLogin.Text = login; };
            w.ShowDialog();
        }

        private void btnLogIn_Click(object sender, RoutedEventArgs e)
        {
            TryToLogIn();
        }

        /// <summary>
        /// Попытка входа в систему
        /// </summary>
        private void TryToLogIn()
        {
            if (String.IsNullOrEmpty(txtLogin.Text) || String.IsNullOrEmpty(txtPassword.Password))
            {
                MessageBox.Show("Некоторые поля остались незаполеннными",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
            else
            {
                var res = Database.UserLogIn(txtLogin.Text, Utils.ToHash(txtPassword.Password));

                if (res.Status == LogInStatus.NonRegistered)
                {
                    MessageBox.Show("Пользователь с таким логином не зарегистрирован в системе. Проверьте введенные данные и повторите попытку входа",
                                    "Ошибка",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
                else if (res.Status == LogInStatus.WrongPassword)
                {
                    MessageBox.Show("Вы ввели неверный пароль. Войти в программу невозможно",
                                    "Ошибка",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
                else if (res.Status == LogInStatus.OK)
                {
                    this.Hide();
                    var w = new MainWindow(res.UserID, _fileLoader);
                    _toAnotherFormFlag = true;
                    _showChangeMessage = false;

                    if ((bool)checkBoxRememberMe.IsChecked)
                        Utils.EnableAutoLogin(txtLogin.Text, Utils.ToHash(txtPassword.Password));
                    else
                        Utils.DisableAutoLogin();

                    this.Close();
                    w.Show();
                }
            }
        }

        private void TryToLogIn(string login, string password)
        {
            var res = Database.UserLogIn(login, password);

            if (res.Status == LogInStatus.NonRegistered)
            {
                MessageBox.Show("Пользователь с таким логином не зарегистрирован в системе. Проверьте введенные данные и повторите попытку входа",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
            else if (res.Status == LogInStatus.WrongPassword)
            {
                MessageBox.Show("Вы ввели неверный пароль. Войти в программу невозможно",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
            else if (res.Status == LogInStatus.OK)
            {
                this.Hide();
                var w = new MainWindow(res.UserID, _fileLoader);
                _toAnotherFormFlag = true;
                _showChangeMessage = false;
                this.Close();
                w.Show();
            }
        }

        private void CheckDB(string LE)
        {
            if (LE.Length > 0)
            {
                MessageBox.Show(LE,
                                "Ошибка в базе данных",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return;
            }
        }

        private void txtPassword_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                TryToLogIn();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtLogin.Focus();
        }

        private void DXWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_fileLoader != null && !_toAnotherFormFlag)
            {
                _showChangeMessage = false;  //не показывать сообщение об изменении версии

                if (_fileLoader.State != FileLoader.LoaderState.Finished)
                    this.Hide();

                while (_fileLoader.State != FileLoader.LoaderState.Finished)
                { }
            }
        }
    }
}
