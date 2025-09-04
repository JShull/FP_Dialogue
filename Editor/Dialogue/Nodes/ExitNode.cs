namespace FuzzPhyte.Dialogue.Editor
{
    using Unity.GraphToolkit.Editor;
    using System;
    using UnityEngine.Timeline;
    [UseWithGraph(typeof(FPDialogueGraph))]
    [Serializable]
    public class ExitNode : FPVisualNode
    {
        public override void SetupIndex(string passedName)
        {
            this.name = passedName;
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME)
                .WithDataType(typeof(FPVisualNode))
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
            context.AddInputPort(FPDialogueGraphValidation.MAIN_PORT_TIMELINE)
                .WithDataType(typeof(TimelineAsset))
                .WithDisplayName("To Timeline:")
                .WithConnectorUI(PortConnectorUI.Circle)
                .Build();
        }
    }
}
