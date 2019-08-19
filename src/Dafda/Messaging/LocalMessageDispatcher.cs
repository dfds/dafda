using System;
using System.Threading.Tasks;

namespace Dafda.Messaging
{
    public class LocalMessageDispatcher : ILocalMessageDispatcher
    {
        private readonly IMessageHandlerRegistry _handlerRegistry;
        private readonly IHandlerUnitOfWorkFactory _handlerUnitOfWorkFactory;

        public LocalMessageDispatcher(IMessageHandlerRegistry handlerRegistry, IHandlerUnitOfWorkFactory handlerUnitOfWorkFactory)
        {
            if (handlerRegistry == null)
            {
                throw new ArgumentNullException(nameof(handlerRegistry));
            }

            if (handlerUnitOfWorkFactory == null)
            {
                throw new ArgumentNullException(nameof(handlerUnitOfWorkFactory));
            }

            _handlerRegistry = handlerRegistry;
            _handlerUnitOfWorkFactory = handlerUnitOfWorkFactory;
        }

        private MessageRegistration GetMessageRegistrationFor(ITransportLevelMessage message)
        {
            var registration = _handlerRegistry.GetRegistrationFor(message.Type);

            if (registration == null)
            {
                throw new MissingMessageHandlerRegistrationException($"Error! A handler has not been registered for messages of type \"{message.Type}\". Message with id \"{message.MessageId}\" was not handled.");
            }

            return registration;
        }

        public async Task Dispatch(ITransportLevelMessage message)
        {
            var registration = GetMessageRegistrationFor(message);

            var messageInstance = message.ReadDataAs(registration.MessageInstanceType);

            #region handling scope

            var unitOfWork = _handlerUnitOfWorkFactory.CreateForHandlerType(registration.HandlerInstanceType);

            if (unitOfWork == null)
            {
                throw new UnableToResolveUnitOfWorkForHandlerException($"Error! Unable to create unit of work for handler type \"{registration.HandlerInstanceType.FullName}\".");
            }

            await unitOfWork.Run(handler2 => ExecuteHandler((dynamic)messageInstance, (dynamic)handler2));

            //var handler = _typeResolver.Resolve(registration.HandlerInstanceType);
            //if (handler == null)
            //{
            //    throw new UnableToResolveMessageHandlerException($"Type resolver {_typeResolver.GetType().FullName} resolved handler type {registration.HandlerInstanceType.FullName} to NULL.");
            //}

            //await ExecuteHandler((dynamic)messageInstance, (dynamic)handler);

            #endregion
        }

        private static Task ExecuteHandler<TMessage>(TMessage message, IMessageHandler<TMessage> handler) where TMessage : class, new()
        {
            return handler.Handle(message);
        }
    }

    public interface IHandlerUnitOfWork
    {
        Task Run(Func<object, Task> handlingAction);
        //Task Run<TMessage>(Func<IMessageHandler<TMessage>, Task> handlingAction) where TMessage : class, new();
    }

    public interface IHandlerUnitOfWorkFactory
    {
        IHandlerUnitOfWork CreateForHandlerType(Type handlerType);
    }

    public class DefaultUnitOfWorkFactory : IHandlerUnitOfWorkFactory
    {
        private readonly ITypeResolver _typeResolver;

        public DefaultUnitOfWorkFactory(ITypeResolver typeResolver)
        {
            _typeResolver = typeResolver;
        }

        public IHandlerUnitOfWork CreateForHandlerType(Type handlerType)
        {
            return new DefaultUnitOfWork(_typeResolver, handlerType);
        }
    }

    public class DefaultUnitOfWork : IHandlerUnitOfWork
    {
        private readonly ITypeResolver _typeResolver;
        private readonly Type _handlerType;

        public DefaultUnitOfWork(ITypeResolver typeResolver, Type handlerType)
        {
            _typeResolver = typeResolver;
            _handlerType = handlerType;
        }

        public async Task Run(Func<object, Task> handlingAction)
        {
            var handler = _typeResolver.Resolve(_handlerType);
            await handlingAction(handler);
        }

        public Task Run<TMessage>(Func<IMessageHandler<TMessage>, Task> handlingAction) where TMessage : class, new()
        {
            throw new NotImplementedException();
        }
    }
}