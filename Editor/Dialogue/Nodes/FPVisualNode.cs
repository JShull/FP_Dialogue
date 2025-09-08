namespace FuzzPhyte.Dialogue.Editor
{
    using System;
    using UnityEngine;
    using Unity.GraphToolkit.Editor;
    /// <summary>
    /// Base abstract class for all Dialogue Block Nodes
    /// </summary>
    [Serializable]
    internal abstract class FPVisualNode:Node
    {

        [SerializeField] protected string name;
        [SerializeField] public string Name { get { return name; } }
        public abstract void SetupIndex(string passedName);

        protected bool TryGetOptionValue<T>(string name, out T value)
        {
            var opt = GetNodeOptionByName(name);
            if (opt!=null && opt.TryGetValue<T>(out var v))
            {
                value = v;
                return true;
            }
            value = default;
            return false;
        }
        
    }
}
