namespace FuzzPhyte.Dialogue.Editor
{
    using FuzzPhyte.Utility;
    using System;
    using Unity.GraphToolkit.Editor;
    using UnityEngine;

    [UseWithGraph(typeof(FPDialogueGraph))]
    [Serializable]
    internal class SetFPSinglePromptNode: FPVisualNode
    {
        public override void SetupIndex(string passedName)
        {
            this.name = passedName;
        }
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>(FPDialogueGraphValidation.GAMEOBJECT_ID)
               .WithDisplayName("GameObject Binding Id")
               .WithDefaultValue(string.Empty);
        }
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort<SetFPTalkNode>(FPDialogueGraphValidation.MAIN_TEXT)
                .WithDisplayName("User Prompt")
                .Build();
            context.AddInputPort<SetFPTalkNode>(FPDialogueGraphValidation.TRANSLATION_TEXT)
                .WithDisplayName("User Prompt Translation")
                .Build();
            context.AddInputPort<Sprite>(FPDialogueGraphValidation.PORT_ICON)
                .WithDisplayName("Icon")
                .Build();
            //context.AddInputPort<GameObject>(FPDialogueGraphValidation.GO_WORLD_LOCATION)
            //    .WithDisplayName("Spawn Location")
            //    .Build();
            context.AddOutputPort<SetFPSinglePromptNode>(FPDialogueGraphValidation.USER_PROMPT_PORT)
                .WithDisplayName("Response Node?")
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
        }
    }
}
