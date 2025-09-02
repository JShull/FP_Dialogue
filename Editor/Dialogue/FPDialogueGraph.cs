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

    [UseWithGraph(typeof(FPDialogueGraph))]
    [Serializable]
    public class EntryNode : FPVisualNode
    {
        
        protected override void OnDefinePorts(IPortDefinitionContext ports)
        {
            ports.AddOutputPort(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME)
               .WithDataType(typeof(FPVisualNode))
               .WithDisplayName(string.Empty)
               .WithConnectorUI(PortConnectorUI.Arrowhead)
               .Build();
            ports.AddInputPort(FPDialogueGraphValidation.MAIN_PORT_TIMELINE)
                .WithDataType(typeof(TimelineAsset))
                .WithDisplayName("From Timeline:")
                .WithConnectorUI(PortConnectorUI.Circle)
                .Build();
        }
    }
    [UseWithGraph(typeof(FPDialogueGraph))]
    [Serializable]
    public class ExitNode : FPVisualNode
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME)
                .WithDataType(typeof(FPVisualNode))
                .WithDisplayName(string.Empty)
                .WithConnectorUI (PortConnectorUI.Arrowhead)
                .Build();
            context.AddInputPort(FPDialogueGraphValidation.MAIN_PORT_TIMELINE)
                .WithDataType(typeof(TimelineAsset))
                .WithDisplayName("To Timeline:")
                .WithConnectorUI(PortConnectorUI.Circle)
                .Build();
        }
    }
}
