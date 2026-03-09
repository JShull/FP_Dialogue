namespace FuzzPhyte.Dialogue.Editor
{
    using FuzzPhyte.Utility;
    using System;
    using Unity.GraphToolkit.Editor;
    [UseWithGraph(typeof(FPDialogueGraph))]
    [Serializable]
    internal class SetFPWaitingNode:FPVisualNode
    {
        public override void SetupIndex(string passedName)
        {
            this.name = passedName;
        }
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<int>(FPDialogueGraphValidation.USER_NUMBER_OPTIONS)
                .WithDisplayName("Number of Random Responses?")
                .WithDefaultValue(2)
                .Delayed();
            context.AddOption<bool>(FPDialogueGraphValidation.GO_WORLD_LOCATION)
                .WithDisplayName("Use World Locations?")
                .WithDefaultValue(false)
                .Delayed();
            context.AddOption<FP_Data>(FPDialogueGraphValidation.FP_DATA_TAG)
                .WithDisplayName("Matching Event Requirement:")
                .WithDefaultValue(null)
                .Delayed();
        }
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            var numPromptCount = GetNodeOptionByName(FPDialogueGraphValidation.USER_NUMBER_OPTIONS);
            numPromptCount.TryGetValue<int>(out var numPrompts);
            context.AddInputPort<FPVisualNode>(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME)
                .WithDisplayName("Flow In")
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
            context.AddInputPort<SetFPCharacterNode>(FPDialogueGraphValidation.PORT_ACTOR)
                .WithDisplayName("Character In:")
                .WithConnectorUI(PortConnectorUI.Circle)
                .Build();
            for (var i = 0; i < numPrompts; i++)
            {
                context.AddInputPort<SetFPTalkNode>(FPDialogueGraphValidation.USER_PROMPTX_OP + i.ToString())
                    .WithDisplayName($"Response {i + 1}:")
                    .WithConnectorUI(PortConnectorUI.Circle)
                    .Build();
            }
            var useWorldLocation = GetNodeOptionByName(FPDialogueGraphValidation.GO_WORLD_LOCATION);
            bool useWorldLoc = false;
            if (useWorldLocation != null)
            {
                useWorldLocation.TryGetValue<bool>(out useWorldLoc);
                if (useWorldLoc)
                {
                    context.AddInputPort<string>(FPDialogueGraphValidation.USE_WORLD_LOCATION)
                        .WithDisplayName("World Location Name:")
                        .Build();
                }
            }
            context.AddOutputPort<SetFPTalkNode>(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME)
               .WithDisplayName("Flow Out")
               .Build();
        }
    }
}
