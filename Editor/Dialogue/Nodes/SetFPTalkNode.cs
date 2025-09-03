namespace FuzzPhyte.Dialogue.Editor
{
    using FuzzPhyte.Utility;
    using System;
    using Unity.GraphToolkit.Editor;
    using UnityEngine;

    [UseWithGraph(typeof(FPDialogueGraph))]
    [Serializable]
    public class SetFPTalkNode: FPVisualNode
    {
        //public const string LANG_NAME = "Language";
        public const string DIALOGUE_HEADER = "DialogueTitle";
       
       

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort<FP_Language>(FPDialogueGraphValidation.LANG_NAME)
               .WithDisplayName("Language:")
               .Build();
            context.AddInputPort<string>(DIALOGUE_HEADER)
                .WithDisplayName("Header Field:")
                .Build();
            context.AddInputPort<string>(FPDialogueGraphValidation.DIALOGUE)
                .WithDisplayName("Text:")
                .Build();
            context.AddInputPort<AudioClip>(FPDialogueGraphValidation.DIALOGUE_AUDIO_NAME)
                .WithDisplayName("Audio:")
                .Build();
            context.AddInputPort<AnimationClip>(FPDialogueGraphValidation.ANIM_BLEND_FACE)
                .WithDisplayName("Face Animation:")
                .Build();
            context.AddOutputPort<SetFPTalkNode>(FPDialogueGraphValidation.MAIN_TEXT)
                .WithDisplayName("Dialogue->")
                .Build();
        }
    }
}
