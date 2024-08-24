
using EmailStatsService.Data;
using EmailStatsService.GmailApi;
using EmailStatsService.GoogleAuth;

namespace EmailStatsService.BackgroundServices
{
    public class EmailRefresher : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly IServiceScopeFactory _scopeFactory;
        private DateTime _currentDate;
        public EmailRefresher(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;

            _currentDate = DateTime.Today;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(RefreshUserEmails, null, TimeSpan.Zero, TimeSpan.FromHours(1));

            return Task.CompletedTask;
        }

        private async void RefreshUserEmails(object _)
        {
            if (_currentDate.DayOfYear != DateTime.Today.DayOfYear)
            {
                _currentDate = DateTime.Today;

                using (var scope = _scopeFactory.CreateScope())
                {
                    var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepo>();
                    var gmailApiService = scope.ServiceProvider.GetRequiredService<IGmailApiService>();
                    var googleAuth = scope.ServiceProvider.GetRequiredService<IGoogleAuth>();

                    try{
                        foreach (var user in userRepo.GetAllUsers().ToList())
                        {
                            if (user.AccessTokenExpired())
                            {
                                var authParams = await googleAuth.GetNewAccessTokenUsingRefreshToken(user.RefreshToken);
                                userRepo.RefreshUserAccessToken(user, authParams);
                                userRepo.SaveChanges();
                            }

                            var removeDate = _currentDate.AddMonths(-2);
                            var getDate = _currentDate;

                            await gmailApiService.RefreshUserGmails(user, removeDate, getDate);
                        }
                    }
                    catch(Exception ex){
                        Console.WriteLine("Error occured while attempting to refresh user gmails: " + ex.Message);
                    }
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}