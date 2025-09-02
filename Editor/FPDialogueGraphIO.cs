namespace FuzzPhyte.Dialogue.Editor
{
    using UnityEngine;
    using UnityEditor;
    using System.Linq;
    using Unity.GraphToolkit.Editor;
    using System.Collections.Generic;
    public static class FPDialogueGraphIO
    {
        // Create or open a graph asset at path, stamp its DialogueBase GUID
        public static FPDialogueGraph CreateOrLoadGraph(string graphAssetPath, DialogueBase source)
        {
            var graph = GraphDatabase.LoadGraph<FPDialogueGraph>(graphAssetPath);
            if (graph == null)
                graph = GraphDatabase.CreateGraph<FPDialogueGraph>(graphAssetPath);

            graph.dialogueBaseGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(source));
            GraphDatabase.SaveGraphIfDirty(graph);
            return graph;
        }

        // ------- Import: DialogueBase => Graph (handles list-in-list -> edges)
        public static void ImportFrom(DialogueBase source, FPDialogueGraph graph)
        {
            if (source == null || graph == null) return;

            // Basic clear (optional: soft-clear & reconcile)
            DialogueAssetUtil.ClearGraph(graph);

            // Build nodes
           
            var entry = DialogueAssetUtil.CreateNode<EntryNode>(graph, "Start");
            var blockToNode = new Dictionary<string, FPVisualNode>();

            GraphDatabase.SaveGraphIfDirty(graph);
        }

        // ------- Export: Graph => DialogueBase (writes ordering & links back)
        public static void ExportToSOs(FPDialogueGraph graph, DialogueBase target)
        {
            if (graph == null || target == null) return;

            // Map DialogueBlocks by GUID for quick lookup
            var blockMap = target.ConversationData
                .ToDictionary(DialogueAssetUtil.GuidOf, b => b);

            foreach (var node in graph.GetNodes().OfType<FPVisualNode>())
            {
                if (!blockMap.TryGetValue(node.name, out var block))
                    continue;

                EditorUtility.SetDirty(block);
            }

            AssetDatabase.SaveAssets();
            GraphDatabase.SaveGraphIfDirty(graph);
        }
    }
}
