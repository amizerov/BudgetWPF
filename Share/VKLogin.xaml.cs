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
using System.Text.RegularExpressions;
using System.Xml;
using System.Collections.Specialized;
using DevExpress.Xpf.Core;

namespace Budget
{
    public partial class VKLogin : DXWindow
    {
        private string _textToShare;
        private string _accessToken;

        public VKLogin(string textToShare)
        {
            InitializeComponent();

            _textToShare = textToShare;
            
            _accessToken = String.Empty;

            if (Properties.Settings.Default.ThemeIndex > 0 && Properties.Settings.Default.ThemeIndex <= Consts.DevExTheme.Count)
                ThemeManager.SetTheme(this, Consts.DevExTheme[Properties.Settings.Default.ThemeIndex]);

            int scope = (int)(VkontakteScopeList.wall);
            loginVKWebBrowser.Navigate(String.Format("http://api.vkontakte.ru/oauth/authorize?client_id={0}&scope={1}&display=popup&response_type=token", Properties.Settings.Default.VKAppID, scope));
        }

        private void loginVKWebBrowser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.Uri.ToString().IndexOf("access_token") != -1)
            {
                string accessToken = "";
                int userId = 0;
                Regex myReg = new Regex(@"(?<name>[\w\d\x5f]+)=(?<value>[^\x26\s]+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                foreach (Match m in myReg.Matches(e.Uri.ToString()))
                {
                    if (m.Groups["name"].Value == "access_token")
                        accessToken = m.Groups["value"].Value;
                    else if (m.Groups["name"].Value == "user_id")
                        userId = Convert.ToInt32(m.Groups["value"].Value);
                }

                _accessToken = accessToken;

                WallPost(userId);  //пост на стену
                this.Close();
            }
        }

        /// <summary>
        /// Пост на стену ВКонтакте
        /// </summary>
        private void WallPost(int id)
        {
            NameValueCollection qs = new NameValueCollection();
            qs["owner_id"] = id.ToString();
            qs["message"] = _textToShare;
            ExecuteCommand("wall.post", qs);
        }

        private XmlDocument ExecuteCommand(string name, NameValueCollection qs)
        {
            XmlDocument result = new XmlDocument();
            result.Load(String.Format("https://api.vkontakte.ru/method/{0}.xml?access_token={1}&{2}", name, _accessToken, String.Join("&", from item in qs.AllKeys select item + "=" + qs[item])));
            return result;
        }

        private enum VkontakteScopeList
        {
            /// <summary>
            /// Пользователь разрешил отправлять ему уведомления. 
            /// </summary>
            notify = 1,
            /// <summary>
            /// Доступ к друзьям.
            /// </summary>
            friends = 2,
            /// <summary>
            /// Доступ к фотографиям. 
            /// </summary>
            photos = 4,
            /// <summary>
            /// Доступ к аудиозаписям. 
            /// </summary>
            audio = 8,
            /// <summary>
            /// Доступ к видеозаписям. 
            /// </summary>
            video = 16,
            /// <summary>
            /// Доступ к предложениям (устаревшие методы). 
            /// </summary>
            offers = 32,
            /// <summary>
            /// Доступ к вопросам (устаревшие методы). 
            /// </summary>
            questions = 64,
            /// <summary>
            /// Доступ к wiki-страницам. 
            /// </summary>
            pages = 128,
            /// <summary>
            /// Добавление ссылки на приложение в меню слева.
            /// </summary>
            link = 256,
            /// <summary>
            /// Доступ заметкам пользователя. 
            /// </summary>
            notes = 2048,
            /// <summary>
            /// (для Standalone-приложений) Доступ к расширенным методам работы с сообщениями. 
            /// </summary>
            messages = 4096,
            /// <summary>
            /// Доступ к обычным и расширенным методам работы со стеной. 
            /// </summary>
            wall = 8192,
            /// <summary>
            /// Доступ к документам пользователя.
            /// </summary>
            docs = 131072
        }
    }
}
