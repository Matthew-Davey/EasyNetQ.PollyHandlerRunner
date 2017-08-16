namespace EasyNetQ.PollyHandlerRunner {
    using System;
    using System.Threading.Tasks;
    using EasyNetQ;
    using EasyNetQ.Consumer;
    using EasyNetQ.Internals;
    using Polly;

    /// <summary>
    /// An implementation of <see cref="HandlerRunner"/> which executes message consumers within a Polly policy.
    /// </summary>
    public class PollyHandlerRunner : HandlerRunner {
        readonly IEasyNetQLogger _logger;
        readonly Policy _policy;

        /// <summary>
        /// Initializes a new instance of the <see cref="PollyHandlerRunner"/> class.
        /// </summary>
        /// <param name="logger">A reference to an EasyNetQ logger implementation.</param>
        /// <param name="consumerErrorStrategy">A reference to a consumer error strategy.</param>
        /// <param name="eventBus">A reference to an event bus.</param>
        /// <param name="policy">A reference to the policy within which message consumers will be executed.</param>
        public PollyHandlerRunner(IEasyNetQLogger logger, IConsumerErrorStrategy consumerErrorStrategy, IEventBus eventBus, Policy policy)
            : base(logger, consumerErrorStrategy, eventBus) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _policy = policy ?? throw new ArgumentNullException(nameof(policy));
        }

        /// <inheritdoc />
        public override void InvokeUserMessageHandler(ConsumerExecutionContext context) {
            _logger.DebugWrite("Received \n\tRoutingKey: '{0}'\n\tCorrelationId: '{1}'\n\tConsumerTag: '{2}'\n\tDeliveryTag: {3}\n\tRedelivered: {4}",
                context.Info.RoutingKey,
                context.Properties.CorrelationId,
                context.Info.ConsumerTag,
                context.Info.DeliverTag,
                context.Info.Redelivered);

            Task completionTask;

            try {
                completionTask = _policy.Execute(() => {
                    var task = context.UserHandler(context.Body, context.Properties, context.Info);

                    if (task.IsFaulted)
                        throw task.Exception.GetBaseException();

                    return task;
                });
            }
            catch (Exception exception) {
                completionTask = TaskHelpers.FromException(exception);
            }

            if (completionTask.Status == TaskStatus.Created) {
                _logger.ErrorWrite("Task returned from consumer callback is not started. ConsumerTag: '{0}'", context.Info.ConsumerTag);
                return;
            }

            completionTask.ContinueWith(task => base.DoAck(context, base.GetAckStrategy(context, task)));
        }
    }
}
