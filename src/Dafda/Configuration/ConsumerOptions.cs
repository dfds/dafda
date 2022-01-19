using System;
using Dafda.Consuming;
using Dafda.Consuming.MessageFilters;
using Microsoft.Extensions.DependencyInjection;

namespace Dafda.Configuration
{
    /// <summary>
    /// Facilitates Dafda configuration in .NET applications using the <see cref="IServiceCollection"/>.
    /// </summary>
    public sealed class ConsumerOptions
    {
        private readonly ConsumerConfigurationBuilder _builder;
        private readonly IServiceCollection _services;

        internal ConsumerOptions(ConsumerConfigurationBuilder builder, IServiceCollection services)
        {
            _services = services;
            _builder = builder;
        }

        /// <summary>
        /// Sets the partion offset for all subscribed topics to the beginning
        /// </summary>
        public void ReadFromBeginningOfTopics()
        {
            _builder.ReadFromBeginning();
        }

        /// <summary>
        /// Specify a custom implementation of the <see cref="ConfigurationSource"/> to use. 
        /// </summary>
        /// <param name="configurationSource">The <see cref="ConfigurationSource"/> to use.</param>
        public void WithConfigurationSource(ConfigurationSource configurationSource)
        {
            _builder.WithConfigurationSource(configurationSource);
        }

        /// <summary>
        /// Use <see cref="Microsoft.Extensions.Configuration.IConfiguration"/> as the configuration source.
        /// </summary>
        /// <param name="configuration">The configuration instance.</param>
        public void WithConfigurationSource(Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _builder.WithConfigurationSource(new DefaultConfigurationSource(configuration));
        }

        /// <summary>
        /// Add a custom naming convention for converting configuration keys when
        /// looking up keys in the <see cref="ConfigurationSource"/>.
        /// </summary>
        /// <param name="converter">Use this to transform keys.</param>
        public void WithNamingConvention(Func<string, string> converter)
        {
            _builder.WithNamingConvention(converter);
        }

        /// <summary>
        /// Add default environment style naming convention. The configuration will attempt to
        /// fetch keys from <see cref="ConfigurationSource"/>, using the following scheme:
        /// <list type="bullet">
        ///     <item><description>keys will be converted to uppercase.</description></item>
        ///     <item><description>any one or more of <c>SPACE</c>, <c>TAB</c>, <c>.</c>, and <c>-</c> will be converted to a single <c>_</c>.</description></item>
        ///     <item><description>the prefix will be prefixed (in uppercase) along with a <c>_</c>.</description></item>
        /// </list>
        /// 
        /// When configuring a consumer the <c>WithEnvironmentStyle("app")</c>, Dafda will attempt to find the
        /// key <c>APP_GROUP_ID</c> in the <see cref="ConfigurationSource"/>.
        /// </summary>
        /// <param name="prefix">The prefix to use before keys.</param>
        /// <param name="additionalPrefixes">Additional prefixes to use before keys.</param>
        public void WithEnvironmentStyle(string prefix = null, params string[] additionalPrefixes)
        {
            _builder.WithEnvironmentStyle(prefix, additionalPrefixes);
        }

        /// <summary>
        /// Add a configuration key/value directly to the underlying Kafka consumer.
        /// </summary>
        /// <param name="key">The configuration key.</param>
        /// <param name="value">The configuration value.</param>
        public void WithConfiguration(string key, string value)
        {
            _builder.WithConfiguration(key, value);
        }

        /// <summary>
        /// A shorthand to set the <c>group.id</c> Kafka configuration value.
        /// </summary>
        /// <param name="groupId">The group id for the consumer.</param>
        public void WithGroupId(string groupId)
        {
            _builder.WithGroupId(groupId);
        }

        /// <summary>
        /// A shorthand to set the <c>bootstrap.servers</c> Kafka configuration value.
        /// </summary>
        /// <param name="bootstrapServers">A list of bootstrap servers.</param>
        public void WithBootstrapServers(string bootstrapServers)
        {
            _builder.WithBootstrapServers(bootstrapServers);
        }

        /// <summary>
        /// Override the default Dafda implementation of <see cref="IHandlerUnitOfWorkFactory"/>.
        /// </summary>
        /// <typeparam name="T">A custom implementation of <see cref="IHandlerUnitOfWorkFactory"/>.</typeparam>
        public void WithUnitOfWorkFactory<T>() where T : class, IHandlerUnitOfWorkFactory
        {
            _services.AddTransient<IHandlerUnitOfWorkFactory, T>();
        }

        /// <summary>
        /// Override the default Dafda implementation of <see cref="IHandlerUnitOfWorkFactory"/>.
        /// </summary>
        /// <param name="implementationFactory">The factory that creates the instance of <see cref="IHandlerUnitOfWorkFactory"/>.</param>
        public void WithUnitOfWorkFactory(Func<IServiceProvider, IHandlerUnitOfWorkFactory> implementationFactory)
        {
            _services.AddTransient(implementationFactory);
        }

        internal void WithConsumerScopeFactory(Func<IServiceProvider, IConsumerScopeFactory> consumerScopeFactory)
        {
            _builder.WithConsumerScopeFactory(consumerScopeFactory);
        }

        /// <summary>
        /// Override the default Dafda implementation of <see cref="IIncomingMessageFactory"/>.
        /// </summary>
        /// <param name="incomingMessageFactory">A custom implementation of <see cref="IIncomingMessageFactory"/>.</param>
        public void WithIncomingMessageFactory(Func<IServiceProvider, IIncomingMessageFactory> incomingMessageFactory)
        {
            _builder.WithIncomingMessageFactory(incomingMessageFactory);
        }

        /// <summary>
        /// If the <see cref="IIncomingMessageFactory"/> throws an exception during message deserialization, 
        /// Dafda will create a <see cref="TransportLevelPoisonMessage"/> 
        /// that can be handled by the consumer instead of throwing an exception.
        /// Note, if you wish to overwrite the default <see cref="IIncomingMessageFactory"/> you should do so before enabling poison message handling
        /// </summary>
        public void WithPoisonMessageHandling()
        {
            _builder.WithPoisonMessageHandling();
        }

        /// <summary>
        /// Applies a filter that must be evaluated when consuming events.
        /// If the filter evaluated returns false, the event will not be sent to the registered EventHandler class.
        /// If the filter evaluated returns true, the event will be sent to the registered EventHandler.
        /// In either case, the commit logic will continue and the index will be updated.
        /// </summary>
        /// <param name="messageFilter">Overridable message filter exposing CanAcceptMessage evaluation.></param>
        public void WithMessageFilter(MessageFilter messageFilter)
        {
            _builder.WithMessageFilter(messageFilter);
        }

        /// <summary>
        /// Register a message handler for <paramref name="messageType"/> on <paramref name="topic"/>. The
        /// specified <typeparamref name="TMessageHandler"/> must implements <see cref="IMessageHandler{T}"/>
        /// closing on the <typeparamref name="TMessage"/> type.
        /// </summary>
        /// <typeparam name="TMessage">The message type.</typeparam>
        /// <typeparam name="TMessageHandler">The message handler.</typeparam>
        /// <param name="topic">The name of the topic in Kafka.</param>
        /// <param name="messageType">The messageType as specified in the Dafda envelope in the Kafka message.</param>
        public void RegisterMessageHandler<TMessage, TMessageHandler>(string topic, string messageType)
            where TMessageHandler : class, IMessageHandler<TMessage>
        {
            _builder.RegisterMessageHandler<TMessage, TMessageHandler>(topic, messageType);
            _services.AddTransient<TMessageHandler>();
        }

        /// <summary>
        /// Register a strategy for handling messages that are not explicitly configured with handlers
        /// </summary>
        public void WithUnconfiguredMessageHandlingStrategy<T>()
            where T: class, IUnconfiguredMessageHandlingStrategy =>
            _services.AddTransient<IUnconfiguredMessageHandlingStrategy, T>();

        private class DefaultConfigurationSource : ConfigurationSource
        {
            private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

            public DefaultConfigurationSource(Microsoft.Extensions.Configuration.IConfiguration configuration)
            {
                _configuration = configuration;
            }

            public override string GetByKey(string key)
            {
                return _configuration[key];
            }
        }
    }
}
