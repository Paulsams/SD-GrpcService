using Grpc.Net.Client;
using GrpcService;

Console.Write("Enter your name: ");
var clientName = Console.ReadLine();

Console.Write("Enter your port: ");
var port = Console.ReadLine();

using var channel = GrpcChannel.ForAddress($"http://localhost:{port}");
var client = new User.UserClient(channel);

while (true)
{
    Console.Write("Enter message: ");
    var message = Console.ReadLine();
    
    var reply = await client.SendMessageAsync(new MessageRequest() { Text = message });
    Console.WriteLine($"[{DateTime.Now}] <{clientName}>: {reply.Text}");
}
