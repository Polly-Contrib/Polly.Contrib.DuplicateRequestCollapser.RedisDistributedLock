#nullable enable

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using StackExchange.Redis;

namespace Polly.Contrib.DuplicateRequestCollapser.RedisDistributedLock
{
    /// <summary>
    /// Provides a lock scoped to a distributed key. Unlike other local locks, this lock provider
    /// can throw an <see cref="OperationCanceledException" /> if the attempt to acquire the lock times out or is cancelled by the <see cref="CancellationToken" /> passed to execution through the policy.
    /// </summary>
    public class RedisDistributedLockProvider : ISyncLockProvider
    {
        private readonly RedisDistributedLockProviderOptions options;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options">Locking options. This contains the connection multiplexer, lock timeout,
        /// lock retry interval and exception notifier</param>
        public RedisDistributedLockProvider(RedisDistributedLockProviderOptions options)
        {
            this.options = options;
        }

        /// <inheritdoc/>
        public IDisposable AcquireLock(string key, Context context, CancellationToken cancellationToken)
        {
            DistributedLockReleaser distributedLock = new DistributedLockReleaser(key, this);
            bool gotLock = distributedLock.AcquireLock(cancellationToken);
            if (!gotLock)
            {
                throw new OperationCanceledException("Failed to acquire distributed lock for key " + key);
            }
            return distributedLock;
        }

        private class DistributedLockReleaser : IDisposable
        {
            private readonly RedisDistributedCacheLock _distributedLock;

            private RedisDistributedLockProvider? _lockProvider;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="key">Key</param>
            /// <param name="provider">Lock provider</param>
            public DistributedLockReleaser(string key, RedisDistributedLockProvider provider)
            {
                _lockProvider = provider;
                _distributedLock = new RedisDistributedCacheLock(key, provider.options);
            }

            /// <summary>
            /// Acquire the distributed lock
            /// </summary>
            /// <param name="cancelToken">Cancel token</param>
            /// <returns>True if lock acquired, false otherwise</returns>
            public bool AcquireLock(CancellationToken cancelToken)
            {
                return _distributedLock.AcquireLock(cancelToken);
            }

            /// <inheritdoc/>
            public void Dispose()
            {
                RedisDistributedLockProvider? provider = _lockProvider;
                if (provider != null && Interlocked.CompareExchange(ref _lockProvider, null, provider) == provider)
                {
                    _distributedLock.Dispose();
                }
            }
        }

        private class RedisDistributedCacheLock : IDisposable
        {
            private readonly IRedisConnection connection;
            private readonly string lockKey;
            private readonly TimeSpan timeout;
            private readonly TimeSpan retryDelay;
            private readonly Action<Exception>? exceptionNotifier;

            private bool hasLock;

            public RedisDistributedCacheLock(string key, RedisDistributedLockProviderOptions options)
            {
                this.connection = options.Connection;
                this.lockKey = "LOCK_" + key;
                this.timeout = options.Timeout;
                this.retryDelay = options.RetryDelay;
                this.exceptionNotifier = options.ExceptionNotifier;
            }

            public bool AcquireLock(CancellationToken cancelToken)
            {
                return (hasLock = connection.AcquireLock(lockKey, timeout, retryDelay, exceptionNotifier, cancelToken));
            }

            /// <inheritdoc />
            public void Dispose()
            {
                if (hasLock)
                {
                    // get key out of there, we no longer need the lock
                    hasLock = false;
                    connection.ReleaseLock(lockKey);
                }
            }
        }
    }
}

#nullable restore
