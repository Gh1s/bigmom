﻿using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System;

namespace Csb.BigMom.Infrastructure
{
    /// <summary>
    /// Provides a wrapper for <see cref="IProducer{TKey, TValue}"/> and its <see cref="KafkaProducerOptions"/>.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    public class KafkaProducerWrapper<TKey, TValue>
    {
        /// <summary>
        /// The producer instance.
        /// </summary>
        public IProducer<TKey, TValue> Producer { get; }

        /// <summary>
        /// The producer's options monitor.
        /// </summary>
        public IOptionsMonitor<KafkaProducerOptions<TValue>> OptionsMonitor { get; }

        /// <summary>
        /// The producer's options.
        /// </summary>
        public KafkaProducerOptions<TValue> Options => OptionsMonitor.CurrentValue;

        public KafkaProducerWrapper(IProducer<TKey, TValue> producer, IOptionsMonitor<KafkaProducerOptions<TValue>> options)
        {
            Producer = producer ?? throw new ArgumentNullException(nameof(producer));
            OptionsMonitor = options ?? throw new ArgumentNullException(nameof(options));
        }
    }
}
