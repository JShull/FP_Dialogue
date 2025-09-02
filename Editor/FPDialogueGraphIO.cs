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
            var blockToNode = new Dictionary<string, DialogueBlockNode>();

            foreach (var block in source.ConversationData)
            {
                var blockGuid = DialogueAssetUtil.GuidOf(block);
                var node = DialogueAssetUtil.CreateNode<DialogueBlockNode>(graph, block.name); 
                node.blockGuid = blockGuid;
                node.speaker = source.Character.name;
                node.line = block.OriginalLanguage.Text;

               

                node.TouchPorts();
                blockToNode[blockGuid] = node;
            }

            // Connect entry to the first block if you have a notion of "start"
            var startBlock = source.ConversationData.FirstOrDefault();
            if (startBlock != null && blockToNode.TryGetValue(DialogueAssetUtil.GuidOf(startBlock), out var firstNode))
            {
                DialogueAssetUtil.TryConnect(graph, entry.GetOutputPort(0), firstNode.GetInputPort(0));
            }

            // Build edges per response (if your DialogueResponse already points to the next block)
            foreach (var block in source.ConversationData)
            {
                var fromGuid = DialogueAssetUtil.GuidOf(block);
                if (!blockToNode.TryGetValue(fromGuid, out var fromNode)) continue;

                for (int i = 0; i < block.PossibleUserResponses.Count; i++)
                {
                    var resp = block.PossibleUserResponses[i];

                    // If your current Response has a strong link to the next block, use it.
                    // Common patterns tried below (adjust to your actual API):
                    //   1) resp.NextBlock (DialogueBlock)
                    //   2) resp.TargetBlock (DialogueBlock)
                    //   3) resp.Next (ScriptableObject derived)
                    DialogueBlock next = resp.NextBlock as DialogueBlock;

                    if (next == null)
                    {
                        // Migration case: response currently only "visually" references something
                        // Leave the output unconnected — author can wire in the graph
                        continue;
                    }

                    if (blockToNode.TryGetValue(DialogueAssetUtil.GuidOf(next), out var toNode))
                    {
                        var outPort = fromNode.GetOutputPort(i);
                        var inPort = toNode.GetInputPort(0);
                        if (outPort != null && inPort != null)
                        {
                            DialogueAssetUtil.TryConnect(graph, outPort, inPort);
                        }
                    }
                }
            }

            // Auto layout for readability
            FPDialogueGraphLayout.Grid(graph, xSpacing: 400f, ySpacing: 200f);

            GraphDatabase.SaveGraphIfDirty(graph);
        }

        // ------- Export: Graph => DialogueBase (writes ordering & links back)
        public static void ExportToSOs(FPDialogueGraph graph, DialogueBase target)
        {
            if (graph == null || target == null) return;

            // Map DialogueBlocks by GUID for quick lookup
            var blockMap = target.ConversationData
                .ToDictionary(DialogueAssetUtil.GuidOf, b => b);

            foreach (var node in graph.GetNodes().OfType<DialogueBlockNode>())
            {
                if (!blockMap.TryGetValue(node.blockGuid, out var block))
                    continue;

                // Overwrite optional fields
                //block.Speaker = node.speaker;
                //block.Line = node.line;

                // Ensure response list size matches the node outputs (order = port index)
                DialogueAssetUtil.ResizeResponses(block, node.responsesCount);

                for (int i = 0; i < node.responsesCount; i++)
                {

                    var resp = block.PossibleUserResponses[i];

                    //resp.ResponseText = slot.text;

                    // Resolve the connected target block
                    var outPort = node.GetOutputPort(i);
                    var targetPort = outPort?.firstConnectedPort;
                    if (targetPort == null)
                    {
                        // Unwired choice — leave NextBlock null
                        resp.NextBlock = null;
                        continue;
                    }

                    var toNode = targetPort.GetNode() as DialogueBlockNode;
                    if (toNode != null && blockMap.TryGetValue(toNode.blockGuid, out var toBlock))
                    {
                        resp.NextBlock = toBlock;
                    }
                    else
                    {
                        resp.NextBlock = null;
                    }
                }

                EditorUtility.SetDirty(block);
            }

            AssetDatabase.SaveAssets();
            GraphDatabase.SaveGraphIfDirty(graph);
        }
    }
}
