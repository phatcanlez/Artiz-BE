using System.Threading.Channels;
using BLL.Services;

namespace ArtizBackend.Services;

public class InMemoryEmailQueue : IEmailQueue
{
    private readonly Channel<EmailMessage> _channel;

    public InMemoryEmailQueue()
    {
        _channel = Channel.CreateUnbounded<EmailMessage>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
    }

    public void Enqueue(EmailMessage message)
    {
        if (!_channel.Writer.TryWrite(message))
        {
            throw new InvalidOperationException("Không thể enqueue email vào hàng đợi.");
        }
    }

    public async ValueTask<EmailMessage> DequeueAsync(CancellationToken cancellationToken)
    {
        var msg = await _channel.Reader.ReadAsync(cancellationToken);
        return msg;
    }
}

