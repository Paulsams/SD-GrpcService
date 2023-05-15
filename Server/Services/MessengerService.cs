using Grpc.Core;

namespace GrpcService.Services;

public class MessengerService : User.UserBase
{
    private readonly ILogger<MessengerService> _logger;

    public MessengerService(ILogger<MessengerService> logger)
    {
        _logger = logger;
    }

    public override Task<MessageReply> SendMessage(MessageRequest request, ServerCallContext context)
    {
        return Task.FromResult(new MessageReply
        {
            Text = request.Text
        });
    }
}