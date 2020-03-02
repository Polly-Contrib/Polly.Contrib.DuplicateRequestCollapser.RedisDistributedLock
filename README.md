# Polly.Contrib.DuplicateRequestCollapser.RedisDistributedLock

This project contains a lock interface that uses redis to do distributed locking.

## Why use a distributed lock?
A distributed lock is great for the really big, expensive calculations or queries, etc. Something that can be measured in seconds time to get back is a great candidate for a distributed lock.

See `RedisDistributedLockProvider.cs`.

## Usage
```
var lockTimeout = TimeSpan.FromSeconds(15.0); // wait 15 seconds and then give up trying to get the lock
var retryDelay = TimeSpan.FromMilliseconds(100.0); // wait 100 milliseconds in between each attempt to get the lock
void exceptionNotifier(Exception ex)
{
	// log exception, etc...
}
// connectionMultiplexer comes from the StackExchange redis package.
var redisConnection = new RedisConnection(connectionMultiplexer);
var distributedLockOptions = new RedisDistributedLockProviderOptions(redisConnection,
	lockTimeout, retryDelay, exceptionNotifier);
var distributedLock = new RedisDistributedLockProvider(options);
var collapserPolicy = RequestCollapserPolicy.Create(lockProvider: distributedLock);
// use the policy as normal, see https://github.com/Polly-Contrib/Polly.Contrib.DuplicateRequestCollapser/blob/master/README.md
```

## Notes
To enable long paths, see https://www.ryadel.com/en/enable-ntfs-win32-long-paths-policy-remove-255-260-characters-limit-windows-10/

This project has long file names, if the above does not work, try moving to a C:/Code folder or something to reduce folder char size.