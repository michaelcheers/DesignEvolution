using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignEvolution
{
    public class EnumArray<TKey, TValue> : IDictionary<TKey, TValue>
    {
        TValue[] _internal;

        public EnumArray() => _internal = new TValue[Enum.GetValues(typeof(TKey)).Length];

        public TValue this[TKey key] { get => _internal[Convert.ToInt32(key)]; set => _internal[Convert.ToInt32(key)] = value; }

        public ICollection<TKey> Keys => Array.ConvertAll(Enumerable.Range(0, _internal.Length).ToArray(), v => (TKey)Enum.ToObject(typeof(TKey), v));

        public ICollection<TValue> Values => _internal;

        public int Count => _internal.Length;

        public bool IsReadOnly => false;

        public void Add(TKey key, TValue value) => this[key] = value;

        public void Add(KeyValuePair<TKey, TValue> item) => this[item.Key] = item.Value;

        void ICollection<KeyValuePair<TKey, TValue>>.Clear() => throw new NotImplementedException();

        public bool Contains(KeyValuePair<TKey, TValue> item) => Equals(this[item.Key], item.Value);

        public bool ContainsKey(TKey key) => true;

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            for (int n = 0; n < _internal.Length; n++)
            {
                array[n + arrayIndex] = new KeyValuePair<TKey, TValue>((TKey)Enum.ToObject(typeof(TKey), n), _internal[n]);
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            KeyValuePair<TKey, TValue>[] r = new KeyValuePair<TKey, TValue>[_internal.Length];
            CopyTo(r, 0);
            return ((ICollection<KeyValuePair<TKey, TValue>>)r).GetEnumerator();
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) => throw new NotImplementedException();

        bool IDictionary<TKey, TValue>.Remove(TKey item) => throw new NotImplementedException();

        public bool TryGetValue(TKey key, out TValue value)
        {
            value = this[key];
            return true;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
