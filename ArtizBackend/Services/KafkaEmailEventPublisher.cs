using System.Text.Json;
using BLL.Services;
using BO.DTOs;
using Confluent.Kafka;

namespace ArtizBackend.Services;

public class KafkaEmailEventPublisher : IEmailEventPublisher
{
    private readonly IProducer<string, string>? _producer;
    private readonly string _topic;
    private readonly ILogger<KafkaEmailEventPublisher> _logger;

    private record EmailEvent(string Type, object Payload);

    public KafkaEmailEventPublisher(IConfiguration configuration, ILogger<KafkaEmailEventPublisher> logger)
    {
        _logger = logger;
        var kafkaSection = configuration.GetSection("Kafka");
        var bootstrapServers = kafkaSection["BootstrapServers"];
        _topic = kafkaSection["EmailTopic"] ?? "artiz-email-events";

        if (!string.IsNullOrWhiteSpace(bootstrapServers))
        {
            var config = new ProducerConfig
            {
                BootstrapServers = bootstrapServers,
                Acks = Acks.All
            };
            _producer = new ProducerBuilder<string, string>(config).Build();
        }
        else
        {
            _logger.LogWarning("Kafka BootstrapServers is not configured. Email events will not be published to Kafka.");
        }
    }

    public async Task PublishUserRegisteredAsync(UserDto user, CancellationToken cancellationToken = default)
    {
        if (_producer == null) return;
        var evt = new EmailEvent("UserRegistered", new
        {
            user.Id,
            user.Email,
            user.Name
        });
        await PublishAsync(evt, cancellationToken);
    }

    public async Task PublishOrderCreatedAsync(int userId, OrderDto order, CancellationToken cancellationToken = default)
    {
        if (_producer == null) return;
        var evt = new EmailEvent("OrderCreated", new
        {
            UserId = userId,
            order.Id,
            order.OrderInvoiceNumber,
            order.TotalAmount,
            order.Status
        });
        await PublishAsync(evt, cancellationToken);
    }

    public async Task PublishPasswordResetRequestedAsync(string email, CancellationToken cancellationToken = default)
    {
        if (_producer == null) return;
        var evt = new EmailEvent("PasswordResetRequested", new
        {
            Email = email
        });
        await PublishAsync(evt, cancellationToken);
    }

    private async Task PublishAsync(EmailEvent evt, CancellationToken cancellationToken)
    {
        try
        {
            var json = JsonSerializer.Serialize(evt);
            await _producer!.ProduceAsync(_topic, new Message<string, string>
            {
                Key = evt.Type,
                Value = json
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish email event {Type} to Kafka.", evt.Type);
        }
    }
}

