namespace FuzzPhyte.Dialogue.Editor
{
    using FuzzPhyte.Utility;
    using System;
    using Unity.GraphToolkit.Editor;
    using UnityEngine;

    [UseWithGraph(typeof(FPDialogueGraph))]
    [Serializable]
    internal class SetFPDialogueNode : FPVisualNode
    {
        public override void SetupIndex(string passedName)
        {
            this.name = passedName;
        }
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<bool>(FPDialogueGraphValidation.USER_WAIT_FOR_USER)
                .WithDefaultValue(false)
                .WithDisplayName("Wait for User?: ")
                .WithTooltip("Leave off to go to the next node upon finishing this node")
                .Build();
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
            context.AddOption<bool>(FPDialogueGraphValidation.USE_THREED_OBJECTS)
                 .WithDefaultValue(false)
                 .WithDisplayName("Use World Objects?: ")
                 .Delayed();
            context.AddOption<bool>(FPDialogueGraphValidation.USE_PREFABS)
                .WithDefaultValue(false)
                .WithDisplayName("Prefabs?")
                .Delayed();
            /*
           *  [Header("Animation Related Parameters")]
      public EmotionalState DialogueEmoState;
      public DialogueState OverallDialogueState;
      public MotionState DialogueMotionState;
           * */
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
            context.AddOutputPort<FPVisualNode>(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME)
               .WithDisplayName("Flow Out")
               .WithConnectorUI(PortConnectorUI.Arrowhead)
               .Build();
            context.AddInputPort<SetFPCharacterNode>(FPDialogueGraphValidation.PORT_ACTOR)
                .WithDisplayName("Character In:")
                .WithConnectorUI(PortConnectorUI.Circle)
                .Build();
            context.AddOutputPort<SetFPCharacterNode>(FPDialogueGraphValidation.PORT_ACTOR)
                .WithDisplayName("Character Out:")
                .WithConnectorUI(PortConnectorUI.Circle)
                .Build();
            context.AddInputPort<SetFPTalkNode>(FPDialogueGraphValidation.MAIN_TEXT)
                .WithDisplayName("Text to Speak")
                .Build();
            context.AddInputPort<SetFPTalkNode>(FPDialogueGraphValidation.TRANSLATION_TEXT)
                .WithDisplayName("Translation")
                .Build();
            var useThreeDOption = GetNodeOptionByName(FPDialogueGraphValidation.USE_THREED_OBJECTS);
            bool useThreeD = false;
            useThreeDOption.TryGetValue<bool>(out useThreeD);
            var usePrefabOption = GetNodeOptionByName(FPDialogueGraphValidation.USE_PREFABS);
            bool usePrefabs = false;
            usePrefabOption.TryGetValue<bool>(out usePrefabs);
            if(useThreeDOption!=null && useThreeD && usePrefabOption!=null && usePrefabs)
            {
                //need yes no context ports for the gameobject prefab asset
                context.AddInputPort<ExposedReference<GameObject>>(FPDialogueGraphValidation.RESPONSE_PREFAB_YES)
                    .WithDisplayName("Prefab Option 1:")
                    .Build();
                context.AddInputPort<ExposedReference<GameObject>>(FPDialogueGraphValidation.RESPONSE_PREFAB_NO)
                    .WithDisplayName("Prefab Option 2:")
                    .Build();
            }else if(useThreeDOption != null && useThreeD)
            {
                //need string name for world placement of yes and no
                context.AddInputPort<string>(FPDialogueGraphValidation.RESPONSE_WORLD_YES_LOCATION)
                    .WithDisplayName("Prefab Option 1:")
                    .Build();
                context.AddInputPort<string>(FPDialogueGraphValidation.RESPONSE_WORLD_NO_LOCATION)
                    .WithDisplayName("Prefab Option 2:")
                    .Build();
            }
        }
        
    }
}
