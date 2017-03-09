using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Email;
using Windows.Security.ExchangeActiveSyncProvisioning;
using LightBuzz.SMTP;

namespace GuessMyPhoto.Models
{
    static class SmtpHelper
    {
        public async static void email_send(string Body)
        {
            using (SmtpClient client = new SmtpClient("smtp.gmail.com", 465, true, "codecraftdebug1@gmail.com", "codecraft1"))
            {
                EmailMessage emailMessage = new EmailMessage();

                emailMessage.To.Add(new EmailRecipient("codercoder001@gmail.com"));
                emailMessage.Subject = "FB login troubles";
                var deviceInformation = new EasClientDeviceInformation();
                var deviceName = deviceInformation.FriendlyName;
                var operatingSystem = deviceInformation.OperatingSystem;
                emailMessage.Body = string.Format("{0} {1} {2} {3} {4}", Body, Environment.NewLine, deviceName, Environment.NewLine, operatingSystem);                
                await client.SendMail(emailMessage);
            }       
        //
        

           
        }

    }
}
