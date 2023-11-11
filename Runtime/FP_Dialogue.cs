using System;
using System.Collections.Generic;
using UnityEngine;
using FuzzPhyte.Utility;
using FuzzPhyte.Utility.EDU;
using FuzzPhyte.Utility.Notification;

namespace FuzzPhyte.Dialogue
{
    /// <summary>
    /// this is a placeholder enum right now for when/if we decide to expand this dialogue model into a 
    /// branching based model
    /// </summary>
    
    [Serializable]
    [CreateAssetMenu(fileName = "DialogueData", menuName = "FuzzPhyte/Dialogue/Conversation", order = 4)]
    public class FP_Dialogue : FP_Notification
    {
        [Tooltip("Our base character reference")]
        public FP_Character Character;
        [Tooltip("This will then utilize the time values within the CC_OverlayNotification")]
        public bool AutoScrollConversation;
        public List<FP_OverlayNotification> ConversationData;
        [Tooltip("Placeholder for extension purposes, should just keep this at Linear right now")]
        public FP_DialogueType TheDialogueType;
        [Tooltip("Do we want to relocate the player upon starting?")]
        public bool RelocatePlayerStart;
        public FP_Location PlayerPositionPreDialogue;
        public FP_Camera PreDialogueCameraAdjustments;
        [Tooltip("Do we want to relocate the player upon completion?")]
        public bool RelocatePlayerEnd;
        public FP_Location PlayerPositionPostDialogue;
        [Tooltip("Location of World Object to be interacted with")]
        public FP_Location ItemPositionSpawn;
    }
}
