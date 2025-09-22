namespace FuzzPhyte.Dialogue.Editor
{
    using Unity.GraphToolkit.Editor;
    using System;
    using UnityEngine.Timeline;
    [UseWithGraph(typeof(FPDialogueGraph))]
    [Serializable]
    internal class EntryNode : FPVisualNode
    {
        //public RTTimelineDetails TimeLineDetails;
        public override void SetupIndex(string passedName)
        {
            this.name = passedName;
        }
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>(FPDialogueGraphValidation.GRAPHID)
                .WithDisplayName("GraphID:")
                .WithDefaultValue(string.Empty)
                .WithTooltip("Unique value across all graphs in a scene")
                .Build();
        }
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddOutputPort<FPVisualNode>(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME)
               .WithDisplayName("Flow Out")
               .WithConnectorUI(PortConnectorUI.Arrowhead)
               .Build();
            context.AddInputPort<TimelineAsset>(FPDialogueGraphValidation.MAIN_PORT_TIMELINE)
                .WithDisplayName("Timeline Data:")
                .WithConnectorUI(PortConnectorUI.Circle)
                .Build();
            context.AddInputPort<RTTimelineDetails>(FPDialogueGraphValidation.MAIN_PORT_TIMELINEDETAILS)
                .WithDisplayName("Timeline Details:")
                .WithConnectorUI(PortConnectorUI.Circle)
                .Build();
        }
    }
}
