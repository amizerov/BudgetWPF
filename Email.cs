using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;

namespace Budget
{
    class Email
    {
        public static int SendMail(string host, int port, string userName, string pswd, string fromAddress, string toAddress, string body, string subject, bool sslEnabled, out string error)
        {
            error = "";

            MailMessage msg = new MailMessage(new MailAddress(fromAddress), new MailAddress(toAddress));

            msg.Subject = subject;

            msg.SubjectEncoding = System.Text.Encoding.UTF8;
            msg.Body = body;

            msg.BodyEncoding = System.Text.Encoding.UTF8;
            msg.IsBodyHtml = false;
            SmtpClient client = new SmtpClient(host, port);

            client.Credentials = new System.Net.NetworkCredential(userName, pswd);

            client.EnableSsl = sslEnabled;
            try
            {
                client.Send(msg);

                error = "";
                return 0;
            }
            catch (SmtpException ex)
            {
                error = String.Format("There was an Smtp error while sending your message. {0}", ex.ToString());
                return 1;
            }
            catch (Exception ex)
            {
                error = String.Format("There was an error while sending your message. {0}", ex.ToString());
                return 1;
            }
        }
    }
}
