namespace FuzzPhyte.Dialogue.Editor
{
    using Unity.GraphToolkit.Editor;
    using System;
    using UnityEngine.Timeline;
    [UseWithGraph(typeof(FPDialogueGraph))]
    [Serializable]
    internal class ExitNode : FPVisualNode
    {
        public override void SetupIndex(string passedName)
        {
            this.name = passedName;
        }
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>(FPDialogueGraphValidation.GAMEOBJECT_ID)
                .WithTooltip("Ref obj should have an Playable Director On it")
                .WithDisplayName("Playable Director Ref:")
                .WithDefaultValue(string.Empty);
        }
        protected override void OnDefinePorts(IPortDefinitionContext ports)
        {
            ports.AddInputPort(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME)
                .WithDataType(typeof(FPVisualNode))
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
            ports.AddInputPort(FPDialogueGraphValidation.MAIN_PORT_TIMELINE)
                .WithDataType(typeof(TimelineAsset))
                .WithDisplayName("To Timeline:")
                .WithConnectorUI(PortConnectorUI.Circle)
                .Build();
            ports.AddInputPort<RTTimelineDetails>(FPDialogueGraphValidation.MAIN_PORT_TIMELINEDETAILS)
                .WithDisplayName("Timeline Details:")
                .WithConnectorUI(PortConnectorUI.Circle)
                .Build();
        }
    }
}
