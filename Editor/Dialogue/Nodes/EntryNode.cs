namespace FuzzPhyte.Dialogue.Editor
{
    using Unity.GraphToolkit.Editor;
    using System;
    using UnityEngine.Timeline;
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
}
