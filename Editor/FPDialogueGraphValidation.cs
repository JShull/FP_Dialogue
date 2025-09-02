namespace FuzzPhyte.Dialogue.Editor
{
    using System.Linq;
    using UnityEngine;
    using Unity.GraphToolkit.Editor;
    public static class FPDialogueGraphValidation
    {
        public static void Run(FPDialogueGraph graph, GraphLogger logger)
        {
            // 1) Exactly one EntryNode
            var entries = graph.GetNodes().OfType<EntryNode>().ToList();
            if (entries.Count == 0)
                logger.LogError("No EntryNode in graph.", graph);
            else if (entries.Count > 1)
                logger.LogWarning("Multiple EntryNodes found. Only one is recommended.", graph);

            // 2) Every DialogueBlockNode must have input connected
            foreach (var n in graph.GetNodes().OfType<DialogueBlockNode>())
            {
                var inPort = n.GetInputPort(0);
                if (inPort == null || !inPort.isConnected)
                {
                    logger.LogWarning($"'{n.blockGuid}' has no incoming connection.", n);
                }

                // 3) Warn for unconnected outputs
                var count = n.responsesCount;
                for (int i = 0; i < count; i++)
                {
                    var outPort = n.GetOutputPort(i);
                    if (outPort == null || !outPort.isConnected)
                    {
                        logger.Log($"'{n.blockGuid}' → response {i + 1} is not connected.", n);
                    }
                }
            }
        }
    }
}
