# EasyNetQ.PollyHandlerRunner
An extension for EasyNetQ which allows you to execute message handlers within a [Polly](http://www.thepollyproject.org/about/) policy.

[![Nuget Downloads](https://img.shields.io/nuget/dt/EasyNetQ.PollyHandlerRunner.svg)](https://www.nuget.org/packages/EasyNetQ.PollyHandlerRunner/) [![Nuget Version](https://img.shields.io/nuget/v/EasyNetQ.PollyHandlerRunner.svg)](https://www.nuget.org/packages/EasyNetQ.PollyHandlerRunner/)

# Examples

Retry messages 3 times before failing...
```c#
var policy = Policy
    .Handle<Exception>()
    .Retry(3);

var bus = RabbitHutch.CreateBus("host=localhost;username=guest;password=guest",
    registrar => registrar.UseMessageHandlerPolicy(policy));
```

---

Retry MySql deadlocks only...
```c#
var policy = Policy
    .Handle<MySqlException>(error => error.Number == 1213) // 1213 = deadlock
    .RetryForever();

var bus = RabbitHutch.CreateBus("host=localhost;username=guest;password=guest",
    registrar => registrar.UseMessageHandlerPolicy(policy));
```

---

Retry messages 5 times with an exponential backoff between attempts...
```c#
var policy = Policy
    .Handle<Exception>()
    .WaitAndRetry(5, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

var bus = RabbitHutch.CreateBus("host=localhost;username=guest;password=guest",
    registrar => registrar.UseMessageHandlerPolicy(policy));
```

---

Set 5 second timeout for all message processing...
```c#
var policy = Policy.Timeout(TimeSpan.FromSeconds(5));

var bus = RabbitHutch.CreateBus("host=localhost;username=guest;password=guest",
    registrar => registrar.UseMessageHandlerPolicy(policy));
```

---

Combine policies...
```c#
var deadlockRetryPolicy = Policy
    .Handle<MySqlException>(error => error.Number == 1213)
    .Retry(3);

var timeoutPolicy = Policy
    .Timeout(TimeSpan.FromSeconds(10));

var combinedPolicy = Policy.Wrap(deadlockRetryPolicy, timeoutPolicy);

var bus = RabbitHutch.CreateBus("host=localhost;username=guest;password=guest",
    registrar => registrar.UseMessageHandlerPolicy(combinedPolicy));
```

---

Circuit breaker...
```c#
IBus bus;
ISubscriptionResult subscription;

Action establishSubscription = () => {
    subscription = bus.Subscribe<MyMessage>(message => Console.Log(message.ToString()));
};

Action<Exception, TimeSpan> onBreak = (exception, timespan) => subscription.Dispose();
Action onReset = () => establishSubscription();

// Trip the circuit breaker for 1 minute after 3 consecutive failures...
var circuitBreaker = Policy
    .Handle<Exception>()
    .CircuitBreaker(3, TimeSpan.FromMinutes(1), onBreak, onReset);

bus = RabbitHutch.CreateBus("host=localhost;username=guest;password=guest",
    registrar => registrar.UseMessageHandlerPolicy(circuitBreaker));

establishSubscription();
```

See [Polly-Samples](https://github.com/App-vNext/Polly-Samples) for more examples.
