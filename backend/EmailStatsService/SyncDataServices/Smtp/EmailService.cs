using System.Net.Mail;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace EmailStatsService.SyncDataServices.Smtp
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _conf;

        public EmailService(IConfiguration conf)
        {
            _conf = conf;
        }

        public void SendEmail(string subject, string body, string receiverEmail)
        {
            try{
                var client = new TcpClient(_conf["EmailInfo:Ip"], int.Parse(_conf["EmailInfo:Port"]));
                var stream = client.GetStream();

                string message = $"Secret: {_conf["EmailInfo:Secret"]}|||Subject: {subject}|||Body: {body}|||ReceiverEmail: {receiverEmail}";
                int byteCount = Encoding.ASCII.GetByteCount(message);
                byte[] sendData = new byte[byteCount];
                sendData = Encoding.ASCII.GetBytes(message);
                
                stream.Write(sendData);

                stream.Close();
                client.Close();
            }
            catch(Exception ex){
                Console.WriteLine(ex.ToString());
            }
        }

        public void SendEmailWithCsvContent(string subject, string body, string receiverEmail, string csvContent)
        {
            try{
                var client = new TcpClient(_conf["EmailInfo:Ip"], int.Parse(_conf["EmailInfo:Port"]));
                var stream = client.GetStream();

                string message = $"Secret: {_conf["EmailInfo:Secret"]}|||Subject: {subject}|||Body: {body}|||ReceiverEmail: {receiverEmail}|||CsvContent: {csvContent}";
                int byteCount = Encoding.ASCII.GetByteCount(message);
                byte[] sendData = new byte[byteCount];
                sendData = Encoding.ASCII.GetBytes(message);
                
                stream.Write(sendData);

                stream.Close();
                client.Close();
            }
            catch(Exception ex){
                Console.WriteLine(ex.ToString());
            }
        }
    }
}