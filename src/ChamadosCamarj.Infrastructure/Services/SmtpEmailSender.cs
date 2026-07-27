using MailKit.Net.Smtp;
using MimeKit;
using ChamadosCamarj.Application.Common;

namespace ChamadosCamarj.Infrastructure.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly string _remetente;
    private readonly string _senha;
    private readonly string _host;
    private readonly int _porta;

    public SmtpEmailSender(string remetente, string senha, string host = "smtp.gmail.com", int porta = 587)
    {
        _remetente = remetente;
        _senha = senha;
        _host = host;
        _porta = porta;
    }

    public async Task EnviarAsync(string para, string assunto, string corpoHtml)
    {
        using var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Chamados CAMARJ", _remetente));
        message.To.Add(MailboxAddress.Parse(para));
        message.Subject = assunto;
        message.Body = new TextPart("html") { Text = corpoHtml };

        using var client = new SmtpClient();
        await client.ConnectAsync(_host, _porta, MailKit.Security.SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_remetente, _senha);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
