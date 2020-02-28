using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xunit;

namespace Polly.Contrib.DuplicateRequestCollapser.RedisDistributedLock.Specs
{
    public class DistributedLockTests : IRedisConnection
    {
        private Exception throwException;
        private bool acquired;
        private bool released;

        bool IRedisConnection.AcquireLock(string lockKey, TimeSpan timeout, TimeSpan retryDelay, Action<Exception> exceptionNotifier, CancellationToken cancelToken)
        {
            if (throwException != null)
            {
                exceptionNotifier?.Invoke(throwException);
                throwException = null;
            }
            return acquired;
        }

        void IRedisConnection.ReleaseLock(string lockKey)
        {
            released = true;
        }

        [Fact]
        public void TestLockTimesOut()
        {
            RedisDistributedLockProvider provider = new RedisDistributedLockProvider(new RedisDistributedLockProviderOptions
                (this, TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(2)));
            acquired = false;
            try
            {
                IDisposable theLock = provider.AcquireLock("Test", new Context(), default);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            throw new Exception($"{nameof(OperationCanceledException)} should have been thrown");
        }

        [Fact]
        public void TestExceptionNotifierDoesNotExecute()
        {
            Exception foundEx = null;
            void exceptionNotifier(Exception ex)
            {
                foundEx = ex;
            }
            RedisDistributedLockProvider provider = new RedisDistributedLockProvider(new RedisDistributedLockProviderOptions
                (this, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(10), exceptionNotifier));
            acquired = true;
            throwException = null;
            IDisposable theLock = provider.AcquireLock("Test", new Context(), default);
            theLock.Dispose();
            foundEx.Should().BeNull();
            released.Should().BeTrue();
        }

        [Fact]
        public void TestExceptionNotifierExecutes()
        {
            Exception foundEx = null;
            void exceptionNotifier(Exception ex)
            {
                foundEx = ex;
            }

            RedisDistributedLockProvider provider = new RedisDistributedLockProvider(new RedisDistributedLockProviderOptions
                (this, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(10), exceptionNotifier));
            acquired = true;
            throwException = new Exception("Test");
            IDisposable theLock = provider.AcquireLock("Test", new Context(), default);
            theLock.Dispose();
            foundEx.Should().NotBeNull();
            released.Should().BeTrue();
        }
    }
}
