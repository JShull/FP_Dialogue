namespace FuzzPhyte.Dialogue.Editor
{
    using UnityEngine;
    using FuzzPhyte.Utility;
    using System;
    using Unity.GraphToolkit.Editor;
    [UseWithGraph(typeof(FPDialogueGraph))]
    [Serializable]
    public class SetFPCharacterNode:FPVisualNode
    {
        /// <summary>
        /// Defines the output for the node.
        /// </summary>
        /// <param name="context">The scope to define the node.</param>
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            AddOutputActorPort(context);
            context.AddInputPort<string>(FPDialogueGraphValidation.ACTOR_NAME)
                .WithDisplayName("Name:")
                .Build();
            context.AddInputPort<FP_Gender>(FPDialogueGraphValidation.ACTOR_GENDER)
                .WithDisplayName("Gender:")
                .Build();
            context.AddInputPort<FP_Ethnicity>(FPDialogueGraphValidation.ACTOR_ETH)
                .WithDisplayName("Ethnicity:")
                .Build();
            context.AddInputPort<FP_Language>(FPDialogueGraphValidation.ACTOR_LANGUAGES_PRIMARY)
               .WithDisplayName("First Language:")
               .Build();
            context.AddInputPort<FP_Language>(FPDialogueGraphValidation.ACTOR_LANGUAGES_SECONDARY)
               .WithDisplayName("Second Language:")
               .Build();
            context.AddInputPort<FP_Language>(FPDialogueGraphValidation.ACTOR_LANGUAGES_TIERTIARY)
               .WithDisplayName("Tertiary Language:")
               .Build();
            context.AddInputPort<int>(FPDialogueGraphValidation.ACTOR_AGE)
                .WithDisplayName("Age:")
                .Build();
            context.AddInputPort<SkinnedMeshRenderer>(FPDialogueGraphValidation.ANIM_SKIN_MESHR)
                .WithDisplayName("Character SkinnedMR:")
                .Build();
            context.AddInputPort<FP_Theme>(FPDialogueGraphValidation.ACTOR_THEME)
                .WithDisplayName("Theme:")
                .Build();
        }

    }
}
