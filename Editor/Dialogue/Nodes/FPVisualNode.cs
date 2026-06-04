// Copyright (c) 2026 John B. Shull
// FuzzPhyte LLC is a company associated with John B. Shull
// This file is part of FP_Dialogue Package.
//
// Public license: GNU GPLv3-or-later.
// Commercial/proprietary use requires a separate license from John B. Shull.
//
// See LICENSE.md COMMERCIAL-LICENSE.md, and NOTICE.md.

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
        public string Name { get { return name; } }
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
