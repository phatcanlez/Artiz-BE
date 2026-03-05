using BLL.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ArtizBackend.Services;

public class EmailBackgroundService : BackgroundService
{
    private readonly IEmailQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EmailBackgroundService> _logger;

    public EmailBackgroundService(
        IEmailQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<EmailBackgroundService> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email background service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            EmailMessage message;
            try
            {
                message = await _queue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while dequeuing email message.");
                continue;
            }

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                if (emailService.IsConfigured)
                {
                    await emailService.SendAsync(
                        message.ToEmail,
                        message.ToName,
                        message.Subject,
                        message.BodyPlain,
                        message.BodyHtml,
                        stoppingToken);
                }
                else
                {
                    _logger.LogWarning("Email service is not configured. Skipping email to {Email}.", message.ToEmail);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Email}", message.ToEmail);
            }
        }

        _logger.LogInformation("Email background service stopped.");
    }
}

