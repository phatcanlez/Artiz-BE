namespace BLL.Services;

public record EmailMessage(
    string ToEmail,
    string ToName,
    string Subject,
    string BodyPlain,
    string? BodyHtml);

/// <summary>
/// Hàng đợi email nội bộ, dùng cho background worker gửi mail không chặn request.
/// </summary>
public interface IEmailQueue
{
    void Enqueue(EmailMessage message);
    ValueTask<EmailMessage> DequeueAsync(CancellationToken cancellationToken);
}

