using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dafda.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Sample.Infrastructure.Persistence;

namespace Sample.Application
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureCommands(this IServiceCollection services, Action<CommandRegistry> configure)
        {
            var registry = new CommandRegistry(services);
            configure(registry);
            services.AddSingleton(registry);
            services.AddTransient<CommandProcessor>();
        }
    }

    public class CommandRegistry
    {
        private readonly IServiceCollection _services;
        private readonly IDictionary<Type, Type> _commandHandlers = new ConcurrentDictionary<Type, Type>();

        public CommandRegistry(IServiceCollection services)
        {
            _services = services;
        }

        public void Register<TCommand, TCommandHandler>()
            where TCommand : ICommand
            where TCommandHandler : class, ICommandHandler<TCommand>
        {
            _commandHandlers.Add(typeof(TCommand), typeof(TCommandHandler));
            _services.AddTransient<TCommandHandler>();
        }

        public Type GetCommandHandlerType(Type commandType)
        {
            if (_commandHandlers.TryGetValue(commandType, out var commandHandlerType))
            {
                return commandHandlerType;
            }

            throw new InvalidOperationException($"No command handler was registered for {commandType.Name}");
        }
    }

    public class CommandProcessor
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly CommandRegistry _commandRegistry;

        public CommandProcessor(IServiceScopeFactory serviceScopeFactory, CommandRegistry commandRegistry)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _commandRegistry = commandRegistry;
        }

        public async Task Process<TCommand>(TCommand command) where TCommand : ICommand
        {
            var commandHandlerType = _commandRegistry.GetCommandHandlerType(typeof(TCommand));

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
                using (var transaction = dbContext.Database.BeginTransaction())
                {
                    var commandHandler = scope.ServiceProvider.GetRequiredService(commandHandlerType);

                    await ExecuteHandler((dynamic) command, (dynamic) commandHandler);

                    await dbContext.SaveChangesAsync();
                    transaction.Commit();
                }

                var waiter = scope.ServiceProvider.GetRequiredService<IOutboxWaiter>();
                waiter.WakeUp();
            }
        }

        private static Task ExecuteHandler<TCommand>(TCommand command, ICommandHandler<TCommand> handler)
            where TCommand : ICommand
        {
            return handler.Handle(command);
        }
    }

    public interface ICommand
    {
    }

    public interface ICommandHandler<in T> where T : ICommand
    {
        Task Handle(T command);
    }
}