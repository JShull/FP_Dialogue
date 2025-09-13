namespace FuzzPhyte.Dialogue
{
    using UnityEngine;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Binder that holds and connects keys to runtime game objects for the graph dialogue
    /// manages when we access our runtime data index, and hunts that equivalent represented object down from our storage
    /// </summary>
    [ExecuteAlways]
    public class RTExposedBinder : MonoBehaviour,IExposedPropertyTable
    {
        [System.Serializable]struct FPBinding { public string id; public Object obj; }
        [SerializeField] List<FPBinding> bindings = new();

        readonly Dictionary<PropertyName, Object> map = new();

        public void SetReferenceValue(PropertyName id, Object value)
        {
            map[id] = value;
            var s = id.ToString();
            var i = bindings.FindIndex(b => b.id == s);
            if (i < 0) bindings.Add(new FPBinding { id = s, obj = value });
            else { var b = bindings[i]; b.obj = value; bindings[i] = b; }
        }
        public Object GetReferenceValue(PropertyName id, out bool idValid)
        {
            idValid = map.TryGetValue(id, out var v) ||
                  (map[id] = bindings.Find(b => b.id == id.ToString()).obj) != null;
            return map.TryGetValue(id, out var r) ? r : null;
        }
        public void ClearReferenceValue(PropertyName id)
        {
            map.Remove(id);
            var s = id.ToString();
            var i = bindings.FindIndex(b => b.id == s);
            if (i >= 0) bindings.RemoveAt(i);
        }
        /// <summary>
        /// Ergonomic lookup with string and object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool TryGet<T>(string id,out T obj) where T : Object
        {
            var o = this.GetReferenceValue(new PropertyName(id), out var ok);
            obj = ok ? o as T : null;
            return obj != null;
        }
    }
}
