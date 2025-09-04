namespace FuzzPhyte.Dialogue.Editor
{
    using FuzzPhyte.Utility;
    using System;
    using Unity.GraphToolkit.Editor;

    [UseWithGraph(typeof(FPDialogueGraph))]
    [Serializable]
    public class SetFPDialogueNode : FPVisualNode
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
            AddInputExecutionPorts(context);
            AddOutputExecutionPorts(context);
            
            AddInputActorPort(context);
            AddOutputActorPort(context);

            context.AddInputPort<SetFPTalkNode>(FPDialogueGraphValidation.MAIN_TEXT)
                .WithDisplayName("Text to Speak")
                .Build();
            context.AddInputPort<SetFPTalkNode>(FPDialogueGraphValidation.TRANSLATION_TEXT)
                .WithDisplayName("Translation")
                .Build();
          
        }
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            //base.OnDefineOptions(context);
            context.AddOption<EmotionalState>(nameof(FPDialogueGraphValidation.ANIM_EMOTION_STATE))
                .WithDefaultValue(EmotionalState.Neutral)
                .WithDisplayName("Emotion:")
                .WithTooltip("The Emotion?")

                .Build();
            context.AddOption<DialogueState>(nameof(FPDialogueGraphValidation.ANIM_DIALOGUE_STATE))
                .WithDefaultValue(DialogueState.Normal)
                .WithDisplayName("Dialogue:")
                .Build();
            context.AddOption<MotionState>(nameof(FPDialogueGraphValidation.ANIM_MOTION_STATE))
                .WithDefaultValue(MotionState.Idle)
                .WithDisplayName("Motion:")
                .Build();
            /*
           *  [Header("Animation Related Parameters")]
      public EmotionalState DialogueEmoState;
      public DialogueState OverallDialogueState;
      public MotionState DialogueMotionState;
           * */
        }
    }
}
