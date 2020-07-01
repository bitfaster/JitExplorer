namespace Testing
{
    using System.Collections.Concurrent;
    using System;
    using System.Threading;
    using System.Runtime.CompilerServices;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Reflection;

    public class Program
    {
        public static void Main(string[] args)
        {
           // int i = 0;

            var lru = new ConcurrentLru<int, string>(5);
            //int c = lru.ColdCount;

            for (int i = 0; i < 100000; i++)
            {
                var v = lru.TryGet(i, out var x);
            }

            // try preparing all methods:
            foreach (var method in lru.GetType().GetMethods(BindingFlags.DeclaredOnly |
                        BindingFlags.NonPublic |
                        BindingFlags.Public | BindingFlags.Instance |
                        BindingFlags.Static))
            {
                System.Runtime.CompilerServices.RuntimeHelpers.PrepareMethod(method.MethodHandle);
            }

            for (int i = 0; i < 100000; i++)
            {
                var v = lru.TryGet(i, out var x); 
            }

            JitExplorer.Signal.__Jit();
            //System.Threading.Thread.Sleep(1000);
        }
    }

    public sealed class ConcurrentLru<K, V> : TemplateConcurrentLru<K, V, LruItem<K, V>, LruPolicy<K, V>, HitCounter>
    {
        /// <summary>
        /// Initializes a new instance of the ConcurrentLru class with the specified capacity that has the default 
        /// concurrency level, and uses the default comparer for the key type.
        /// </summary>
        /// <param name="capacity">The maximum number of elements that the ConcurrentLru can contain.</param>
        public ConcurrentLru(int capacity)
            : base(8, capacity, EqualityComparer<K>.Default, new LruPolicy<K, V>(), new HitCounter())
        {
        }

        /// <summary>
        /// Initializes a new instance of the ConcurrentLru class that has the specified concurrency level, has the 
        /// specified initial capacity, and uses the specified IEqualityComparer<T>.
        /// </summary>
        /// <param name="concurrencyLevel">The estimated number of threads that will update the ConcurrentLru concurrently.</param>
        /// <param name="capacity">The maximum number of elements that the ConcurrentLru can contain.</param>
        /// <param name="comparer">The IEqualityComparer<T> implementation to use when comparing keys.</param>
        public ConcurrentLru(int concurrencyLevel, int capacity, IEqualityComparer<K> comparer)
            : base(concurrencyLevel, capacity, comparer, new LruPolicy<K, V>(), new HitCounter())
        {
        }

        /// <summary>
        /// Gets the ratio of hits to misses, where a value of 1 indicates 100% hits.
        /// </summary>
        public double HitRatio => this.hitCounter.HitRatio;
    }

    //[JitGeneric(typeof(int), typeof(string), typeof(LruItem<int, string>), typeof(LruPolicy<int, string>), typeof(HitCounter))]
    public class TemplateConcurrentLru<K, V, I, P, H> : ICache<K, V>
            where I : LruItem<K, V>
            where P : struct, IPolicy<K, V, I>
            where H : struct, IHitCounter
    {
        private readonly ConcurrentDictionary<K, I> dictionary;

        private readonly ConcurrentQueue<I> hotQueue;
        private readonly ConcurrentQueue<I> warmQueue;
        private readonly ConcurrentQueue<I> coldQueue;

        // maintain count outside ConcurrentQueue, since ConcurrentQueue.Count holds a global lock
        private int hotCount;
        private int warmCount;
        private int coldCount;

        private readonly int hotCapacity;
        private readonly int warmCapacity;
        private readonly int coldCapacity;

        private readonly P policy;

        // Since H is a struct, making it readonly will force the runtime to make defensive copies
        // if mutate methods are called. Therefore, field must be mutable to maintain count.
        protected H hitCounter;

        public TemplateConcurrentLru(
            int concurrencyLevel,
            int capacity,
            IEqualityComparer<K> comparer,
            P itemPolicy,
            H hitCounter)
        {
            if (capacity < 3)
            {
                throw new ArgumentOutOfRangeException("Capacity must be greater than or equal to 3.");
            }

            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer));
            }

            this.hotCapacity = capacity / 3;
            this.warmCapacity = capacity / 3;
            this.coldCapacity = capacity / 3;

            this.hotQueue = new ConcurrentQueue<I>();
            this.warmQueue = new ConcurrentQueue<I>();
            this.coldQueue = new ConcurrentQueue<I>();

            int dictionaryCapacity = this.hotCapacity + this.warmCapacity + this.coldCapacity + 1;

            this.dictionary = new ConcurrentDictionary<K, I>(concurrencyLevel, dictionaryCapacity, comparer);
            this.policy = itemPolicy;
            this.hitCounter = hitCounter;
        }

        public int Count => this.dictionary.Count;

        public int HotCount => this.hotCount;

        public int WarmCount => this.warmCount;

        public int ColdCount => this.coldCount;

        ///<inheritdoc/>
        public bool TryGet(K key, out V value)
        {
            I item;
            if (dictionary.TryGetValue(key, out item))
            {
                if (this.policy.ShouldDiscard(item))
                {
                    this.Move(item, ItemDestination.Remove);
                    value = default(V);
                    return false;
                }

                value = item.Value;
                this.policy.Touch(item);
                this.hitCounter.IncrementHit();
                return true;
            }

            value = default(V);
            this.hitCounter.IncrementMiss();
            return false;
        }

        ///<inheritdoc/>

        public V GetOrAdd(K key, Func<K, V> valueFactory)
        {
            if (this.TryGet(key, out var value))
            {
                return value;
            }

            // The value factory may be called concurrently for the same key, but the first write to the dictionary wins.
            // This is identical logic in ConcurrentDictionary.GetOrAdd method.
            var newItem = this.policy.CreateItem(key, valueFactory(key));

            if (this.dictionary.TryAdd(key, newItem))
            {
                this.hotQueue.Enqueue(newItem);
                Interlocked.Increment(ref hotCount);
                Cycle();
                return newItem.Value;
            }

            return this.GetOrAdd(key, valueFactory);
        }

        ///<inheritdoc/>
        public async Task<V> GetOrAddAsync(K key, Func<K, Task<V>> valueFactory)
        {
            if (this.TryGet(key, out var value))
            {
                return value;
            }

            // The value factory may be called concurrently for the same key, but the first write to the dictionary wins.
            // This is identical logic in ConcurrentDictionary.GetOrAdd method.
            var newItem = this.policy.CreateItem(key, await valueFactory(key).ConfigureAwait(false));

            if (this.dictionary.TryAdd(key, newItem))
            {
                this.hotQueue.Enqueue(newItem);
                Interlocked.Increment(ref hotCount);
                Cycle();
                return newItem.Value;
            }

            return await this.GetOrAddAsync(key, valueFactory).ConfigureAwait(false);
        }

        ///<inheritdoc/>
        public bool TryRemove(K key)
        {
            // Possible race condition:
            // Thread A TryRemove(1), removes LruItem1, has reference to removed item but not yet marked as removed
            // Thread B GetOrAdd(1) => Adds LruItem1*
            // Thread C GetOrAdd(2), Cycle, Move(LruItem1, Removed)
            // 
            // Thread C can run and remove LruItem1* from this.dictionary before Thread A has marked LruItem1 as removed.
            // 
            // In this situation, a subsequent attempt to fetch 1 will be a miss. The queues will still contain LruItem1*, 
            // and it will not be marked as removed. If key 1 is fetched while LruItem1* is still in the queue, there will 
            // be two queue entries for key 1, and neither is marked as removed. Thus when LruItem1 * ages out, it will  
            // incorrectly remove 1 from the dictionary, and this cycle can repeat.
            if (this.dictionary.TryGetValue(key, out var existing))
            {
                if (existing.WasRemoved)
                {
                    return false;
                }

                lock (existing)
                {
                    if (existing.WasRemoved)
                    {
                        return false;
                    }

                    existing.WasRemoved = true;
                }

                if (this.dictionary.TryRemove(key, out var removedItem))
                {
                    // Mark as not accessed, it will later be cycled out of the queues because it can never be fetched 
                    // from the dictionary. Note: Hot/Warm/Cold count will reflect the removed item until it is cycled 
                    // from the queue.
                    removedItem.WasAccessed = false;

                    if (removedItem.Value is IDisposable d)
                    {
                        d.Dispose();
                    }

                    return true;
                }
            }

            return false;
        }

        private void Cycle()
        {
            // There will be races when queue count == queue capacity. Two threads may each dequeue items.
            // This will prematurely free slots for the next caller. Each thread will still only cycle at most 5 items.
            // Since TryDequeue is thread safe, only 1 thread can dequeue each item. Thus counts and queue state will always
            // converge on correct over time.
            CycleHot();

            // Multi-threaded stress tests show that due to races, the warm and cold count can increase beyond capacity when
            // hit rate is very high. Double cycle results in stable count under all conditions. When contention is low, 
            // secondary cycles have no effect.
            CycleWarm();
            CycleWarm();
            CycleCold();
            CycleCold();
        }

        private void CycleHot()
        {
            if (this.hotCount > this.hotCapacity)
            {
                Interlocked.Decrement(ref this.hotCount);

                if (this.hotQueue.TryDequeue(out var item))
                {
                    var where = this.policy.RouteHot(item);
                    this.Move(item, where);
                }
                else
                {
                    Interlocked.Increment(ref this.hotCount);
                }
            }
        }

        private void CycleWarm()
        {
            if (this.warmCount > this.warmCapacity)
            {
                Interlocked.Decrement(ref this.warmCount);

                if (this.warmQueue.TryDequeue(out var item))
                {
                    var where = this.policy.RouteWarm(item);

                    // When the warm queue is full, we allow an overflow of 1 item before redirecting warm items to cold.
                    // This only happens when hit rate is high, in which case we can consider all items relatively equal in
                    // terms of which was least recently used.
                    if (where == ItemDestination.Warm && this.warmCount <= this.warmCapacity)
                    {
                        this.Move(item, where);
                    }
                    else
                    {
                        this.Move(item, ItemDestination.Cold);
                    }
                }
                else
                {
                    Interlocked.Increment(ref this.warmCount);
                }
            }
        }

        private void CycleCold()
        {
            if (this.coldCount > this.coldCapacity)
            {
                Interlocked.Decrement(ref this.coldCount);

                if (this.coldQueue.TryDequeue(out var item))
                {
                    var where = this.policy.RouteCold(item);

                    if (where == ItemDestination.Warm && this.warmCount <= this.warmCapacity)
                    {
                        this.Move(item, where);
                    }
                    else
                    {
                        this.Move(item, ItemDestination.Remove);
                    }
                }
                else
                {
                    Interlocked.Increment(ref this.coldCount);
                }
            }
        }

        private void Move(I item, ItemDestination where)
        {
            item.WasAccessed = false;

            switch (where)
            {
                case ItemDestination.Warm:
                    this.warmQueue.Enqueue(item);
                    Interlocked.Increment(ref this.warmCount);
                    break;
                case ItemDestination.Cold:
                    this.coldQueue.Enqueue(item);
                    Interlocked.Increment(ref this.coldCount);
                    break;
                case ItemDestination.Remove:
                    if (!item.WasRemoved)
                    {
                        // avoid race where 2 threads could remove the same key - see TryRemove for details.
                        lock (item)
                        {
                            if (item.WasRemoved)
                            {
                                break;
                            }

                            if (this.dictionary.TryRemove(item.Key, out var removedItem))
                            {
                                item.WasRemoved = true;
                                if (removedItem.Value is IDisposable d)
                                {
                                    d.Dispose();
                                }
                            }
                        }
                    }
                    break;
            }
        }
    }

    public interface ICache<K, V>
    {
        /// <summary>
        /// Attempts to get the value associated with the specified key from the cache.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">When this method returns, contains the object from the cache that has the specified key, or the default value of the type if the operation failed.</param>
        /// <returns>true if the key was found in the cache; otherwise, false.</returns>
        bool TryGet(K key, out V value);

        /// <summary>
        /// Adds a key/value pair to the cache if the key does not already exist. Returns the new value, or the 
        /// existing value if the key already exists.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="valueFactory">The factory function used to generate a value for the key.</param>
        /// <returns>The value for the key. This will be either the existing value for the key if the key is already 
        /// in the cache, or the new value if the key was not in the dictionary.</returns>
        V GetOrAdd(K key, Func<K, V> valueFactory);

        /// <summary>
        /// Adds a key/value pair to the cache if the key does not already exist. Returns the new value, or the 
        /// existing value if the key already exists.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="valueFactory">The factory function used to asynchronously generate a value for the key.</param>
        /// <returns>A task that represents the asynchronous GetOrAdd operation.</returns>
        Task<V> GetOrAddAsync(K key, Func<K, Task<V>> valueFactory);

        /// <summary>
        /// Attempts to remove and return the value that has the specified key from the cache.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>true if the object was removed successfully; otherwise, false.</returns>
        bool TryRemove(K key);
    }

    public class LruItem<K, V>
    {
        private volatile bool wasAccessed;
        private volatile bool wasRemoved;

        public LruItem(K k, V v)
        {
            this.Key = k;
            this.Value = v;
        }

        public readonly K Key;

        public readonly V Value;

        public bool WasAccessed
        {
            get => this.wasAccessed;
            set => this.wasAccessed = value;
        }

        public bool WasRemoved
        {
            get => this.wasRemoved;
            set => this.wasRemoved = value;
        }
    }

    public readonly struct LruPolicy<K, V> : IPolicy<K, V, LruItem<K, V>>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LruItem<K, V> CreateItem(K key, V value)
        {
            return new LruItem<K, V>(key, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Touch(LruItem<K, V> item)
        {
            item.WasAccessed = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ShouldDiscard(LruItem<K, V> item)
        {
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemDestination RouteHot(LruItem<K, V> item)
        {
            if (item.WasAccessed)
            {
                return ItemDestination.Warm;
            }

            return ItemDestination.Cold;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemDestination RouteWarm(LruItem<K, V> item)
        {
            if (item.WasAccessed)
            {
                return ItemDestination.Warm;
            }

            return ItemDestination.Cold;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemDestination RouteCold(LruItem<K, V> item)
        {
            if (item.WasAccessed)
            {
                return ItemDestination.Warm;
            }

            return ItemDestination.Remove;
        }
    }

    public struct HitCounter : IHitCounter
    {
        private long hitCount;
        private long missCount;

        public double HitRatio => Total == 0 ? 0 : (double)hitCount / (double)Total;

        public long Total => this.hitCount + this.missCount;



        public void IncrementHit()
        {
            Interlocked.Increment(ref this.hitCount);
        }

        public void IncrementMiss()
        {
            Interlocked.Increment(ref this.missCount);
        }
    }

    public interface IHitCounter
    {
        void IncrementMiss();

        void IncrementHit();

        double HitRatio { get; }
    }

    public interface IPolicy<in K, in V, I> where I : LruItem<K, V>
    {
        I CreateItem(K key, V value);

        void Touch(I item);

        bool ShouldDiscard(I item);

        ItemDestination RouteHot(I item);

        ItemDestination RouteWarm(I item);

        ItemDestination RouteCold(I item);
    }

    public enum ItemDestination
    {
        Warm,
        Cold,
        Remove
    }
}

