using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Budget
{
    public static class Transport
    {
        public static bool SetImage(int userID, int operationID, string fileName, byte[] imageBytes)
        {
            try
            {
                var baseUrl = Properties.Settings.Default.BudgetServiceFilesUrl;
                string url = String.Format("{0}/SetImage?FileName={1}&UserID={2}&OperationID={3}",
                                           baseUrl,
                                           fileName,
                                           userID,
                                           operationID);
                var uri = new Uri(url);

                var request = (HttpWebRequest) WebRequest.Create(uri);
                request.ContentType = "application/octet-stream";
                request.Method = WebRequestMethods.Http.Post;

                using (var requestStream = request.GetRequestStream())
                    requestStream.Write(imageBytes, 0, imageBytes.Length);

                using (var response = request.GetResponse())
                {
                    var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                    var res = reader.ReadToEnd();

                    if (res == "FAIL")
                        return false;
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static Image GetImage(int userID, int operationID, out string fileName)
        {
            fileName = String.Empty;
            Image resImg = null;

            var baseUrl = Properties.Settings.Default.BudgetServiceFilesUrl;
            string url = String.Format("{0}/GetImage/{1}/{2}",
                                           baseUrl,
                                           userID,
                                           operationID);

            try
            {
                var httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
                var httpWebReponse = (HttpWebResponse)httpWebRequest.GetResponse();
                var stream = httpWebReponse.GetResponseStream();
                fileName = httpWebReponse.Headers["Content-Disposition"];
                var indFileName = fileName.IndexOf("filename=")+"filename=".Length;
                fileName = fileName.Substring(indFileName, fileName.Length-indFileName);

                resImg = Image.FromStream(stream);
            }
            catch
            {
            }

            return resImg;
        }

        public static void DelImage(int userID, int operationID)
        {
            var baseUrl = Properties.Settings.Default.BudgetServiceFilesUrl;
            string url = String.Format("{0}/DelImage/{1}/{2}",
                                           baseUrl,
                                           userID,
                                           operationID);

            try
            {
                var httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
                var httpWebReponse = (HttpWebResponse)httpWebRequest.GetResponse();
                var stream = httpWebReponse.GetResponseStream();
            }
            catch
            {
            }
        }
    }
}
