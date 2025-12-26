using HadiyahServices.Interfaces;
using HadiyahServices.Options;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

public class EmailSender :IEmailSender
{
    private readonly EmailSettings _settings;


    public EmailSender(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }

    public async Task SendAsync(string to, string subject, string body)
    {
        using var client = new SmtpClient(_settings.Host ?? "smtp.gmail.com",
                                          _settings.Port == 0 ? 587 : _settings.Port)
        {
            EnableSsl = true,                 // STARTTLS on 587; TLS on connect for 465 usually works too
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(_settings.Sender, _settings.Password)
        };

        using var message = new MailMessage
        {
            From = new MailAddress(_settings.Sender,
                string.IsNullOrWhiteSpace(_settings.DisplayName) ? _settings.Sender : _settings.DisplayName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        message.To.Add(to);

        await client.SendMailAsync(message);
    }
}

