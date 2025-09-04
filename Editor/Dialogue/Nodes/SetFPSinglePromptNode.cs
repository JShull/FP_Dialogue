namespace FuzzPhyte.Dialogue.Editor
{
    using FuzzPhyte.Utility;
    using System;
    using Unity.GraphToolkit.Editor;
    using UnityEngine;

    [UseWithGraph(typeof(FPDialogueGraph))]
    [Serializable]
    public class SetFPSinglePromptNode: FPVisualNode
    {
        public override void SetupIndex(string passedName)
        {
            this.name = passedName;
        }
        //public const string PROMPT_ICON = "PromptIcon";
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
            context.AddInputPort<GameObject>(FPDialogueGraphValidation.GO_WORLD_LOCATION)
                .WithDisplayName("Spawn Location")
                .Build();
            context.AddOutputPort<SetFPSinglePromptNode>(FPDialogueGraphValidation.USER_PROMPT_PORT)
                .WithDisplayName("Response Node?")
                .Build();
        }
    }
}
