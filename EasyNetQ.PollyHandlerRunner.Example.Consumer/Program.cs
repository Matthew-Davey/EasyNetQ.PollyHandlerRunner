namespace EasyNetQ.PollyHandlerRunner.Example.Consumer
{
    using System;
    using EasyNetQ;
    using EasyNetQ.Loggers;
    using EasyNetQ.PollyHandlerRunner.Example.Message;
    using Polly;

    class Program {
        static void Main() {
            var policy = Policy
                .Handle<HandlerException>()
                .RetryForever(exception => {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Retrying message {0}", ((HandlerException)exception).MessageIndex);
                    Console.ForegroundColor = ConsoleColor.Gray;
                });

            var bus = RabbitHutch.CreateBus("host=localhost;username=guest;password=guest", registrar => registrar
                .Register<IEasyNetQLogger>(_ => new ConsoleLogger())
                .UseMessageHandlerPolicy(policy)
            );

            Console.CancelKeyPress += (_, __) => bus.Dispose();

            var random = new Random();

            bus.Subscribe<ExampleEvent>(String.Empty, message => {
                if (random.Next(2) == 0) // 1 in 3 consumer callbacks will fail...
                    throw new HandlerException(message.MessageIndex);
            });
        }
    }
}
