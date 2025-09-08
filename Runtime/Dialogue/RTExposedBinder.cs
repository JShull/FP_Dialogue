namespace FuzzPhyte.Dialogue
{
    using UnityEngine;
    using System.Collections.Generic;
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
    }
}
