using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcDemoServer
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;
        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Message = $"{request.Greeting} {request.Name}"
            });
        }

        public override async Task ParrotSaysHello(HelloRequest request, IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
        {
            while(!context.CancellationToken.IsCancellationRequested)
            {
                await responseStream.WriteAsync(new HelloReply()
                {
                    Message = $"{request.Greeting} {request.Name}"
                });

                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        public override async Task<DelayedReply> ChattyClientSaysHello(IAsyncStreamReader<HelloRequest> requestStream, ServerCallContext context)
        {
            DelayedReply reply = new DelayedReply();
            await foreach (var message in requestStream.ReadAllAsync())
            {
                reply.Request.Add(message);
            }

            reply.Message = $"You have sent {reply.Request.Count} message. Please expect a delayed reply!";
            return reply;
        }

        public override async Task InteractingHello(IAsyncStreamReader<HelloRequest> requestStream, IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var request = requestStream.Current;
                await responseStream.WriteAsync(new HelloReply() {Message = $"{request.Greeting} {request.Name}"});
            }
        }
    }
}
