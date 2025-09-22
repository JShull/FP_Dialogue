namespace FuzzPhyte.Dialogue
{
    using FuzzPhyte.Utility;
    using UnityEngine;
    using System;
    using System.Collections.Generic;

    [Serializable]
    [CreateAssetMenu(fileName = "Dialogue Base", menuName = "FuzzPhyte/Dialogue/Dialogue Base", order = 5)]

    public class DialogueBase : FP_Data
    {
        [Tooltip("Our character reference - as well as the look and feel of the UI for the conversation via Character Theme")]
        public FP_Character Character;
        [Tooltip("This will then utilize the time values within the CC_OverlayNotification")]
        public bool AutoScrollConversation;
        public bool UseJustDialoguePanel = true; //if we want to use just the dialogue text without the character image or other elements
        public bool UseJustAudio = false; //if we want to still invoke our system but hide all of the UI elements except audio
        [Tooltip("List of our dialogue blocks, should be at least one")]
        public List<DialogueBlock> ConversationData;
        
        [Tooltip("List of possible next dialogues, should be empty if this is the end of the conversation.")]
        public List<DialogueBase> NextPotentialDialogues; 
        [Tooltip("Placeholder for extension purposes, should just keep this at Linear right now")]
        public FP_DialogueType TheDialogueType;

        //Called via a Delegate of some sorts is my guess when the user responds to a UI item - in most cases the UI
        //won't have more than 4 possible choices
        public DialogueBase ReturnDialogueBasedOnUserSelection(int userSelection)
        {
            if (userSelection < 0 || userSelection >= NextPotentialDialogues.Count)
            {
                Debug.LogError("Invalid user selection");
                return null;
            }
            return NextPotentialDialogues[userSelection];
        }
    }
}
