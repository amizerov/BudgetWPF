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
using System.Configuration;
using DevExpress.Xpf.Core;
using Facebook;

namespace Budget
{
    public partial class FacebookLogin : DXWindow
    {
        private string _textToShare;

        public FacebookLogin(string textToShare)
        {
            InitializeComponent();

            _textToShare = textToShare;

            if (Properties.Settings.Default.ThemeIndex > 0 && Properties.Settings.Default.ThemeIndex <= Consts.DevExTheme.Count)
                ThemeManager.SetTheme(this, Consts.DevExTheme[Properties.Settings.Default.ThemeIndex]);

            loginWebBrowser.Navigate("https://graph.facebook.com/oauth/authorize" +
                                     "?client_id=" + Properties.Settings.Default.FacebookAppID +
                                     "&redirect_uri=http://www.facebook.com/connect/login_success.html" +
                                     "&display=popup&type=user_agent&scope=publish_stream");
        }

        private void loginWebBrowser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (loginWebBrowser.Source.ToString().Contains("access_token"))
            {
                string token = String.Empty;
                string URL = loginWebBrowser.Source.ToString();
                token = URL.Substring(URL.IndexOf("access_token") + 13);
                token = token.Substring(0, token.IndexOf("&"));

                var client = new FacebookClient(token);

                string me = client.Get("me").ToString();
                int stIndex = me.IndexOf("\"id\":") + "\"id\":".Length + 1;
                int finIndex = stIndex;
                for (int i = stIndex; i < me.Length; i++)
                {
                    if (me[i] == '"')
                    {
                        finIndex = i;
                        break;
                    }
                }
                string myID = me.Substring(stIndex, finIndex - stIndex);

                var parameters = new Dictionary<string, object>();
                parameters["message"] = _textToShare;
                client.PostAsync(String.Format("{0}/feed", myID), parameters);

                this.Close();
            }
        }
    }
}
