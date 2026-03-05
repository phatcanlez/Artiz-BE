using BO.DTOs;

namespace BLL.Services;

/// <summary>
/// Publish email-related domain events (for Kafka or other buses).
/// </summary>
public interface IEmailEventPublisher
{
    Task PublishUserRegisteredAsync(UserDto user, CancellationToken cancellationToken = default);
    Task PublishOrderCreatedAsync(int userId, OrderDto order, CancellationToken cancellationToken = default);
    Task PublishPasswordResetRequestedAsync(string email, CancellationToken cancellationToken = default);
}

