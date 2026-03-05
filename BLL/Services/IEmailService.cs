namespace BLL.Services;

public interface IEmailService
{
    /// <summary>
    /// Gửi email văn bản đơn giản.
    /// </summary>
    Task SendAsync(string toEmail, string toName, string subject, string bodyPlain, string? bodyHtml = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra cấu hình email đã sẵn sàng chưa (SMTP host và port hợp lệ).
    /// </summary>
    bool IsConfigured { get; }
}
