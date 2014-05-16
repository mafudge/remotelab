using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Net.Mail;
using System.Text;

namespace RemoteLab.Utilities
{
    public class SmtpEmail
    {
        public async Task SendMailAsync(String SmtpHost, String FromEmail, String ToEmail, String Subject, String Body)
        {
            using(var Msg = new MailMessage() {
                From = new MailAddress(FromEmail),
                Subject = Subject,
                Body = Body,
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = false 
            })
            {
            
                Msg.To.Add(new MailAddress(ToEmail));

                using(var Smtp = new SmtpClient(SmtpHost))
                {
                    await Smtp.SendMailAsync(Msg);
                }                
            }
        }

    }
}