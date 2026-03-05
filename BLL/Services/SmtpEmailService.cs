using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace BLL.Services;

public class SmtpEmailService : IEmailService
{
    private readonly string _host;
    private readonly int _port;
    private readonly bool _enableSsl;
    private readonly string? _userName;
    private readonly string? _password;
    private readonly string _fromDisplayName;
    private readonly string _fromAddress;

    public SmtpEmailService(IConfiguration configuration)
    {
        var section = configuration.GetSection("Email");
        _host = section["SmtpHost"] ?? "";
        _port = int.TryParse(section["SmtpPort"], out var p) ? p : 587;
        _enableSsl = string.Equals(section["EnableSsl"], "true", StringComparison.OrdinalIgnoreCase);
        _userName = section["UserName"];
        _password = section["Password"];
        _fromDisplayName = section["FromDisplayName"] ?? "Artiz";
        _fromAddress = section["FromAddress"] ?? _userName ?? "noreply@artiz.com";
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_host) && _port > 0;

    public async Task SendAsync(string toEmail, string toName, string subject, string bodyPlain, string? bodyHtml = null, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
            throw new InvalidOperationException("Email is not configured. Set Email:SmtpHost and Email:SmtpPort in appsettings.");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_fromDisplayName, _fromAddress));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;

        var builder = new BodyBuilder { TextBody = bodyPlain };
        if (!string.IsNullOrWhiteSpace(bodyHtml))
            builder.HtmlBody = bodyHtml;
        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        var secureSocketOptions = _enableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
        await client.ConnectAsync(_host, _port, secureSocketOptions, cancellationToken);

        if (!string.IsNullOrWhiteSpace(_userName) && !string.IsNullOrWhiteSpace(_password))
            await client.AuthenticateAsync(_userName, _password, cancellationToken);

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
