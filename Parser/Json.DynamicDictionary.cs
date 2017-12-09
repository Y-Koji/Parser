using System.Collections;
using System.Collections.Generic;
using System.Dynamic;

namespace Parser
{
    partial class Json
    {
        private class DynamicDictionary : DynamicObject, IDictionary<string, object>
        {

            private Dictionary<string, object> _object;

            public DynamicDictionary(Dictionary<string, object> obj)
            {
                _object = obj;
            }

            #region Dynamic Implements
            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                if (_object.ContainsKey(binder.Name))
                {
                    result = _object[binder.Name];
                }
                else
                {
                    result = null;
                }

                return true;
            }

            public override IEnumerable<string> GetDynamicMemberNames()
            {
                foreach (var kvp in _object)
                {
                    yield return kvp.Key;
                }
            }
            #endregion

            #region Dictionary Implements
            public ICollection<string> Keys => _object.Keys;

            public ICollection<object> Values => _object.Values;

            public int Count => _object.Count;

            public bool IsReadOnly => true;

            public object this[string key]
            {
                get
                {
                    if (_object.ContainsKey(key))
                    {
                        return _object[key];
                    }
                    else
                    {
                        return null;
                    }
                }

                set
                {
                    if (!_object.ContainsKey(key))
                    {
                        _object.Add(key, value);
                    }
                }
            }

            public bool ContainsKey(string key)
            {
                return _object.ContainsKey(key);
            }

            public void Add(string key, object value)
            {
                _object.Add(key, value);
            }

            public bool Remove(string key)
            {
                if (_object.ContainsKey(key))
                {
                    _object.Remove(key);

                    return true;
                }

                return false;
            }

            public bool TryGetValue(string key, out object value)
            {
                return _object.TryGetValue(key, out value);
            }

            public void Add(KeyValuePair<string, object> item)
            {
                if (!_object.ContainsKey(item.Key))
                {
                    _object.Add(item.Key, item.Value);
                }
            }

            public void Clear()
            {
                _object.Clear();
            }

            public bool Contains(KeyValuePair<string, object> item)
            {
                return _object.ContainsKey(item.Key);
            }

            public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
            {
                if (arrayIndex < array.Length)
                {
                    if (!_object.ContainsKey(array[arrayIndex].Key))
                    {
                        _object.Add(array[arrayIndex].Key, array[arrayIndex].Value);
                    }
                }
            }

            public bool Remove(KeyValuePair<string, object> item)
            {
                if (_object.ContainsKey(item.Key))
                {
                    _object.Remove(item.Key);

                    return true;
                }

                return false;
            }

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                foreach (var item in _object)
                {
                    yield return item;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                foreach (var item in _object)
                {
                    yield return item;
                }
            }
            #endregion
        }
    }
}
