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
using System.ComponentModel;
using am.BL;
using System.Data;

namespace Budget
{
    public partial class AddComment : DXWindow
    {
        private int _userID;

        public AddComment(int userID)
        {
            InitializeComponent();
            _userID = userID;

            FillCommentTypes();
            if (Properties.Settings.Default.ThemeIndex > 0 && Properties.Settings.Default.ThemeIndex <= Consts.DevExTheme.Count)
                ThemeManager.SetTheme(this, Consts.DevExTheme[Properties.Settings.Default.ThemeIndex]);
        }

        /// <summary>
        /// Загрузка типов комментариев
        /// </summary>
        private void FillCommentTypes()
        {
            var dt = G.db_select("exec GetCommentTypes");
            CheckDB(G.LastError);
            commentTypesComboBox.ItemsSource = ((IListSource)dt).GetList();
            commentTypesComboBox.ValueMember = "ID";
            commentTypesComboBox.DisplayMember = "Type";
            if (commentTypesComboBox.ItemsSource != null)
                commentTypesComboBox.SelectedIndex = 0;
        }

        private void commentTypesComboBox_SelectedIndexChanged(object sender, RoutedEventArgs e)
        {
            Uri uri = null;

            var selType = (commentTypesComboBox.SelectedItem as DataRowView).Row.ItemArray[1].ToString();
            if (selType == "Комментарий")
                uri = new Uri("pack://application:,,,/Budget;component/images/comment_big.png");
            else if (selType == "Вопрос")
                uri = new Uri("pack://application:,,,/Budget;component/images/question.png");
            else if (selType == "Предложение")
                uri = new Uri("pack://application:,,,/Budget;component/images/offer.png");
            else if (selType == "Информация об ошибке")
                uri = new Uri("pack://application:,,,/Budget;component/images/error.png");

            if (uri != null)
                imgComment.Source = new BitmapImage(uri);
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(txtComment.Text))
            {
                var typeID = -1;
                try { typeID = Convert.ToInt32(commentTypesComboBox.EditValue); } catch {}

                Database.AddComment(typeID, txtComment.Text, _userID);
                CheckDB(G.LastError);
                if (String.IsNullOrEmpty(G.LastError))
                {
                    this.Close();
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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
    }
}
