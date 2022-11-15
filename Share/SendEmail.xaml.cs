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
    public partial class SendEmail : DXWindow
    {
        public SendEmail(string textToShare)
        {
            InitializeComponent();

            txtMessageText.Text = textToShare;

            if (Properties.Settings.Default.ThemeIndex > 0 && Properties.Settings.Default.ThemeIndex <= Consts.DevExTheme.Count)
                ThemeManager.SetTheme(this, Consts.DevExTheme[Properties.Settings.Default.ThemeIndex]);
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(txtToWhom.Text))
                MessageBox.Show("Поле \"Кому\" осталось незаполненным. Невозможно отправить e-mail", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                string error = String.Empty;
                this.Cursor = Cursors.Wait;

                Email.SendMail("smtp.mail.ru", 25,
                               "budget.ultrazoom@mail.ru", "!QAZ1qaz",
                               "budget.ultrazoom@mail.ru", txtToWhom.Text,
                               txtMessageText.Text, txtSubject.Text,
                               true, out error);
                Email.SendMail("smtp.mail.ru", 25,
                               "budget.ultrazoom@mail.ru", "!QAZ1qaz",
                               "budget.ultrazoom@mail.ru", "budget.ultrazoom@mail.ru",
                               txtMessageText.Text, txtSubject.Text,
                               true, out error);

                this.Cursor = Cursors.Arrow;

                if (!String.IsNullOrEmpty(error))
                    MessageBox.Show(error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                {
                    this.Close();
                }
            }
        }
    }
}
