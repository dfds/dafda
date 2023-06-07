using System;
using Dafda.Configuration.ProducerConfigurations;
using Dafda.Outbox;
using Dafda.Producing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dafda.Configuration
{
    /// <summary>
    /// Facilitates Dafda configuration in .NET applications using the <see cref="IServiceCollection"/>.
    /// </summary>
    public sealed class OutboxProducerOptions
    {
        private readonly ProducerConfigurationBuilder _builder;
        private readonly IServiceCollection _services;

        internal OutboxProducerOptions(ProducerConfigurationBuilder builder, IServiceCollection services)
        {
            _services = services;
            _builder = builder;
        }

        internal IOutboxListener OutboxListener { get; private set; }

        /// <summary>
        /// Use <see cref="Microsoft.Extensions.Configuration.IConfiguration"/> as the configuration source.
        /// </summary>
        /// <param name="configuration">The configuration instance.</param>
        public void WithConfigurationSource(Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _builder.WithConfigurationSource(new DefaultConfigurationSource(configuration));
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
        /// A shorthand to set the <c>bootstrap.servers</c> Kafka configuration value.
        /// </summary>
        /// <param name="bootstrapServers">A list of bootstrap servers.</param>
        public void WithBootstrapServers(string bootstrapServers)
        {
            _builder.WithBootstrapServers(bootstrapServers);
        }

        internal void WithKafkaProducerFactory(Func<ILoggerFactory, KafkaProducer> inlineFactory)
        {
            _builder.WithKafkaProducerFactory(inlineFactory);
        }

        /// <summary>
        /// Override the default Dafda implementation of <see cref="IOutboxUnitOfWorkFactory"/>.
        /// </summary>
        /// <typeparam name="T">A custom implementation of <see cref="IOutboxUnitOfWorkFactory"/>.</typeparam>
        public void WithUnitOfWorkFactory<T>() where T : class, IOutboxUnitOfWorkFactory
        {
            _services.AddTransient<IOutboxUnitOfWorkFactory, T>();
        }

        /// <summary>
        /// Override the default Dafda implementation of <see cref="IOutboxUnitOfWorkFactory"/>.
        /// </summary>
        /// <param name="implementationFactory">The factory that creates the instance of <see cref="IOutboxUnitOfWorkFactory"/>.</param>
        public void WithUnitOfWorkFactory(Func<IServiceProvider, IOutboxUnitOfWorkFactory> implementationFactory)
        {
            _services.AddTransient(implementationFactory);
        }

        /// <summary>
        /// Add a custom implementation of <see cref="IOutboxListener"/>
        /// </summary>
        /// <param name="outboxListener">The custom implementation of <see cref="IOutboxListener"/></param>
        public void WithListener(IOutboxListener outboxListener)
        {
            OutboxListener = outboxListener;
        }

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