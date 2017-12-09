using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    partial class Json
    {
        class DynamicList : DynamicObject, IEnumerable<object>
        {
            private List<object> _array = new List<object>();

            public DynamicList(List<object> list)
            {
                _array = list;
            }

            #region Dynamic Implements
            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                if (int.TryParse(binder.Name, out int index))
                {
                    if (index < _array.Count)
                    {
                        result = _array[index];

                        return true;
                    }
                    else
                    {
                        result = null;
                    }
                }
                else
                {
                    result = null;
                }

                return false;
            }

            public override IEnumerable<string> GetDynamicMemberNames()
            {
                foreach (var i in Enumerable.Range(0, _array.Count))
                {
                    yield return i.ToString();
                }
            }
            #endregion

            #region Enumerable Implements
            public IEnumerator<object> GetEnumerator()
            {
                foreach (var item in _array)
                {
                    yield return item;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                foreach (var item in _array)
                {
                    yield return item;
                }
            }
            #endregion
        }
    }
}
