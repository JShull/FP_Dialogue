// Copyright (c) 2026 John B. Shull
// FuzzPhyte LLC is a company associated with John B. Shull
// This file is part of FP_Dialogue Package.
//
// Public license: GNU GPLv3-or-later.
// Commercial/proprietary use requires a separate license from John B. Shull.
//
// See LICENSE.md COMMERCIAL-LICENSE.md, and NOTICE.md.

namespace FuzzPhyte.Dialogue
{
    using UnityEngine;
    using System.Collections.Generic;
    using System.Globalization;

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
            var i = bindings.FindIndex(b => Matches(b.id, id));
            if (i < 0) bindings.Add(new FPBinding { id = s, obj = value });
            else { var b = bindings[i]; b.obj = value; bindings[i] = b; }
        }
        public Object GetReferenceValue(PropertyName id, out bool idValid)
        {
            if (map.TryGetValue(id, out var value))
            {
                idValid = value != null;
                return value;
            }
            foreach (var binding in bindings)
            {
                if (!Matches(binding.id, id)) continue;
                map[id] = binding.obj;
                idValid = binding.obj != null;
                return binding.obj;
            }
            // Do not cache a miss as a valid dictionary entry on the next lookup.
            idValid = false;
            return null;
        }
        public void ClearReferenceValue(PropertyName id)
        {
            map.Remove(id);
            var i = bindings.FindIndex(b => Matches(b.id, id));
            if (i >= 0) bindings.RemoveAt(i);
        }

        private static bool Matches(string stored, PropertyName id)
        {
            if (string.IsNullOrEmpty(stored)) return false;
            // Older scenes store PropertyName.ToString(): "Name:numericId".
            // Players may return "Unknown:numericId" without the Editor reverse-name table.
            // Compare the actual identity, keeping those existing serialized bindings intact.
            var separator = stored.LastIndexOf(':');
            if (separator >= 0 && int.TryParse(stored.Substring(separator + 1),
                NumberStyles.Integer, CultureInfo.InvariantCulture, out var numericId))
            {
                return new PropertyName(numericId) == id;
            }
            return new PropertyName(stored) == id;
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
