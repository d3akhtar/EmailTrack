using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using System.Text.Json;
using System.Reflection;
using EmailStatsService.File;

namespace EmailSender
{
    public class Services
    {
        private readonly IConfiguration _configuration;

        public Services(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SendEmail(string subject, string body, string receiverEmail, string csvContent = "")
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration["EmailInfo:SenderEmail"]));
            email.To.Add(MailboxAddress.Parse(receiverEmail));
            email.Subject = subject;

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = body;

            if (!string.IsNullOrEmpty(csvContent))
            {
                using (var tempFile = new TempFile(Path.Combine(Path.GetFullPath("./"), "data.csv")))
                {
                    using (var file = new StreamWriter(tempFile.Path, true))
                    {
                        file.Write(csvContent);
                    }
                    bodyBuilder.Attachments.Add(tempFile.Path);
                }
            }
            email.Body = bodyBuilder.ToMessageBody();

            Console.WriteLine("--> Creating client");

            using var smtp = new SmtpClient();
            smtp.Connect(_configuration["EmailInfo:EmailHost"], 587, SecureSocketOptions.StartTls);
            smtp.Authenticate(_configuration["EmailInfo:SenderEmail"], _configuration["EmailInfo:EmailPassword"]);
            smtp.Send(email);
            smtp.Disconnect(true);
        }

        public void HandleClient(Socket socket)
        {
            Console.WriteLine("--> Handling Client");

            byte[] bytes = new byte[65536];
            socket.Receive(bytes);

            Console.WriteLine("--> Received request");

            string request = Encoding.ASCII.GetString(bytes);

            // Handle email send request
            var emailRequestContents = request.Split("|||");

            string secret = emailRequestContents[0].Replace("Secret: ","");

            if (secret == _configuration["EmailInfo:Secret"])
            {
                try {
                    Console.WriteLine("--> Sending email");

                    string subject = emailRequestContents[1].Replace("Subject: ","");
                    string body = emailRequestContents[2].Replace("Body: ", "");
                    string receiverEmail = emailRequestContents[3].Replace("ReceiverEmail: ", "");

                    if (emailRequestContents.Length == 5){
                        string csvContent =  emailRequestContents[4].Replace("CsvContent: ", "").TrimEnd('\0');
                        SendEmail(subject, body, receiverEmail, csvContent);
                    }
                    else{
                        SendEmail(subject, body, receiverEmail.TrimEnd('\0'));
                    }
                }
                catch(Exception ex){
                    Console.WriteLine("--> Error while sending email: " + ex.Message);
                }
            }
        }
    }
}