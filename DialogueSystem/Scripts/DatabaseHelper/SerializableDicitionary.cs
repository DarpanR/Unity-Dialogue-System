using UnityEngine;
using System.Linq;

namespace System.Collections.Generic {
    [Serializable]
    public class SerializableDicitionary<TKey, TValue> : ISerializationCallbackReceiver, IEnumerable {
        [SerializeField]
        List<TKey> _keys;
        [SerializeField]
        List<TValue> _values;

        public List<KeyValuePair<TKey,TValue>> kvpList;

        public int Count { get { return kvpList.Count; } }
        public List<TKey> Keys { get { return (from kvp in kvpList select kvp.Key).Distinct ().ToList (); } }
        public List<TValue> Values { get { return (from kvp in kvpList select kvp.Value).Distinct ().ToList (); } }

        public SerializableDicitionary () {
            kvpList = new List<KeyValuePair<TKey, TValue>> ();
        }

        public SerializableDicitionary (int capacity) {
            kvpList = new List<KeyValuePair<TKey, TValue>> (capacity);
        }

        public void Add (KeyValuePair<TKey,TValue> KVP) {
            if (kvpList.Contains (KVP))
                return;
            kvpList.Add (KVP);
        }

        public void Remove (TKey key) {
            if (!Keys.Contains (key))
                return;
            kvpList.RemoveAt (Keys.IndexOf (key));
        }

        public bool TryGetValue (TKey key, out TValue value) {
            if (!Keys.Contains (key)) {
                value = default (TValue);
                return false;
            }
            value = Values[Keys.IndexOf (key)];
            return true;
        }

        public void ChangeValue (KeyValuePair<TKey, TValue> KVP) {
            if (!kvpList.Contains (KVP))
                return;
            kvpList[Keys.IndexOf (KVP.Key)] = KVP;
        }

        public void OnBeforeSerialize () {
            _keys.Clear ();
            _values.Clear ();

            foreach (var kvp in kvpList) {
                _keys.Add (kvp.Key);
                _values.Add (kvp.Value);
            }
        }

        public void OnAfterDeserialize () {
            kvpList = new List<KeyValuePair<TKey, TValue>> ();

            for (int i = 0; i < _keys.Count; i++)
                kvpList.Add (new KeyValuePair<TKey, TValue> (_keys[i], _values[i]));
        }

        public IEnumerator GetEnumerator () {
            return kvpList.GetEnumerator ();
        }
    }
}
