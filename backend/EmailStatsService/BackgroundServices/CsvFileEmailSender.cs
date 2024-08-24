
using EmailStatsService.Data;
using EmailStatsService.DTO;
using EmailStatsService.GmailApi;
using EmailStatsService.Model;
using EmailStatsService.SyncDataServices.Smtp;

namespace EmailStatsService.BackgroundServices
{
    public class CsvFileEmailSender : IHostedService, IDisposable
    {
        private enum TimePeriodType {
            Weekly,
            Monthly
        }

        private Timer _timer;
        private readonly IServiceScopeFactory _scopeFactory;
        private DayOfWeek _currentDayOfWeek;
        private int _currentMonth;
        public CsvFileEmailSender(IServiceScopeFactory scopeFactory, IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;

            _currentDayOfWeek = DateTime.Today.DayOfWeek;
            _currentMonth = DateTime.Today.Month;
        }
        public void Dispose()
        {
            _timer?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(SendCsvEmails, null, TimeSpan.Zero, TimeSpan.FromHours(1));

            return Task.CompletedTask;
        }

        private void SendCsvEmails(object _)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepo>();
                var gmailApiService = scope.ServiceProvider.GetRequiredService<IGmailApiService>();
                
                DateTime rightNow = DateTime.Today;
                if (_currentDayOfWeek != rightNow.DayOfWeek)
                {
                    if (_currentDayOfWeek == DayOfWeek.Saturday && rightNow.DayOfWeek == DayOfWeek.Sunday)
                    {
                        foreach (var user in userRepo.GetAllUsers())
                        {
                            if (user.GetWeeklyCsv) 
                                SendCsvEmail(
                                    user, 
                                    gmailApiService
                                    .GetDetailedUserEmailStatistics(user, DateTime.Today.AddDays(-7), DateTime.Today)
                                    .Interactions,
                                    TimePeriodType.Weekly);
                        }
                    }
                    else _currentDayOfWeek = rightNow.DayOfWeek;
                }

                if (_currentMonth != rightNow.Month)
                {
                    _currentMonth = rightNow.Month;

                    foreach(var user in userRepo.GetAllUsers())
                    {
                        if (user.GetMonthlyCsv)
                            SendCsvEmail(
                                user, 
                                gmailApiService
                                .GetDetailedUserEmailStatistics(user, DateTime.Today.AddMonths(-1), DateTime.Today).Interactions,
                                TimePeriodType.Monthly);
                    }
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        private void SendCsvEmail(User user, Dictionary<string,InteractionInfoDTO> interactions, TimePeriodType timePeriod)
        {
            var csvContent = GetCsvContentWithInteractionInfo(interactions);
            Console.WriteLine("csv content: " + csvContent);

            using (var scope = _scopeFactory.CreateScope())
            {
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                if (timePeriod == TimePeriodType.Weekly)
                {
                    emailService.SendEmailWithCsvContent
                        ("Weekly Interactions Data", 
                        $"This email contains a .csv file attachment with information about {user.Email}'s interactions within the past week",
                        user.Email,
                        csvContent);
                }
                else
                {
                    Console.WriteLine("sending email to " + user.Email);
                    emailService.SendEmailWithCsvContent
                        ("Monthly Interactions Data", 
                        $"This email contains a .csv file attachment with information about {user.Email}'s interactions within the past month",
                        user.Email,
                        csvContent);
                }
            }
        }

        private string GetCsvContentWithInteractionInfo(Dictionary<string,InteractionInfoDTO> interactions)
        {
            var csvContent = "Email,Total Interactions,SentCount,ReceiveCount,BestContactTime,ResponseTime\n";
            foreach (var kv in interactions)
            {
                var email = kv.Key;
                var interaction = kv.Value;

                csvContent += email + ",";
                csvContent += interaction.TotalInteractions.ToString() + ",";
                csvContent += interaction.SentCount.ToString() + ",";
                csvContent += interaction.ReceiveCount.ToString() + ",";
                csvContent += interaction.BestContactTime.ToString() + ",";
                csvContent += (interaction.ResponseTime == -1 ? "NA":interaction.ResponseTime.ToString()) + "\n";
            }

            return csvContent;
        }
    }
}