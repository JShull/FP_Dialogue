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

        /// <summary>
        /// Defines the output for the node.
        /// </summary>
        /// <param name="context">The scope to define the node.</param>
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort<FPVisualNode>(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME)
                .WithDisplayName("Flow In")
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();

            context.AddInputPort<SetFPCharacterNode>(FPDialogueGraphValidation.PORT_ACTOR)
                .WithDisplayName("Character In:")
                .WithConnectorUI(PortConnectorUI.Circle)
                .Build();
            context.AddInputPort<SetFPSinglePromptNode>(FPDialogueGraphValidation.USER_PROMPT_ONE)
                .WithDisplayName("Prompt A")
                .Build();
            context.AddInputPort<SetFPSinglePromptNode>(FPDialogueGraphValidation.USER_PROMPT_TWO)
                .WithDisplayName("Prompt B")
                .Build();
            context.AddInputPort<SetFPSinglePromptNode>(FPDialogueGraphValidation.USER_PROMPT_THREE)
                .WithDisplayName("Prompt C")
                .Build();
            context.AddInputPort<SetFPSinglePromptNode>(FPDialogueGraphValidation.USER_PROMPT_FOUR)
                .WithDisplayName("Prompt D")
                .Build();
            context.AddOutputPort<FPVisualNode>(FPDialogueGraphValidation.USER_PROMPT_ONE)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .WithDisplayName("If Prompt A:")
                .Build();
            context.AddOutputPort<FPVisualNode>(FPDialogueGraphValidation.USER_PROMPT_TWO)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .WithDisplayName("If Prompt B:")
                .Build();
            context.AddOutputPort<FPVisualNode>(FPDialogueGraphValidation.USER_PROMPT_THREE)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .WithDisplayName("If Prompt C:")
                .Build();
            context.AddOutputPort<FPVisualNode>(FPDialogueGraphValidation.USER_PROMPT_FOUR)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .WithDisplayName("If Prompt D:")
                .Build();
        }
    }
}
