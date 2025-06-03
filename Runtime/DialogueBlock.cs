namespace FuzzPhyte.Dialogue
{
    using System.Collections.Generic;
    using UnityEngine;
    using FuzzPhyte.Utility;

    [CreateAssetMenu(fileName = "Dialogue Block", menuName = "FuzzPhyte/Dialogue/Dialogue Block", order = 6)]
    public class DialogueBlock : FP_Data
    {
        [Header("Dialogue Information")]
        public ConvoTranslation OriginalLanguage;
        [Space]
        public ConvoTranslation TranslatedLanguage;
        [Space]
        [Header("Animation Related Parameters")]
        public EmotionalState DialogueEmoState;
        public DialogueState OverallDialogueState;
        public MotionState DialogueMotionState;
        [Space]
        [Tooltip("Assume this is normally empty and usually only at the end of a conversation with a response prompt needed")]
        public List<DialogueResponse> PossibleUserResponses;
        [Header("Other Information")]
        [Space]
        [Tooltip("If we want this to be part of the top left running list of tasks")]
        public OverlayType OverlayTaskType;
        public float OverlayDuration;
        [Tooltip("If we want to delay showing the information, only works on type 'information'")]
        public float DelayBeforeOverlay;
        [Tooltip("Useful for having other media content but not required")]
        public GameObject MiscData;
        [Tooltip("If we want to display the text")]
        public bool DisplayTextPanels = true;
        
    }
}
