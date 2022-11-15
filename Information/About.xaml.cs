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
using System.Reflection;

namespace Budget
{
    public partial class About : DXWindow
    {
        private MainWindow _mw;

        public About(MainWindow mw)
        {
            InitializeComponent();

            this.Owner = _mw = mw;

            string ver = "Версия " + Assembly.GetExecutingAssembly().GetName().Version.ToString() + " " + _mw._BuildTime.ToString();

            txtAbout.Text =

"Программа \"Бюджет\"\n" + ver + @"

Бюджет - бесплатная и простая в использовании программа, которая предназначена для ведения учета в небольшой компании или домашней бухгалтерии. 
Присутствует возможность добавления и редактирования счетов, проведения по ним различных операций, осуществление перевода средств со счета на счет, получение статистики, экспорт в Excel и многое другое.";

            if (Properties.Settings.Default.ThemeIndex > 0 && Properties.Settings.Default.ThemeIndex <= Consts.DevExTheme.Count)
                ThemeManager.SetTheme(this, Consts.DevExTheme[Properties.Settings.Default.ThemeIndex]);

            button1.Focus();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DXWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (_mw._userID == 34)
            {
                var w = new Parking.SimakOperations();
                w.Show();
            }
        }

        private void DXWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();
        }
    }
}
