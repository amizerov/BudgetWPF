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
    public partial class InfoWindow : DXWindow
    {
        private Queue<Hint> _hints;

        public InfoWindow()
        {
            InitializeComponent();
            _hints = LoadAllHints();

            if (Properties.Settings.Default.ThemeIndex > 0 && Properties.Settings.Default.ThemeIndex <= Consts.DevExTheme.Count)
                ThemeManager.SetTheme(this, Consts.DevExTheme[Properties.Settings.Default.ThemeIndex]);

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.ShowHints)
                checkBoxDontShow.IsChecked = true;
        }

        /// <summary>
        /// Далее
        /// </summary>
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            LoadNext();
        }

        /// <summary>
        /// Показать следующую подсказку
        /// </summary>
        public void LoadNext()
        {
            var nowHint = _hints.Dequeue();
            _hints.Enqueue(nowHint);

            LoadHint(nowHint.ID);
        }

        /// <summary>
        /// Показать подсказку по алиасу
        /// </summary>
        /// <param name="alias"></param>
        public void LoadHint(string alias)
        {
            if (_hints.Count > 0)
            {
                var nowHint = _hints.FirstOrDefault(h => h.Alias == alias);
                if (nowHint != null)
                {
                    RewindHints(nowHint);
                    
                    lblHeader.Content = nowHint.Header;
                    txtContent.Text = nowHint.Content;

                }
            }
        }

        /// <summary>
        /// Показать подсказку по id
        /// </summary>
        /// <param name="alias"></param>
        private void LoadHint(int id)
        {
            if (_hints.Count > 0)
            {
                var nowHint = _hints.FirstOrDefault(h => h.ID == id);
                if (nowHint != null)
                {
                    RewindHints(nowHint);

                    lblHeader.Content = nowHint.Header;
                    txtContent.Text = nowHint.Content;
                }
            }
        }

        /// <summary>
        /// Перемотать очередь подсказок до определенной подсказки
        /// </summary>
        private void RewindHints(Hint hint)
        {
            var tempHint = new Hint();

            while (!tempHint.Equals(hint))
            {
                tempHint = _hints.Dequeue();
                _hints.Enqueue(tempHint);
            }
        }

        /// <summary>
        /// Загрузить подсказки
        /// </summary>
        private Queue<Hint> LoadAllHints()
        {
            var temp = new Queue<Hint>();

            var dt = G.db_select("exec GetAllHints");
            CheckDB(G.LastError);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                temp.Enqueue(new Hint(dt.Rows[i]["ID"].ToString(),
                                      dt.Rows[i]["Header"].ToString(),
                                      dt.Rows[i]["Content"].ToString(),
                                      dt.Rows[i]["Alias"].ToString()
                                      )
                            );
            }

            return temp;
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

        /// <summary>
        /// Больше не показывать подсказки
        /// </summary>
        private void checkBoxDontShow_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ShowHints = false;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Показывать подсказки
        /// </summary>
        private void checkBoxDontShow_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ShowHints = true;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Закрыть
        /// </summary>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}
