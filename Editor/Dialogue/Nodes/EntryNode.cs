namespace FuzzPhyte.Dialogue.Editor
{
    using Unity.GraphToolkit.Editor;
    using System;
    using UnityEngine.Timeline;
    [UseWithGraph(typeof(FPDialogueGraph))]
    [Serializable]
    public class EntryNode : FPVisualNode
    {
        public override void SetupIndex(string passedName)
        {
            this.name = passedName;
        }
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
           base.OnDefineOptions(context);
        }
        protected override void OnDefinePorts(IPortDefinitionContext ports)
        {
            AddOutputExecutionPorts(ports);
            ports.AddInputPort(FPDialogueGraphValidation.MAIN_PORT_TIMELINE)
                .WithDataType(typeof(TimelineAsset))
                .WithDisplayName("From Timeline:")
                .WithConnectorUI(PortConnectorUI.Circle)
                .Build();
        }
    }
}
