namespace Dafda.Tests.Builders;

using System;
using Dafda.Consuming;
using Dafda.Consuming.MessageFilters;
using TestDoubles;

internal class ConsumerBuilder
{
    private IHandlerUnitOfWorkFactory _unitOfWorkFactory = new HandlerUnitOfWorkFactoryStub(null);
    private IConsumerScopeFactory _consumerScopeFactory = new ConsumerScopeFactoryStub(new ConsumerScopeStub(new MessageResultBuilder().Build()));
    private MessageHandlerRegistry _registry = new();
    private IUnconfiguredMessageHandlingStrategy _unconfiguredMessageStrategy = new RequireExplicitHandlers();
    private readonly IMessageHandlerExecutionStrategy _messageHandlerExecutionStrategy = new DirectMessageHandlerExecutionStrategy();

    private bool _enableAutoCommit;
    private MessageFilter _messageFilter = MessageFilter.Default;
    private IDeadLetterQueue _deadLetterQueue = NullDeadLetterQueue.Instance;
    private int _maxRetries;
    private Func<Exception, bool> _deadLetterQueueBypass;

    public ConsumerBuilder WithUnitOfWork(IHandlerUnitOfWork unitOfWork)
    {
        return WithUnitOfWorkFactory(new HandlerUnitOfWorkFactoryStub(unitOfWork));
    }

    public ConsumerBuilder WithUnitOfWorkFactory(IHandlerUnitOfWorkFactory unitofWorkFactory)
    {
        _unitOfWorkFactory = unitofWorkFactory;
        return this;
    }

    public ConsumerBuilder WithConsumerScopeFactory(IConsumerScopeFactory consumerScopeFactory)
    {
        _consumerScopeFactory = consumerScopeFactory;
        return this;
    }

    public ConsumerBuilder WithMessageHandlerRegistry(MessageHandlerRegistry registry)
    {
        _registry = registry;
        return this;
    }

    public ConsumerBuilder WithEnableAutoCommit(bool enableAutoCommit)
    {
        _enableAutoCommit = enableAutoCommit;
        return this;
    }

    public void WithMessageFilter(MessageFilter messageFilter)
    {
        _messageFilter = messageFilter;
    }

    public ConsumerBuilder WithUnconfiguredMessageStrategy(
        IUnconfiguredMessageHandlingStrategy strategy)
    {
        _unconfiguredMessageStrategy = strategy;
        return this;
    }

    public ConsumerBuilder WithDeadLetterQueue(IDeadLetterQueue deadLetterQueue)
    {
        _deadLetterQueue = deadLetterQueue;
        return this;
    }

    public ConsumerBuilder WithMaxRetries(int maxRetries)
    {
        _maxRetries = maxRetries;
        return this;
    }

    public ConsumerBuilder WithDeadLetterQueueBypass(Func<Exception, bool> deadLetterQueueBypass)
    {
        _deadLetterQueueBypass = deadLetterQueueBypass;
        return this;
    }

    public Consumer Build() =>
        new Consumer(
            _registry,
            _unitOfWorkFactory,
            _consumerScopeFactory,
            _unconfiguredMessageStrategy,
            _messageFilter,
            _messageHandlerExecutionStrategy,
            _enableAutoCommit,
            _deadLetterQueue,
            _maxRetries,
            _deadLetterQueueBypass);
}