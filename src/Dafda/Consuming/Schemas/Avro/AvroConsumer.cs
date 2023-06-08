using Dafda.Consuming.MessageFilters;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using Avro.Specific;
using System.Diagnostics;
using Dafda.Consuming.Interfaces;
using Dafda.Consuming.Avro;

namespace Dafda.Consuming.Schemas.Avro
{
    internal class AvroConsumer<TKey, TValue> : IConsumer where TValue : ISpecificRecord
    {
        private readonly MessageRegistration<TKey, TValue> _messageRegistration;
        private readonly IConsumerScopeFactory<MessageResult<TKey, TValue>> _consumerScopeFactory;
        private readonly bool _isAutoCommitEnabled;
        private readonly IHandlerUnitOfWorkFactory _unitOfWorkFactory;

        public AvroConsumer(
            MessageRegistration<TKey, TValue> messageHandler,
            IHandlerUnitOfWorkFactory unitOfWorkFactory,
            IConsumerScopeFactory<MessageResult<TKey, TValue>> consumerScopeFactory,
            bool isAutoCommitEnabled = false)
        {
            _messageRegistration = messageHandler;
            _unitOfWorkFactory = unitOfWorkFactory;
            _consumerScopeFactory =
                consumerScopeFactory
                ?? throw new ArgumentNullException(nameof(consumerScopeFactory));

            _isAutoCommitEnabled = isAutoCommitEnabled;
        }

        public async Task ConsumeAll(CancellationToken cancellationToken)
        {
            using (var consumerScope = _consumerScopeFactory.CreateConsumerScope())
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await ProcessNextMessage(consumerScope, cancellationToken);
                }
            }
        }

        public async Task ConsumeSingle(CancellationToken cancellationToken)
        {
            using (var consumerScope = _consumerScopeFactory.CreateConsumerScope())
            {
                await ProcessNextMessage(consumerScope, cancellationToken);
            }
        }

        private async Task ProcessNextMessage(IConsumerScope<MessageResult<TKey, TValue>> consumerScope, CancellationToken cancellationToken)
        {

            var messageResult = await consumerScope.GetNext(cancellationToken);
            var messageContext = new MessageHandlerContext(new Metadata() { Type = typeof(TValue).ToString() }); //TODO: Fix the MessageHandlerContext

            var unitOfWork = _unitOfWorkFactory.CreateForHandlerType(_messageRegistration.HandlerInstanceType);

            if (unitOfWork == null)
                throw new UnableToResolveUnitOfWorkForHandlerException($"Error! Unable to create unit of work for handler type \"{_messageRegistration.HandlerInstanceType.FullName}\".");


            await unitOfWork.Run(async handler =>
            {
                if (handler == null)
                    throw new InvalidMessageHandlerException($"Error! Message handler of type \"{_messageRegistration.HandlerInstanceType.FullName}\" not instantiated in unit of work and message instance type of \"{_messageRegistration.MessageInstanceType}\" for message type \"{_messageRegistration.MessageInstanceType}\" can therefor not be handled.");

                // TODO -- verify that the handler is in fact an implementation of IMessageHandler<registration.MessageInstanceType> to provider sane error messages.
                //await ((IMessageHandler<TValue>)handler).Handle(messageResult.Value, messageContext);
                await ((IMessageHandler<MessageResult<TKey, TValue>>)handler).Handle(messageResult, messageContext);
            });

            if (!_isAutoCommitEnabled)
            {
                await messageResult.Commit();
            }
        }
    }
}
