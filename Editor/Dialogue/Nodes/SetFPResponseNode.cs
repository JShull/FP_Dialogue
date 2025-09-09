namespace FuzzPhyte.Dialogue.Editor
{
    using FuzzPhyte.Utility;
    using System;
    using Unity.GraphToolkit.Editor;
    using UnityEngine;

    [UseWithGraph(typeof(FPDialogueGraph))]
    [Serializable]
    internal class SetFPResponseNode: FPVisualNode
    {
        public override void SetupIndex(string passedName)
        {
           this.name = passedName;
        }

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<int>(FPDialogueGraphValidation.USER_NUMBER_OPTIONS)
                .WithDisplayName("Number of Prompts?")
                .WithDefaultValue(4)
                .Delayed();
        }
        /// <summary>
        /// Defines the output for the node.
        /// </summary>
        /// <param name="context">The scope to define the node.</param>
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
                context.AddInputPort<SetFPSinglePromptNode>(FPDialogueGraphValidation.USER_PROMPTX_OP + i.ToString())
                    .WithDisplayName($"Prompt {i + 1}:")
                    .WithConnectorUI(PortConnectorUI.Circle)
                    .Build();
                context.AddOutputPort<FPVisualNode>(FPDialogueGraphValidation.USER_PROMPTX_OP +i.ToString())
                    .WithDisplayName($"If Prompt {i+1}:")
                    .WithConnectorUI(PortConnectorUI.Arrowhead)
                    .Build();
            }
        }
    }
}
