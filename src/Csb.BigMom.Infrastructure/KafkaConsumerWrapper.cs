using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System;

namespace Csb.BigMom.Infrastructure
{
    /// <summary>
    /// Provides a wrapper for <see cref="IConsumer{TKey, TValue}"/> and its <see cref="KafkaConsumerOptions"/>.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    public class KafkaConsumerWrapper<TKey, TValue>
    {
        /// <summary>
        /// The consumer instance.
        /// </summary>
        public IConsumer<TKey, TValue> Consumer { get; }

        /// <summary>
        /// The consumer's options monitor.
        /// </summary>
        public IOptionsMonitor<KafkaConsumerOptions<TValue>> OptionsMonitor { get; }

        /// <summary>
        /// The consumer's options.
        /// </summary>
        public KafkaConsumerOptions<TValue> Options => OptionsMonitor.CurrentValue;

        public KafkaConsumerWrapper(IConsumer<TKey, TValue> consumer, IOptionsMonitor<KafkaConsumerOptions<TValue>> optionsMonitor)
        {
            Consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
            OptionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
        }
    }
}
