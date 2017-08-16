namespace EasyNetQ.PollyHandlerRunner.Example.Producer {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using EasyNetQ;
    using EasyNetQ.Loggers;
    using EasyNetQ.PollyHandlerRunner.Example.Message;

    class Program {
        static void Main() {
            var bus = RabbitHutch.CreateBus("host=localhost;username=guest;password=guest", registrar => {
                registrar.Register<IEasyNetQLogger>(_ => new ConsoleLogger());
            });

            using (bus) {
                var cancellationTokenSource = new CancellationTokenSource();

                Console.CancelKeyPress += (_, eventArgs) => {
                    eventArgs.Cancel = true;
                    cancellationTokenSource.Cancel();
                };

                var messageIndex = 0;

                while (!cancellationTokenSource.IsCancellationRequested) {
                    bus.Publish(new ExampleEvent {
                        MessageIndex = ++messageIndex
                    });

                    Task.Delay(1000).Wait();
                }
            }
        }
    }
}
