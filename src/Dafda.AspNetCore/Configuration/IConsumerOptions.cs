using System;
using Dafda.Consuming;
using Dafda.Messaging;

namespace Dafda.Configuration
{
    public interface IConsumerOptions
    {
        void WithConfigurationSource(ConfigurationSource configurationSource);
        void WithConfigurationSource(Microsoft.Extensions.Configuration.IConfiguration configuration);
        void WithNamingConvention(NamingConvention namingConvention);
        void WithEnvironmentStyle(string prefix = null, params string[] additionalPrefixes);
        void WithConfiguration(string key, string value);
        void WithGroupId(string groupId);
        void WithBootstrapServers(string bootstrapServers);
        void WithUnitOfWorkFactory<T>() where T : class, IHandlerUnitOfWorkFactory;
        void WithUnitOfWorkFactory(Func<IServiceProvider, IHandlerUnitOfWorkFactory> implementationFactory);
        void WithUnitOfWork<T>() where T : ScopedUnitOfWork;
        void WithUnitOfWork(Func<IServiceProvider, ScopedUnitOfWork> implementationFactory);
        void WithTopicSubscriberScopeFactory(ITopicSubscriberScopeFactory topicSubscriberScopeFactory);
        void RegisterMessageHandler<TMessage, TMessageHandler>(string topic, string messageType)
            where TMessage : class, new()
            where TMessageHandler : class, IMessageHandler<TMessage>;
        void WithAutoCommitOnly(int autoCommitIntervalMs);
        void WithManualCommitOnly();
    }
}