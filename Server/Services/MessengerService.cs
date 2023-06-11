using Grpc.Core;

namespace GrpcService.Services;

public class MessengerService : User.UserBase
{
    private class UserInfo
    {
        public string Name { get; }
        public string Password { get; }
        
        public UserInfo(string name, string password)
        {
            Name = name;
            Password = password;
        }
    }
    
    // Я не понял прикола с тем, что сервис пересоздаётся, так что затычку в виде статики пока сделаю
    private static readonly Dictionary<string, UserInfo> _users = new();
    private static readonly Dictionary<string, string> _usersFromWaitPassword = new();
    
    private const string _userNameId = "Name";
    
    private readonly ILogger<MessengerService> _logger;

    public MessengerService(ILogger<MessengerService> logger)
    {
        _logger = logger;
    }

    public override Task<AuthorizationReply> Authorization(AuthorizationRequest request, ServerCallContext context)
    {
        var userId = request.Name;
        
        var isNotValid = _usersFromWaitPassword.ContainsKey(userId) || _users.ContainsKey(userId);
        if (!isNotValid)
            _usersFromWaitPassword.Add(userId, request.Name);

        return Task.FromResult(new AuthorizationReply()
        {
            Id = isNotValid ? null : userId,
            IsValid = !isNotValid
        });
    }

    public override Task<CheckPasswordReply> SendPassword(CheckPasswordRequest request, ServerCallContext context)
    {
        bool isValid;
        if (_usersFromWaitPassword.TryGetValue(request.UserId, out var userName))
        {
            _usersFromWaitPassword.Remove(request.UserId);
            _users.Add(request.UserId, new UserInfo(userName, request.Password));
            isValid = true;
        }
        else
        {
            isValid = _users[request.UserId].Password == request.Password;
        }
        
        return Task.FromResult(new CheckPasswordReply()
        {
            IsValid = isValid
        });
    }

    public override async Task WaiterMessages(IAsyncStreamReader<MessageRequest> requestStream,
        IServerStreamWriter<MessageReply> responseStream, ServerCallContext context)
    {
        var readTask = Task.Run(async () =>
        {
            await foreach (var message in requestStream.ReadAllAsync())
            {
                await responseStream.WriteAsync(new MessageReply
                {
                    Text = message.Text,
                    UserName = _users[message.UserId].Name
                });
            }
        });
        
        await readTask;
    }
}