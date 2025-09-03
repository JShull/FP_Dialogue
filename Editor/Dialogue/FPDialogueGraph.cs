namespace FuzzPhyte.Dialogue.Editor
{
    using System.Collections.Generic;
    using Unity.GraphToolkit.Editor;
    using UnityEngine;
    using System;
    using FuzzPhyte.Utility;
    using System.Linq;
    using UnityEngine.Timeline;

    // Control-flow phantom type for port compatibility

    [Graph(AssetExtension, GraphOptions.SupportsSubgraphs)]
    public class FPDialogueGraph:Graph
    {
        const string k_graphName = "Dialogue Graph";
        public const string AssetExtension = "fpdialogue";
        // Link back to the DialogueBase asset this graph represents
        [SerializeField] public string dialogueBaseGuid;

        public override void OnGraphChanged(GraphLogger logger)
        {
            base.OnGraphChanged(logger);
            // 1) Run node actions triggered via options
            //FPDialogueGraphCommands.ProcessNodeActions(this);

            // 2) Validation messages
            foreach(var c in GetNodes().OfType<FPCombineNodes>())
            {
                c.DefineNode();
            }
            FPDialogueGraphValidation.Run(this, logger);
        }
    }
}
