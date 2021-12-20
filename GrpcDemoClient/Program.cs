using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;

namespace GrpcDemoClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Which rpc call would you like to make?");
            Console.WriteLine("1. SayHello");
            Console.WriteLine("2. ParrotSaysHello");
            Console.WriteLine("3. ChattyClientSaysHello");
            Console.WriteLine("4. InteractingHello");

            Console.Write("Enter a number: ");
            char number = Console.ReadKey().KeyChar;
            Console.WriteLine();

            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new Greeter.GreeterClient(channel);

            switch (number)
            {
                case '1':
                    var reply = await client.SayHelloAsync(new HelloRequest() { Name = "YouTube", Greeting = "Bonjour" });
                    Console.WriteLine(reply.Message);
                    break;
                case '2':
                    using (var call = client.ParrotSaysHello(new HelloRequest() { Greeting = "Hello", Name = "YouTube"}))
                    {
                        int numHellos = 0;
                        while (await call.ResponseStream.MoveNext())
                        {
                            HelloReply currentReply = call.ResponseStream.Current;
                            Console.WriteLine(currentReply.Message);
                            numHellos++;

                            if (numHellos >= 5)
                            {
                                call.Dispose();
                                break;
                            }
                        }
                    }
                    break;
                case '3':
                    using (var call = client.ChattyClientSaysHello())
                    {
                        Console.WriteLine("Enter names to greet, enter nothing to stop");
                        string input;
                        while ((input = Console.ReadLine()) != "")
                        {
                            await call.RequestStream.WriteAsync(new HelloRequest() {Greeting = "Hiya", Name = input});
                        }

                        await call.RequestStream.CompleteAsync();

                        DelayedReply summary = await call.ResponseAsync;
                        Console.WriteLine(summary.Message);
                        foreach (var helloRequest in summary.Request)
                        {
                            Console.WriteLine($"Greeting: {helloRequest.Greeting}, Name: {helloRequest.Name}");
                        }
                    }
                    break;
                case '4':
                    using (var call = client.InteractingHello())
                    {
                        var responseReaderTask = Task.Run(async () =>
                        {
                            while (await call.ResponseStream.MoveNext())
                            {
                                var response = call.ResponseStream.Current;
                                Console.WriteLine($"Received: {response.Message}");
                            }
                        });

                        Console.WriteLine("Enter names to greet, enter nothing to stop");
                        string input;
                        while ((input = Console.ReadLine()) != "")
                        {
                            await call.RequestStream.WriteAsync(new HelloRequest() { Greeting = "Hiya", Name = input });
                        }

                        await call.RequestStream.CompleteAsync();
                        await responseReaderTask;
                    }
                    break;
                default:
                    throw new Exception("Invalid option");
            }
        }
    }
}
