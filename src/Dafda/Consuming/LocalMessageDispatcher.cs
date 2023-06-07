﻿using System;
using System.Threading.Tasks;
using Dafda.Consuming.Exceptions;
using Dafda.Consuming.Handlers;
using Dafda.Consuming.Interfaces;

namespace Dafda.Consuming
{
    internal class LocalMessageDispatcher
    {
        private readonly MessageHandlerRegistry _messageHandlerRegistry;
        private readonly IHandlerUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IUnconfiguredMessageHandlingStrategy _fallbackHandler;

        public LocalMessageDispatcher(
            MessageHandlerRegistry messageHandlerRegistry,
            IHandlerUnitOfWorkFactory handlerUnitOfWorkFactory,
            IUnconfiguredMessageHandlingStrategy fallbackHandler)
        {
            _messageHandlerRegistry =
                messageHandlerRegistry
                ?? throw new ArgumentNullException(nameof(messageHandlerRegistry));
            _unitOfWorkFactory =
                handlerUnitOfWorkFactory
                ?? throw new ArgumentNullException(nameof(handlerUnitOfWorkFactory));
            _fallbackHandler =
                fallbackHandler
                ?? throw new ArgumentNullException(nameof(fallbackHandler));
        }

        private MessageRegistration GetMessageRegistrationFor(TransportLevelMessage message) =>
            _messageHandlerRegistry.GetRegistrationFor(message.Metadata.Type)
            ?? _fallbackHandler.GetFallback(message.Metadata.Type);

        public async Task Dispatch(TransportLevelMessage message)
        {
            var registration = GetMessageRegistrationFor(message);

            var unitOfWork = _unitOfWorkFactory.CreateForHandlerType(registration.HandlerInstanceType);
            if (unitOfWork == null)
            {
                throw new UnableToResolveUnitOfWorkForHandlerException($"Error! Unable to create unit of work for handler type \"{registration.HandlerInstanceType.FullName}\".");
            }

            var messageInstance = message.ReadDataAs(registration.MessageInstanceType);
            var context = new MessageHandlerContext(message.Metadata);

            await unitOfWork.Run(async handler =>
            {
                if (handler == null)
                {
                    throw new InvalidMessageHandlerException($"Error! Message handler of type \"{registration.HandlerInstanceType.FullName}\" not instantiated in unit of work and message instance type of \"{registration.MessageInstanceType}\" for message type \"{registration.MessageType}\" can therefor not be handled.");
                }
                
                // TODO -- verify that the handler is in fact an implementation of IMessageHandler<registration.MessageInstanceType> to provider sane error messages.
                
                await ExecuteHandler((dynamic) messageInstance, (dynamic) handler, context);
            });
        }

        private static Task ExecuteHandler<TMessage>(
            TMessage message,
            IMessageHandler<TMessage> handler,
            MessageHandlerContext context)
        {
            return handler.Handle(message, context);
        }
    }
}
