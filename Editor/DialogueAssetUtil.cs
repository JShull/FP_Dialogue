namespace FuzzPhyte.Dialogue.Editor
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using Unity.GraphToolkit.Editor;
    using System.Collections;
    using System.Reflection;
    using System;
    using System.Linq;

    public static class DialogueAssetUtil
    {
        #region Graph Based
        public static void ClearGraph(Graph graph)
        {
            if (graph == null) return;

            // Try to enumerate nodes
            var getNodes = graph.GetType().GetMethod("GetNodes", BindingFlags.Public | BindingFlags.Instance);
            if (getNodes == null)
            {
                Debug.LogWarning("[FPGraphCompat] GetNodes() not found on Graph; skipping ClearGraph.");
                return;
            }

            var nodesEnum = getNodes.Invoke(graph, null) as IEnumerable;
            if (nodesEnum == null)
            {
                Debug.LogWarning("[FPGraphCompat] GetNodes() didn’t return IEnumerable; skipping ClearGraph.");
                return;
            }

            // Snapshot nodes (avoid modifying while iterating)
            var nodes = new List<object>();
            foreach (var n in nodesEnum) nodes.Add(n);

            // Try common removal methods on Graph
            var remove1 = graph.GetType().GetMethod("RemoveNode", BindingFlags.Public | BindingFlags.Instance);
            var remove2 = graph.GetType().GetMethod("DeleteNode", BindingFlags.Public | BindingFlags.Instance);
            var remove3 = graph.GetType().GetMethod("Remove", BindingFlags.Public | BindingFlags.Instance);

            MethodInfo remover = remove1 ?? remove2 ?? remove3;

            if (remover == null || remover.GetParameters().Length != 1)
            {
                Debug.LogWarning("[FPGraphCompat] No public RemoveNode/DeleteNode/Remove(INode) on Graph; skipping ClearGraph.");
                return;
            }

            foreach (var n in nodes)
            {
                try { remover.Invoke(graph, new[] { n }); }
                catch (Exception e)
                {
                    Debug.LogWarning($"[FPGraphCompat] Failed to remove node {n}: {e.Message}");
                }
            }
        }
        /// <summary>
        /// Attempts to create a node of type T and attach it to the graph.
        /// Tries Graph.CreateNode<T>(string), Graph.CreateNode<T>(), Graph.AddNode<T>(), and
        /// as a last resort instantiates T and calls a possible AddNode(object) API.
        /// Returns null if nothing works.
        /// </summary>
        public static T CreateNode<T>(Graph graph, string name = null) where T : Node
        {
            if (graph == null) return null;

            // Try Graph.CreateNode<T>(string)
            var methods = graph.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                               .Where(m => m.IsGenericMethodDefinition)
                               .ToArray();

            var createWithName = methods.FirstOrDefault(m =>
                m.Name == "CreateNode" &&
                m.GetGenericArguments().Length == 1 &&
                m.GetParameters().Length == 1 &&
                m.GetParameters()[0].ParameterType == typeof(string));

            if (createWithName != null)
            {
                try
                {
                    var mi = createWithName.MakeGenericMethod(typeof(T));
                    var created = mi.Invoke(graph, new object[] { name ?? typeof(T).Name }) as T;
                    return created;
                }
                catch { /* fall through */ }
            }

            // Try Graph.CreateNode<T>()
            var createNoArgs = methods.FirstOrDefault(m =>
                m.Name == "CreateNode" &&
                m.GetGenericArguments().Length == 1 &&
                m.GetParameters().Length == 0);

            if (createNoArgs != null)
            {
                try
                {
                    var mi = createNoArgs.MakeGenericMethod(typeof(T));
                    var created = mi.Invoke(graph, null) as T;
                    if (created != null && !string.IsNullOrEmpty(name))
                    {
                        TrySetNodeTitle(created, name);
                    }
                    return created;
                }
                catch { /* fall through */ }
            }

            // Try Graph.AddNode<T>()
            var addNode = methods.FirstOrDefault(m =>
                m.Name == "AddNode" &&
                m.GetGenericArguments().Length == 1 &&
                m.GetParameters().Length == 0);

            if (addNode != null)
            {
                try
                {
                    var mi = addNode.MakeGenericMethod(typeof(T));
                    var created = mi.Invoke(graph, null) as T;
                    if (created != null && !string.IsNullOrEmpty(name))
                    {
                        TrySetNodeTitle(created, name);
                    }
                    return created;
                }
                catch { /* fall through */ }
            }

            // Last resort: new T(), then try Graph.AddNode(object)
            try
            {
                var instance = Activator.CreateInstance(typeof(T)) as T;
                if (instance != null)
                {
                    var addObj = graph.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                      .FirstOrDefault(m => m.Name == "AddNode" &&
                                                           m.GetParameters().Length == 1);
                    if (addObj != null)
                    {
                        addObj.Invoke(graph, new object[] { instance });
                        if (!string.IsNullOrEmpty(name))
                        {
                            TrySetNodeTitle(instance, name);
                        }
                        return instance;
                    }
                }
            }
            catch { /* fall through */ }

            Debug.LogWarning($"[FPGraphCompat] Could not find a supported node-creation API for {typeof(T).Name}. Create it via the graph UI instead.");
            return null;
        }
        private static void TrySetNodeTitle(Node node, string title)
        {
            if (node == null || string.IsNullOrEmpty(title)) return;

            // Try common name/title fields or properties via reflection
            var t = node.GetType();
            // properties named "title" or "Title" or "displayName" (ignore case)
            var prop = t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .FirstOrDefault(p => p.PropertyType == typeof(string) &&
                                             p.CanWrite &&
                                             string.Equals(p.Name, "title", StringComparison.OrdinalIgnoreCase) ||
                                             string.Equals(p.Name, "displayName", StringComparison.OrdinalIgnoreCase));
            if (prop != null)
            {
                try { prop.SetValue(node, title); return; } catch { /* ignore */ }
            }

            // fields named "title"/"Title"/"displayName"
            var field = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                         .FirstOrDefault(f => f.FieldType == typeof(string) &&
                                              (string.Equals(f.Name, "title", StringComparison.OrdinalIgnoreCase) ||
                                               string.Equals(f.Name, "displayName", StringComparison.OrdinalIgnoreCase) ||
                                               string.Equals(f.Name, "nodeTitle", StringComparison.OrdinalIgnoreCase)));
            if (field != null)
            {
                try { field.SetValue(node, title); return; } catch { /* ignore */ }
            }

            // If none exist, no problem — the node just won’t display a header text from here.
        }

        /// <summary>
        /// Attempts to connect two ports. Uses Graph.Connect(IPort, IPort) if available,
        /// otherwise tries IPort.ConnectTo(IPort).
        /// </summary>
        public static bool TryConnect(Graph graph, IPort from, IPort to)
        {
            if (graph == null || from == null || to == null) return false;

            // Try Graph.Connect(IPort, IPort)
            var connect = graph.GetType().GetMethod("Connect", BindingFlags.Public | BindingFlags.Instance);
            if (connect != null && connect.GetParameters().Length == 2)
            {
                try
                {
                    connect.Invoke(graph, new object[] { from, to });
                    return true;
                }
                catch { /* fall through */ }
            }

            // Try IPort.ConnectTo(IPort)
            var m = from.GetType().GetMethod("ConnectTo", BindingFlags.Public | BindingFlags.Instance);
            if (m != null)
            {
                try
                {
                    m.Invoke(from, new object[] { to });
                    return true;
                }
                catch { /* fall through */ }
            }

            Debug.LogWarning("[FPGraphCompat] No supported connect API found.");
            return false;
        }
        #endregion

        #region Dialogue Based
        public static string GuidOf(UnityEngine.Object obj)
        {
            if (obj == null) return string.Empty;
            var path = AssetDatabase.GetAssetPath(obj);
            return string.IsNullOrEmpty(path) ? string.Empty : AssetDatabase.AssetPathToGUID(path);
        }

        public static DialogueBase LoadDialogueBaseByGuid(string guid)
        {
            if (string.IsNullOrEmpty(guid)) return null;
            var path = AssetDatabase.GUIDToAssetPath(guid);
            return AssetDatabase.LoadAssetAtPath<DialogueBase>(path);
        }

        public static void PingByGuid(string guid)
        {
            if (string.IsNullOrEmpty(guid)) return;
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            if (obj) EditorGUIUtility.PingObject(obj);
        }

        public static string NewTempGuid() => System.Guid.NewGuid().ToString("N");

        public static void ResizeResponses(DialogueBlock block, int count)
        {
            if (block.PossibleUserResponses == null)
                block.PossibleUserResponses = new List<DialogueResponse>();

            while (block.PossibleUserResponses.Count < count)
            {
                var resp = ScriptableObject.CreateInstance<DialogueResponse>();
                resp.name = $"{block.name}_Resp_{block.PossibleUserResponses.Count + 1}";
                AssetDatabase.AddObjectToAsset(resp, block); // sub-asset to keep things tidy
                block.PossibleUserResponses.Add(resp);
            }

            while (block.PossibleUserResponses.Count > count)
            {
                var last = block.PossibleUserResponses[^1];
                block.PossibleUserResponses.RemoveAt(block.PossibleUserResponses.Count - 1);
                UnityEngine.Object.DestroyImmediate(last, allowDestroyingAssets: true);
            }
        }
        #endregion
    }
}
