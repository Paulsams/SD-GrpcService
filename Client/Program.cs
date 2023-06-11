using Grpc.Core;
using Grpc.Net.Client;
using GrpcService;

Console.WriteLine("Hello! \"Messenger\" welcomes you!");

Console.Write("Enter your port: ");
var port = Console.ReadLine();

using var channel = GrpcChannel.ForAddress($"http://localhost:{port}");
var client = new User.UserClient(channel);

var authorizationReply = await Authorization();
var waiterMessagesCall = client.WaiterMessages();

var waiterMessages = Task.Run(async () =>
{
    await foreach (var response in waiterMessagesCall.ResponseStream.ReadAllAsync())
        Console.WriteLine($"[{DateTime.Now}] <{response.UserName}>: {response.Text}");
});

await Task.Run(async () =>
{
    while (true)
    {
        var message = Console.ReadLine();
        await waiterMessagesCall.RequestStream.WriteAsync(new MessageRequest()
        {
            UserId = authorizationReply.Id,
            Text = message
        });
    }
});

async Task<AuthorizationReply> Authorization() {
    while (true)
    {
        Console.Write("Enter your name: ");
        var clientName = Console.ReadLine();

        if (clientName == string.Empty)
        {
            Console.Error.WriteLine("The name cannot be empty");
            continue;
        }
    
        var reply = await client.AuthorizationAsync(new AuthorizationRequest()
        {
            Name = clientName
        });

        if (!reply.IsValid)
        {
            Console.Error.WriteLine("This name is already taken by another user");
            continue;
        }

        Console.Write("Enter your password: ");
        var password = Console.ReadLine();
        var passwordReply = await client.SendPasswordAsync(new CheckPasswordRequest()
        {
            UserId = reply.Id,
            // Давайте считать, что раскрытие паролей не важно). :sunglasses:
            Password = password
        });
        
        if (passwordReply.IsValid)
            return reply;
        
        Console.Error.WriteLine("The password is incorrect");
    }
}

