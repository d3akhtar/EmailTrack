using System.Net.Mail;

namespace EmailStatsService.SyncDataServices.Smtp
{
    public interface IEmailService
    {
        void SendEmail(string subject, string body, string receiverEmail);
        void SendEmailWithCsvContent(string subject, string body, string receiverEmail, string csvContent);
    }
}