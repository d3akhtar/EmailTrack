using System.Net;
using System.Net.Sockets;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;
using EmailSender;

// configuration to let us use appsettings.json
var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory()) // start looking for these files here
    .AddJsonFile($"appsettings.json", optional: false, reloadOnChange: true);

IConfiguration _configuration = builder.Build();

Services services = new Services(_configuration);

TcpListener server = new TcpListener(IPAddress.Any, 4222);

Console.WriteLine("--> Starting server");

server.Start();

while (true){
    Socket socket = server.AcceptSocket(); // wait for client
    await Task.Run(() => {
        services.HandleClient(socket);
    });
}