namespace FuzzPhyte.Dialogue.Editor
{
    using System;
    using System.Linq;
    using UnityEditor;
    using Unity.GraphToolkit.Editor;

    public static class FPDialogueGraphCommands
    {
        public static void ProcessNodeActions(FPDialogueGraph graph)
        {
            foreach (var node in graph.GetNodes().OfType<DialogueBlockNode>())
            {
                if (node.action == NodeAction.None)
                {
                    continue;
                }
                switch (node.action)
                {
                    case NodeAction.AddResponse:
                    case NodeAction.RemoveSelected:
                    case NodeAction.MoveUp:
                    case NodeAction.MoveDown:
                    case NodeAction.ApplySelectedText:
                    case NodeAction.RefreshPorts:
                    case NodeAction.PingBackingAssets:
                        node.TouchPorts();
                        node.SyncDerivedUIFields();
                        GraphDatabase.SaveGraphIfDirty(graph);
                        break;
                }

                // Reset the action so it behaves like a one-shot command
                node.action = NodeAction.None;
            }
        }
    }
}
