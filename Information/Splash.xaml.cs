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

namespace Budget
{
    public partial class Splash : Window
    {
        public void SetText(string msg) 
        {
            lblContent.Content = msg; 
        }
        public Splash(string content)
        {
            InitializeComponent();

            //Top -= 150;
            lblContent.Content = content;

            if (Properties.Settings.Default.ThemeIndex > 0 && Properties.Settings.Default.ThemeIndex <= Consts.DevExTheme.Count)
                ThemeManager.SetTheme(this, Consts.DevExTheme[Properties.Settings.Default.ThemeIndex]);
        }
    }
}
