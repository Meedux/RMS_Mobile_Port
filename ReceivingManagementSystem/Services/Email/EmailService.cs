using ReceivingManagementSystem.Wrapper;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Serilog;

namespace ReceivingManagementSystem.Services.Email
{
    public class EmailService : IEmailService
    {
        private ISaveSettingsWrapper _pSaveSettingsWrapper;

        public EmailService()
        {
            _pSaveSettingsWrapper = DependencyService.Get<ISaveSettingsWrapper>();
        }

        public async Task<bool> SendEmail(string emailTo, string emailCC, string subject, string content)
        {
            try
            {
                string port = _pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Send_Port, null);
                string encryptionMethod = _pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Send_Encryption_Method, string.Empty);
                bool requiresAuthentication = _pSaveSettingsWrapper.GetBool(ReceivingManagementSystem.Common.Constants.Setting_Send_Requires_Authentication, false);
                string password = _pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Send_Password, string.Empty);
                string userName = _pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Send_User_Name, string.Empty);
                string server = _pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Send_Server, string.Empty);

                NetworkCredential networkCred = new NetworkCredential();
                networkCred.UserName = userName;
                networkCred.Password = password;

                SmtpClient smtpServer = new SmtpClient(server);
                smtpServer.UseDefaultCredentials = true;
                smtpServer.Credentials = networkCred;
                smtpServer.Port = int.Parse(port);
                smtpServer.EnableSsl = encryptionMethod.Equals("SSL");

                MailMessage mail = new MailMessage();
                MailAddress fromMail = new MailAddress(userName);
                mail.From = fromMail;

                foreach (var address in emailTo.Split(new[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries))
                {
                    mail.To.Add(address);
                }

                mail.Subject = subject;
                mail.Body = content;

                mail.IsBodyHtml = true;
                await smtpServer.SendMailAsync(mail);

                return true;
            }
            catch(Exception ex)
            {
                Log.Error(ex, ex.Message);
                return false;
            }
        }
    }
}
