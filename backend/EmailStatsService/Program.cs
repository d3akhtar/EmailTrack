using EmailStatsService.BackgroundServices;
using EmailStatsService.Data;
using EmailStatsService.GmailApi;
using EmailStatsService.GoogleAuth;
using EmailStatsService.SyncDataServices.Smtp;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

if (builder.Environment.IsDevelopment())
{
    Console.WriteLine("--> Using in memory database...");
    builder.Services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase("emailtrack"));
}
else if(builder.Environment.IsProduction())
{
    Console.WriteLine("--> Using MySQL");
    builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("Default")!));
}

builder.Services.AddScoped<IGoogleAuth, GoogleAuth>();
builder.Services.AddScoped<IUserRepo, UserRepo>();
builder.Services.AddScoped<ITeamRepo, TeamRepo>();
builder.Services.AddScoped<IGmailRepo, GmailRepo>();
builder.Services.AddScoped<IGmailApiService,GmailApiService>();
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddHostedService<EmailRefresher>();
builder.Services.AddHostedService<CsvFileEmailSender>();


var app = builder.Build();

// Configure app
app.UseRouting();
app.UseHttpsRedirection();
app.UseCors(o => o.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod().WithExposedHeaders("*"));
app.MapControllers();
app.UseSession();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else if (app.Environment.IsProduction())
{
    using (var scope = app.Services.CreateScope()){
        var db = scope.ServiceProvider.GetService<AppDbContext>();
        db!.Database.Migrate();
    }
}

app.Run();
