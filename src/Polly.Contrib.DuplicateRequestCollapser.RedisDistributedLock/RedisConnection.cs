#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

using StackExchange.Redis;

using Polly.Utilities;

namespace Polly.Contrib.DuplicateRequestCollapser.RedisDistributedLock
{
    /// <summary>
    /// A StackExchange redis connection
    /// </summary>
    public class RedisConnection : IRedisConnection
    {
        private readonly string lockValue;
        private readonly ConnectionMultiplexer connection;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connection">Connection multiplexer</param>
        public RedisConnection(ConnectionMultiplexer connection)
        {
            lockValue = Environment.MachineName;
            this.connection = connection;
        }

        /// <inheritdoc />
        public bool AcquireLock(string lockKey, TimeSpan timeout, TimeSpan retryDelay,
            Action<Exception>? exceptionNotifier, CancellationToken cancelToken)
        {
            const string script = "local f,k,v f=redis.call k=KEYS[1] v=ARGV[1] if f('get',k) then return 0 end f('set',k,v) f('expire',k,ARGV[2]) return 1";
            RedisKey[] keys = new RedisKey[] { lockKey };
            RedisValue[] values = new RedisValue[] { lockValue, (int)timeout.TotalSeconds };
            DateTime startDateTime = DateTime.UtcNow;
            Stopwatch timer = Stopwatch.StartNew();
            while (!cancelToken.IsCancellationRequested && timer.Elapsed < timeout)
            {
                try
                {
                    if ((int)connection.GetDatabase().ScriptEvaluate(script, keys, values) == 1)
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    exceptionNotifier?.Invoke(ex);
                }
                SystemClock.Sleep(retryDelay, cancelToken);
            }
            return false;
        }

        /// <inheritdoc />
        public void ReleaseLock(string lockKey)
        {
            connection.GetDatabase().KeyExpire(lockKey, TimeSpan.Zero);
        }
    }
}

#nullable disable
