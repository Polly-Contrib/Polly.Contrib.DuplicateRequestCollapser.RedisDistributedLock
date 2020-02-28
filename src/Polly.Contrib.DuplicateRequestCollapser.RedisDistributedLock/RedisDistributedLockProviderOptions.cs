#nullable enable

using System;
using System.Collections.Generic;
using System.Text;

using StackExchange.Redis;

namespace Polly.Contrib.DuplicateRequestCollapser.RedisDistributedLock
{
    /// <summary>
    /// Options for StackexchangeRedisDistributedLockProvider
    /// </summary>
    public class RedisDistributedLockProviderOptions
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connection">Connection</param>
        /// <param name="timeout">Wait this amount of time to get the lock</param>
        /// <param name="retryDelay">After a failed lock acquire attempt, wait this amount of time before trying again</param>
        /// <param name="exceptionNotifier">Notifier when exceptions are thrown. Exceptions will not bring down
        /// the lock, they will be informational only.</param>
        public RedisDistributedLockProviderOptions(IRedisConnection connection,
            TimeSpan timeout, TimeSpan retryDelay, Action<Exception>? exceptionNotifier = null)
        {
            if (timeout.Ticks < 1)
            {
                throw new ArgumentOutOfRangeException($"{nameof(timeout)} must be greater than zero");
            }
            if (retryDelay.Ticks < 1)
            {
                throw new ArgumentOutOfRangeException($"{nameof(retryDelay)} must be greater than zero");
            }
            Connection = connection;
            Timeout = timeout;
            RetryDelay = retryDelay;
            ExceptionNotifier = exceptionNotifier;
        }

        /// <summary>
        /// Redis connection
        /// </summary>
        public IRedisConnection Connection { get; }

        /// <summary>
        /// WAit a maximum amount of this time to get the lock, failing if the time to get the lock goes over this value
        /// </summary>
        public TimeSpan Timeout { get; }

        /// <summary>
        /// Retry wait time after each failed distributed lock attempt
        /// </summary>
        public TimeSpan RetryDelay { get; }

        /// <summary>
        /// Exception notifier, optional
        /// </summary>
        public Action<Exception>? ExceptionNotifier { get; }
    }
}

#nullable restore
