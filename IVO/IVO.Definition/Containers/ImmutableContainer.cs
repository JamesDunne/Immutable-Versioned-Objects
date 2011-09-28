using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IVO.Definition.Containers
{
    public sealed class ImmutableContainer<K,V> : IEnumerable<KeyValuePair<K, V>>
        where K : struct
        where V : class
    {
        private Dictionary<K, V> _container;

        private ImmutableContainer(Dictionary<K, V> items)
        {
            _container = items;
        }

        public ImmutableContainer(Func<V, K> keySelector, IEqualityComparer<K> comparer, params V[] items)
        {
            construct(keySelector, comparer, items);
        }

        public ImmutableContainer(Func<V, K> keySelector, params V[] items)
        {
            construct(keySelector, EqualityComparer<K>.Default, items);
        }

        public ImmutableContainer(Func<V, K> keySelector, IEqualityComparer<K> comparer, IEnumerable<V> items)
        {
            construct(keySelector, comparer, items.ToList());
        }

        public ImmutableContainer(Func<V, K> keySelector, IEnumerable<V> items)
        {
            construct(keySelector, EqualityComparer<K>.Default, items.ToList());
        }

        public ImmutableContainer(IDictionary<K, V> items, IEqualityComparer<K> comparer)
        {
            _container = new Dictionary<K, V>(items, comparer);
        }

        public ImmutableContainer(IDictionary<K, V> items)
        {
            _container = new Dictionary<K, V>(items);
        }

        private void construct(Func<V, K> keySelector, IEqualityComparer<K> comparer, IList<V> items)
        {
            _container = new Dictionary<K,V>(items.Count, comparer);
            foreach (V value in items)
            {
                K key = keySelector(value);
                if (_container.ContainsKey(key)) continue;

                _container.Add(key, value);
            }
        }

        public V this[K id]
        {
            get { return _container[id]; }
        }

        public bool ContainsKey(K id)
        {
            return _container.ContainsKey(id);
        }

        public bool TryGetValue(K id, out V value)
        {
            return _container.TryGetValue(id, out value);
        }

        /// <summary>
        /// Try to get a value given a key, returned in a Maybe structure.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Maybe<V> MaybeGet(K id)
        {
            V value;
            
            if (_container.TryGetValue(id, out value))
                return new Maybe<V>(value);

            return Maybe<V>.Nothing;
        }

        /// <summary>
        /// Creates a new ImmutableContainer that merges this container's contents with the
        /// <paramref name="other"/> container's contents, using this container's equality comparer.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public ImmutableContainer<K, V> MergeWith(ImmutableContainer<K, V> other)
        {
            Dictionary<K, V> newContainer = new Dictionary<K, V>(this._container, this._container.Comparer);

            foreach (KeyValuePair<K, V> pair in other._container)
            {
                if (this.ContainsKey(pair.Key)) continue;
                newContainer.Add(pair.Key, pair.Value);
            }

            return new ImmutableContainer<K, V>(newContainer);
        }

        /// <summary>
        /// Creates a new ImmutableContainer that contains this container's elements plus the new KeyValuePairs.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public ImmutableContainer<K, V> AddRange(params KeyValuePair<K, V>[] elements)
        {
            Dictionary<K, V> newContainer = new Dictionary<K, V>(this._container, this._container.Comparer);

            foreach (KeyValuePair<K, V> pair in elements)
            {
                if (this.ContainsKey(pair.Key)) continue;
                newContainer.Add(pair.Key, pair.Value);
            }

            return new ImmutableContainer<K, V>(newContainer);
        }

        public int Count { get { return _container.Count; } }

        public IEnumerable<K> Keys { get { return _container.Keys; } }

        public IEnumerable<V> Values { get { return _container.Values; } }

        #region IEnumerable<KeyValuePair<K,V>> Members

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return _container.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _container.GetEnumerator();
        }

        #endregion
    }
}
