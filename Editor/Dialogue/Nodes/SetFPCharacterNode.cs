namespace FuzzPhyte.Dialogue.Editor
{
    using UnityEngine;
    using FuzzPhyte.Utility;
    using System;
    using Unity.GraphToolkit.Editor;
    [UseWithGraph(typeof(FPDialogueGraph))]
    [Serializable]
    internal class SetFPCharacterNode:FPVisualNode
    {
        GameObject headNode;
        public override void SetupIndex(string passedName)
        {
            this.name = passedName;
        }
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<bool>(FPDialogueGraphValidation.GETDATAFILE)
                .WithDisplayName("Use Character File?")
                .WithTooltip("If you want to override the local data with the file")
                .WithDefaultValue(true);
            context.AddOption<string>(FPDialogueGraphValidation.GAMEOBJECT_ID)
                .WithDisplayName("GameObject Binding Id")
                .WithDefaultValue(string.Empty);
        }
        public bool TryGetHeadBindingId(out string id) => TryGetOptionValue<string>(FPDialogueGraphValidation.GAMEOBJECT_ID, out id);
        /// <summary>
        /// Defines the output for the node.
        /// </summary>
        /// <param name="context">The scope to define the node.</param>
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddOutputPort<SetFPCharacterNode>(FPDialogueGraphValidation.PORT_ACTOR)
                .WithDisplayName("Character Out:")
                .WithConnectorUI(PortConnectorUI.Circle)
                .Build();
            context.AddInputPort<FP_Character>(FPDialogueGraphValidation.PORT_CHARACTER_DATA)
                .WithDisplayName("Character:")
                .Build();
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
            context.AddInputPort<FP_Theme>(FPDialogueGraphValidation.ACTOR_THEME)
                .WithDefaultValue((FP_Theme) null) 
                .WithDisplayName("Theme:")
                .Build();
        }
        
    }
}
