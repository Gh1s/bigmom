﻿using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Csb.BigMom.Infrastructure
{
    /// <summary>
    /// Provides extension methods for <see cref="ILogger"/>.
    /// </summary>
    public static class LoggingExtensions
    {
        private static readonly string MessageTemplate = "{message}: " + Environment.NewLine + "{obj}";
        // TODO: Make it configurable.
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true,
            IgnoreNullValues = true,
            // TODO: Avoid ReferenceHandler.Preserve.
            ReferenceHandler = ReferenceHandler.Preserve,
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };

        /// <summary>
        /// Serializes and logs the provided object.
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message template.</param>
        /// <param name="obj">The object.</param>
        /// <param name="arguments">The message template arguments.</param>
        public static void LogTraceObject<T>(this ILogger logger, string message, T obj, params object[] arguments)
        {
            if (logger.IsEnabled(LogLevel.Trace))
            {
                var args = new object[arguments.Length + 2];
                args[0] = message;
                arguments.CopyTo(args, 1);
                args[^1] = JsonSerializer.Serialize(obj, obj.GetType(), SerializerOptions);
                logger.LogTrace(MessageTemplate, args);
            }
        }
    }
}
