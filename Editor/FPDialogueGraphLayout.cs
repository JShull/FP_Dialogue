namespace FuzzPhyte.Dialogue.Editor
{
    using System.Collections.Generic;
    using UnityEngine;
    using Unity.GraphToolkit.Editor;
    using System.Linq;

    public static class FPDialogueGraphLayout
    {
        // Simple grid layout; replace with your own BFS layout if you prefer
        public static void Grid(FPDialogueGraph graph, float xSpacing = 350f, float ySpacing = 200f)
        {
            var nodes = graph.GetNodes();
            int cols = Mathf.CeilToInt(Mathf.Sqrt(nodes.Count<INode>()));
            int x = 0, y = 0;
            
            foreach (var n in nodes)
            {
                // If Graph Toolkit exposes a transform/position, set it here.
                // Otherwise keep a serialized "editorPosition" option on nodes.
                var nn= n as Node;
                nn.SetEditorPosition(new Vector2(x * xSpacing, y * ySpacing));

                x++;
                if (x >= cols) { x = 0; y++; }
            }
            
        }
    }
    internal static class GraphNodePositionExtensions
    {
        // Fallback serialized position if toolkit doesn't expose direct transforms
        public static void SetEditorPosition(this Node node, Vector2 pos)
        {
            // Store as hidden options to keep example self-contained
            
            //node.Set("EditorPosX", pos.x);
            //node.SetOption("EditorPosY", pos.y);
        }
    }
}
