namespace FuzzPhyte.Dialogue
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using FuzzPhyte.Utility;
    using UnityEditor.EditorTools;

    [CreateAssetMenu(fileName = "Dialogue Block", menuName = "FuzzPhyte/Dialogue/Dialogue Block", order = 6)]
    public class DialogueBlock : FP_Data
    {
        [TextArea(2,4)]
        public string DialogueText;
        [Tooltip("Language of the dialogue - for references later if needed")]
        public FP_Language DialogueLanguage;
        public FP_Audio DialogueAudio;
        public EmotionalState DialogueEmoState;
        public DialogueState OverallDialogueState;
        public MotionState DialogueMotionState;
        [Tooltip("Assume this is normally empty and usually only at the end of a conversation with a response prompt needed")]
        public List<DialogueUserResponse> PossibleUserResponses;
        [Header("Other Information")]
        [Space]
        [Tooltip("If we want this to be part of the top left running list of tasks")]
        public OverlayType OverlayTaskType;
        public float OverlayDuration;
        [Tooltip("If we want to delay showing the information, only works on type 'information'")]
        public float DelayBeforeOverlay;
        [Tooltip("Useful for having other media content but not required")]
        public GameObject MiscData;
        
    }
}
