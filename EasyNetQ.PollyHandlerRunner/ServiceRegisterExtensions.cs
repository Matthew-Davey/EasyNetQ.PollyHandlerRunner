namespace EasyNetQ.PollyHandlerRunner {
    using System;
    using EasyNetQ.Consumer;
    using EasyNetQ;
    using Polly;

    /// <summary>
    /// Defines extension methods for the <see cref="IServiceRegister"/> interface.
    /// </summary>
    public static class ServiceRegisterExtensions {
        /// <summary>
        /// Configures EasyNetQ to execute message handlers within a Polly policy.
        /// </summary>
        /// <param name="registrar">Extended instance.</param>
        /// <param name="policy">The policy within which message handlers will be executed.</param>
        /// <returns>Extended instance.</returns>
        public static IServiceRegister UseMessageHandlerPolicy(this IServiceRegister registrar, Policy policy) {
            if (registrar == null)
                throw new ArgumentNullException(nameof(registrar));

            if (policy == null)
                throw new ArgumentNullException(nameof(policy));

            registrar.Register<IHandlerRunner>(services =>
                new PollyHandlerRunner(
                    services.Resolve<IEasyNetQLogger>(),
                    services.Resolve<IConsumerErrorStrategy>(),
                    services.Resolve<IEventBus>(),
                    policy));

            return registrar;
        }
    }
}
