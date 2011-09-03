using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IVO.Definition.Containers
{
    public sealed class ImmutableContainer<K,V>
        where K : struct
        where V : class
    {
        private Dictionary<K, V> _container;

        public ImmutableContainer(Func<V, K> keySelector, params V[] items)
        {
            _container = items.ToDictionary(e => keySelector(e));
        }

        public ImmutableContainer(Func<V, K> keySelector, IEnumerable<V> items)
        {
            _container = items.ToDictionary(e => keySelector(e));
        }

        public ImmutableContainer(IDictionary<K, V> items)
        {
            _container = new Dictionary<K, V>(items);
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

        public Maybe<V> MaybeGet(K id)
        {
            V value;
            
            if (_container.TryGetValue(id, out value))
                return new Maybe<V>(value);

            return Maybe<V>.Nothing;
        }

        public int Count { get { return _container.Count; } }

        public IEnumerable<K> Keys { get { return _container.Keys; } }

        public IEnumerable<V> Values { get { return _container.Values; } }
    }
}
