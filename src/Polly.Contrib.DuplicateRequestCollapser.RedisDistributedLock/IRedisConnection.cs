#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Polly.Contrib.DuplicateRequestCollapser.RedisDistributedLock
{
    /// <summary>
    /// Represents a connection to redis
    /// </summary>
    public interface IRedisConnection
    {
        /// <summary>
        /// Attempt to acquire a distributed lock.
        /// </summary>
        /// <param name="lockKey">Key to lock on</param>
        /// <param name="timeout">Lock timeout, after this time, lock fails to acquire</param>
        /// <param name="retryDelay">Amount to wait in between each failed attempt to acquire the lock</param>
        /// <param name="exceptionNotifier">Notifier for any exceptions</param>
        /// <param name="cancelToken">Cancellation token</param>
        /// <returns>True if the lock is acquired, false otherwise.</returns>
        bool AcquireLock(string lockKey, TimeSpan timeout, TimeSpan retryDelay, Action<Exception>? exceptionNotifier,
            CancellationToken cancelToken);

        /// <summary>
        /// Release a distributed lock
        /// </summary>
        /// <param name="lockKey">Key to release lock on</param>
        void ReleaseLock(string lockKey);
    }
}

#nullable disable
